using GK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveGenerator : MonoBehaviour
{
    public static CaveGenerator Instance { get; private set; }

    //Multiple of 8
    [Header("Size")]
    public int width;
    public int height;
    public int depth;

    [Header("Seed")]
    [SerializeField] private int _seed;
    [SerializeField] private bool _randomSeed;

    [Header("Layer")]
    [SerializeField] private LayerMask _terrainLayer;

    [Header("Prefab")]
    [SerializeField] private List<GameObject> _flowerPrefab;
    [SerializeField] private List<GameObject> _plantPrefab;
    
    [Header("Script References")]
    private GameManager _gameManager;
    [SerializeField] private CellularAutomata _cellularAutomata;
    [SerializeField] private CaveVisualisor _caveVisualisor;
    [SerializeField] private ChunkManager _chunkManager;
    [SerializeField] private ConvexHull _convexHull;
    public SimplexNoise _simplexNoise;

    [Header("Cave Data")]
    public Vector3Int startingPt;
    [System.NonSerialized] public float[] caveGrid;
    public List<Vector3> orePoints;
    public List<Vector3> flowerPoints;

    public ComputeShader noiseComputeShader;

    public RaycastHit spiderHit;

    List<Vector3Int> openList = new List<Vector3Int>();
    HashSet<Vector3Int> openSet = new HashSet<Vector3Int>();
    int fillCount = 0;

    List<Tuple<List<Vector3Int>, float>> chunkList = new List<Tuple<List<Vector3Int>, float>>();

    public List<Vector3> orePointsNew;

    public Player player;

    public float generateProgress;
    public bool isGen;
    public delegate void GenComplete();
    public event GenComplete OnGenComplete;

    public int progressInt = 0;

    Vector3Int[] nTable = {
        new Vector3Int(0, 0, 8),
        new Vector3Int(0, 0, -8),
        new Vector3Int(8, 0, 0),
        new Vector3Int(-8, 0, 0),
        new Vector3Int(0, 8, 0),
        new Vector3Int(0, -8, 0)
    };

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(new Vector3(0,0,0) /4, 1);
        Gizmos.DrawSphere(new Vector3(0, 0, height) /4, 1);
        Gizmos.DrawSphere(new Vector3(0, depth, 0) /4, 1);
        Gizmos.DrawSphere(new Vector3(0, depth, height) / 4, 1);
        Gizmos.DrawSphere(new Vector3(width, 0, 0) /4, 1);
        Gizmos.DrawSphere(new Vector3(width, 0, height) / 4, 1);
        Gizmos.DrawSphere(new Vector3(width, depth, 0)/ 4, 1);
        Gizmos.DrawSphere(new Vector3(width, depth, height)/4, 1);

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(orePoints[orePoints.Count-1], 0.05f);
        Gizmos.DrawSphere(spiderHit.point, 0.02f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(spiderHit.point, spiderHit.point + spiderHit.normal * 0.5f);

        Gizmos.color = Color.yellow;
        foreach (Vector3 vv in orePointsNew)
        {
            Gizmos.DrawSphere(vv, 0.5f);
        }
    }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        _gameManager = GameManager.Instance;
    }

    void Start()
    {
        caveGrid = new float[(width + 1) * (height + 1) * (depth + 1)];
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < depth + 1; z++)
                {
                    SetCave(x, y, z, -1f);
                }
            }
        }

        orePoints = new List<Vector3>();
        flowerPoints = new List<Vector3>();

        isGen = false;

        StartCoroutine(CaveGeneration());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(BackToMain());
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            player.Spawn(startingPt.x / 4, startingPt.y / 4, startingPt.z / 4);
        }
    }

    IEnumerator BackToMain()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(0);

        while (!operation.isDone)
        {
            Debug.Log("progress load 0" + operation.progress);

            yield return null;
        }
    }

    public List<Vector3Int> FloodFillList(Vector3Int start)
    {
        List<Vector3Int> fill = new List<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        queue.Enqueue(start);
        fillCount = 0;

        while (queue.Count > 0 && fillCount < 50)
        {
            Vector3Int aloc = queue.Dequeue();

            if (openSet.Contains(aloc))
            {
                openSet.Remove(aloc);
                openList.Remove(aloc);

                fill.Add(aloc);
                fillCount++;

                foreach (Vector3Int offset in nTable)
                {
                    Vector3Int neighbor = aloc + offset;
                    if (openSet.Contains(neighbor) && fillCount < 50)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return fill;
    }

    IEnumerator CaveGeneration() 
    {
        isGen = true;

        float curTimeOri = Time.realtimeSinceStartup;

        if (_randomSeed)
        {
            _seed = UnityEngine.Random.Range(1, 100000);
        }
        UnityEngine.Random.InitState(_seed);

        startingPt = new Vector3Int(width / 2, height / 2, depth / 2);

        float curTimeBase = Time.realtimeSinceStartup;
        GrowAgent mainTunnelAgent = new GrowAgent(startingPt, 1000, 5);
        mainTunnelAgent.Walk();
        ExcavationAgent mainCaveAgent = new ExcavationAgent(startingPt, 1, 5);
        mainCaveAgent.Walk();
        Debug.Log("BaseCave time: " + (Time.realtimeSinceStartup - curTimeBase));

        float curTimeCA = Time.realtimeSinceStartup;
        progressInt = 1;
        yield return StartCoroutine(_cellularAutomata.RunCSCA(width, height, depth));
        Debug.Log("CA time: " + (Time.realtimeSinceStartup - curTimeCA));

        float curTimeNoise = Time.realtimeSinceStartup;
        _simplexNoise = new SimplexNoise(width, height, depth, _seed, noiseComputeShader);
        progressInt = 2;
        yield return StartCoroutine(_simplexNoise.GenerateNoise());
        Debug.Log("SimplexNoise time: " + (Time.realtimeSinceStartup - curTimeNoise));


        float curTimeMarch = Time.realtimeSinceStartup;
        progressInt = 3;
        yield return StartCoroutine(_chunkManager.CreateChunks(width, height, depth));
        Debug.Log("MarchTime: " + (Time.realtimeSinceStartup - curTimeMarch));

        //Array.Clear(caveGrid, 0, caveGrid.Length);

        float curTimeDiff = Time.realtimeSinceStartup;
        DifficultyAreaGen();
        Debug.Log("DiffTime: " + (Time.realtimeSinceStartup - curTimeDiff));

        float curTimeOre = Time.realtimeSinceStartup;
        _convexHull.OreMeshGen();
        Debug.Log("OreMeshTime: " + (Time.realtimeSinceStartup - curTimeOre));

        Debug.Log("TotalTime: " + (Time.realtimeSinceStartup - curTimeOri));

        player.Spawn(startingPt.x / 4, startingPt.y / 4, startingPt.z / 4);
        
        isGen = false;
        if (OnGenComplete != null)
        {
            OnGenComplete();
        }
    }

    /*
    void OreSpawn()
    {
        for (int i = 0; i < orePoints.Count; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 raycastOrigin = orePoints[i] / 4;

                Vector3 raycastDirection = UnityEngine.Random.insideUnitSphere * 10f;

                RaycastHit hit;

                Debug.DrawRay(raycastOrigin, raycastDirection * 4f, UnityEngine.Color.red);
                if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 4f, _terrainLayer))
                {
                    orePoints[i] = (hit.point);

                    spiderHit = hit;
                    break;
                }
            }
        }
        
    }
    */

    void FlowerSpawn()
    { 
        foreach (Vector3 flowerPoint in flowerPoints)
        {
            for (int i = 0; i < 10; i++)
            { 
                Vector3 raycastOrigin = flowerPoint;

                Vector3 raycastDirection = UnityEngine.Random.insideUnitSphere * 10f;

                RaycastHit hit;

                if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 4f, _terrainLayer))
                {
                    Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0); ;

                    //Quaternion rotation = Quaternion.FromToRotation(hit.point, hit.normal);
                    if (hit.normal.y > 0)
                    {
                        int randomIdx = UnityEngine.Random.Range(0, _flowerPrefab.Count);
                        Instantiate(_flowerPrefab[randomIdx], hit.point - hit.normal * 0.05f, rotation);
                    }
                    else
                    {
                        int randomIdx = UnityEngine.Random.Range(0, _plantPrefab.Count);
                        Instantiate(_plantPrefab[randomIdx], hit.point - hit.normal * 0.05f, rotation);
                    }

                    break;
                }
            }
        }
    }

    void FlowerSpawn(List<Vector3Int> cList, int diffLevel)
    {
        int fCount = 0;
        int iter = 0;
        int totalFCount;
        bool isTop;
        switch (diffLevel)
        {
            case 0: 
                totalFCount = 8;
                isTop = true;
                break;
            case 1: totalFCount = 4;
                isTop = true;
                break;
            case 2: totalFCount = 2;
                isTop = false;
                break;
            default: totalFCount = 0;
                isTop = true;
                break;
        }

        while (fCount < totalFCount && iter <= 100)
        {
            if (diffLevel == 0)
            {
                isTop = UnityEngine.Random.value > 0.5f;
            }

            int randomIdx = UnityEngine.Random.Range(0, cList.Count);

            Vector3Int cLoc = cList[randomIdx];

            int randomX = UnityEngine.Random.Range(0, 8);
            int randomY = UnityEngine.Random.Range(0, 8);
            int randomZ = UnityEngine.Random.Range(0, 8);

            int tries = 0;
            while (tries < 5)
            {
                Vector3Int randLoc = cLoc + new Vector3Int(randomX, randomY, randomZ);

                if (GetCave(randLoc.x, randLoc.y, randLoc.z) > 0)
                {
                    bool added = false;

                    for (int j = 0; j < 5; j++)
                    {
                        Vector3 randOri = randLoc / 4;
                        Vector3 raycastDirection;
                        if (isTop)
                        {
                            raycastDirection = RandomRayTop();
                        }
                        else
                        {
                            raycastDirection = RandomRayBottom();
                        }

                        RaycastHit hit;

                        if (Physics.Raycast(randOri, raycastDirection, out hit, 4f, _terrainLayer))
                        {
                            Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0); ;

                            //Quaternion rotation = Quaternion.FromToRotation(hit.point, hit.normal);
                            if (isTop && hit.normal.y > 0)
                            {
                                int randomFlowerIdx = UnityEngine.Random.Range(0, _flowerPrefab.Count);
                                Instantiate(_flowerPrefab[randomFlowerIdx], hit.point - hit.normal * 0.05f, rotation);
                                tries = 5;
                                fCount++;
                                added = true;
                            }
                            else if (!isTop && hit.normal.y < 0)
                            {
                                int randomFlowerIdx = UnityEngine.Random.Range(0, _plantPrefab.Count);
                                Instantiate(_plantPrefab[randomFlowerIdx], hit.point - hit.normal * 0.05f, rotation);
                                tries = 5;
                                fCount++; 
                                added = true;
                            }

                            if (added)
                            {
                                break;
                            }
                        }
                    }
                }

                tries++;
                iter++;
            }
        }
    }

    Vector3 RandomRayTop()
    {
        Vector3 direction;
        do
        {
            direction = UnityEngine.Random.insideUnitSphere;
        } while (direction.y < 0);

        return direction.normalized * 10f;
    }

    Vector3 RandomRayBottom()
    {
        Vector3 direction;
        do
        {
            direction = UnityEngine.Random.insideUnitSphere;
        } while (direction.y >= 0);

        return direction.normalized * 10f;
    }

    void OreSpawn(List<Vector3Int> cList, int oreNum)
    {
        int oreCount = 0;
        int iter = 0;
        while (oreCount < oreNum && iter <= 20)
        {
            int randomIdx = UnityEngine.Random.Range(0, cList.Count);

            Vector3Int cLoc = cList[randomIdx];

            int randomX = UnityEngine.Random.Range(0, 8);
            int randomY = UnityEngine.Random.Range(0, 8);
            int randomZ = UnityEngine.Random.Range(0, 8);

            int tries = 0;
            while (tries < 5)
            {
                Vector3Int randLoc = cLoc + new Vector3Int(randomX, randomY, randomZ);

                if (GetCave(randLoc.x, randLoc.y, randLoc.z) > 0)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Vector3 randOri = randLoc / 4;
                        Vector3 raycastDirection = UnityEngine.Random.insideUnitSphere * 10f;

                        RaycastHit hit;

                        if (Physics.Raycast(randOri, raycastDirection, out hit, 4f, _terrainLayer))
                        {
                            orePointsNew.Add(hit.point);
                            tries = 5;
                            oreCount++;
                            break;
                        }
                    }
                }

                tries++;
                iter++;
            }
        }
    }

    public void DigOre(Ray ray, RaycastHit hit)
    {
        Debug.Log("DigOre");

        _convexHull.UpdateOre(hit);
    }

    public void DigCave(Ray ray, RaycastHit hit)
    {
        Debug.Log("Hit: " + hit.point * 4);
        Debug.Log("ray: " + ray.direction);
        Vector3 hitPos = hit.point * 4;
        float rayx = Mathf.Abs(ray.direction.x);
        float rayy = Mathf.Abs(ray.direction.y);
        float rayz = Mathf.Abs(ray.direction.z);

        if (rayx > rayy && rayx > rayz)
        {
            if (ray.direction.x > 0)
            {
                hitPos.x = Mathf.Ceil(hitPos.x);
            }
            else
            {
                hitPos.x = Mathf.Floor(hitPos.x);
            }
        }
        else if (rayy > rayx && rayy > rayz)
        {
            if (ray.direction.y > 0)
            {
                hitPos.y = Mathf.Ceil(hitPos.y);
            }
            else
            {
                hitPos.y = Mathf.Floor(hitPos.y);
            }
        }
        else if (rayz > rayx && rayz > rayy)
        {
            if (ray.direction.z > 0)
            {
                hitPos.z = Mathf.Ceil(hitPos.z);
            }
            else
            {
                hitPos.z = Mathf.Floor(hitPos.z);
            }
        }
        else
        {
            Debug.Log("No match");
        }


        Debug.Log("Dig:" + hitPos);

        int x = Mathf.RoundToInt(hitPos.x);
        int y = Mathf.RoundToInt(hitPos.y);
        int z = Mathf.RoundToInt(hitPos.z);
        Vector3 digSpot = new Vector3(x, y, z);

        Debug.Log("DigInt:" + digSpot);

        List<Vector3Int> updatedPoint = new List<Vector3Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (x + i < 0 || x + j < 0 || x + k < 0 || x + i > width -1 || x + j > height - 1 || x + k > depth -1)
                    { continue; }

                    if (GetCave(x + i, y + j, z + k) < 0)
                    {
                        Vector3Int point = new Vector3Int(x + i, y + j, z + k);
                        SetCave(point.x, point.y, point.z, 1f);
                        updatedPoint.Add(point);
                    }
                }
            }
        }
        _chunkManager.UpdateChunks(updatedPoint);
    }

    public void DigCaveNew(Ray ray, RaycastHit hit)
    {
        Debug.Log("Hit: " + hit.point * 4);
        Debug.Log("ray: " + ray.direction);
        Vector3 hitPos = hit.point * 4;
        float rayx = Mathf.Abs(ray.direction.x);
        float rayy = Mathf.Abs(ray.direction.y);
        float rayz = Mathf.Abs(ray.direction.z);

        if (rayx > rayy && rayx > rayz)
        {
            if (ray.direction.x > 0)
            {
                hitPos.x = Mathf.Ceil(hitPos.x);
            }
            else
            {
                hitPos.x = Mathf.Floor(hitPos.x);
            }
        }
        else if (rayy > rayx && rayy > rayz)
        {
            if (ray.direction.y > 0)
            {
                hitPos.y = Mathf.Ceil(hitPos.y);
            }
            else
            {
                hitPos.y = Mathf.Floor(hitPos.y);
            }
        }
        else if (rayz > rayx && rayz > rayy)
        {
            if (ray.direction.z > 0)
            {
                hitPos.z = Mathf.Ceil(hitPos.z);
            }
            else
            {
                hitPos.z = Mathf.Floor(hitPos.z);
            }
        }
        else
        {
            Debug.Log("No match");
        }


        Debug.Log("Dig:" + hitPos);

        int x = Mathf.RoundToInt(hitPos.x);
        int y = Mathf.RoundToInt(hitPos.y);
        int z = Mathf.RoundToInt(hitPos.z);
        Vector3 digSpot = new Vector3(x, y, z);

        Debug.Log("DigInt:" + digSpot);

        List<Vector3Int> updatedPoint = new List<Vector3Int>();
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                for (int k = -2; k <= 2; k++)
                {
                    if (x + i < 0 || x + j < 0 || x + k < 0 || x + i > width - 1 || x + j > height - 1 || x + k > depth - 1)
                    { continue; }

                    Vector3Int point = new Vector3Int(x + i, y + j, z + k);
                    updatedPoint.Add(point);
                }
            }
        }
        _chunkManager.DigCS(updatedPoint);
    }

    public void DifficultyAreaGen()
    {
        orePointsNew = new List<Vector3>();

        Debug.Log("total chunks" + _chunkManager.activeChunk.Count);

        Vector3Int midP = new Vector3Int(4, 4, 4);

        foreach (Vector3Int cLoc in _chunkManager.activeChunk)
        {
            float noise = _simplexNoise.GetNoise(cLoc.x / 8, cLoc.y / 8, cLoc.z / 8);
            openList.Add(cLoc);
            openSet.Add(cLoc);
        }

        while (openList.Count > 0)
        {
            List<Vector3Int> filledArea = FloodFillList(openList[0]);

            if (filledArea.Count > 25)
            {
                float totalNoise = 0;

                foreach (Vector3Int cLoc in filledArea)
                {
                    totalNoise += _simplexNoise.GetNoise(cLoc.x / 8, cLoc.y / 8, cLoc.z / 8);
                }

                totalNoise /= filledArea.Count;

                chunkList.Add(new Tuple<List<Vector3Int>, float>(filledArea, totalNoise));
            }
        }

        chunkList.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        if (chunkList.Count >= 20)
        {
            for (int i = 0; i < 10; i++)
            {
                OreSpawn(chunkList[i].Item1, 2);
                FlowerSpawn(chunkList[i].Item1, 2);
            }

            for (int j = 10; j < 20; j++)
            {
                OreSpawn(chunkList[j].Item1, 1);
                FlowerSpawn(chunkList[j].Item1, 1);
            }

            for (int k = 20; k < chunkList.Count; k++)
            {
                FlowerSpawn(chunkList[k].Item1, 0);
            }    
        }
        else
        {
            Debug.Log("Gen fail");
        }
    }


    Vector3Int GetLoc(int index)
    {
        int x, y, z;

        int xy = height * depth;

        // Extract x
        x = index / xy;
        int remainder = index % xy;

        // Extract y
        y = remainder / depth;

        // Extract z
        z = remainder % depth;

        return new Vector3Int(x, y, z);
    }

    public void SetCave(int x, int y, int z, float val)
    {
        caveGrid[x * height * depth + y * depth + z] = val;
    }

    public float GetCave(int x, int y, int z)
    {
        return caveGrid[x * height * depth + y * depth + z];
    }

    public void MultiplyCave(int x, int y, int z, float scale)
    {
        caveGrid[x * height * depth + y * depth + z] *= scale;
    }
}
