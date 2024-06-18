using GK;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private CellularAutomata _cellularAutomata;
    [SerializeField] private CaveVisualisor _caveVisualisor;
    [SerializeField] private ChunkManager _chunkManager;
    [SerializeField] private ConvexHull _convexHull;
    private SimplexNoise _simplexNoise;

    [Header("Cave Data")]
    public Vector3Int startingPt;
    [System.NonSerialized] public float[] caveGrid;
    public List<Vector3> orePoints;
    public List<Vector3> flowerPoints;

    public ComputeShader noiseComputeShader;

    public RaycastHit spiderHit;


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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OreSpawn();
            Debug.Log("OreSpawn");
            FlowerSpawn();
            Debug.Log("FlowerSpawn");
        }
    }

    public void CaveGeneration() 
    {
        float curTimeOri = Time.realtimeSinceStartup;

        if (_randomSeed)
        {
            _seed = UnityEngine.Random.Range(1, 100000);
        }
        UnityEngine.Random.InitState(_seed);

        startingPt = new Vector3Int(width / 2, height / 2, depth / 2);

        Debug.Log("RAM: " + SystemInfo.systemMemorySize);
        Debug.Log("VRAM: " + SystemInfo.graphicsMemorySize);

        float curTimeBase = Time.realtimeSinceStartup;
        GrowAgent mainTunnelAgent = new GrowAgent(startingPt, 1000, 5);
        mainTunnelAgent.Walk();
        ExcavationAgent mainCaveAgent = new ExcavationAgent(startingPt, 1, 5);
        mainCaveAgent.Walk();
        Debug.Log("BaseCave time: " + (Time.realtimeSinceStartup - curTimeBase));
        
        float curTimeCA = Time.realtimeSinceStartup;
        _cellularAutomata.RunCSCA(width, height, depth);
        Debug.Log("CA time: " + (Time.realtimeSinceStartup - curTimeCA));

        float curTimeNoise = Time.realtimeSinceStartup;
        _simplexNoise = new SimplexNoise(width, height, depth, _seed, noiseComputeShader);
        Debug.Log("var0" + CaveGenerator.Instance.caveGrid[80]);
        _simplexNoise.GenerateNoise();
        //_simplexNoise.GenerateNoiseNoCS();
        Debug.Log("SimplexNoise time: " + (Time.realtimeSinceStartup - curTimeNoise));

        float curTimeMarch = Time.realtimeSinceStartup;
        _chunkManager.CreateChunks(width, height, depth);
        Debug.Log("MarchTime: " + (Time.realtimeSinceStartup - curTimeMarch));

        //Array.Clear(caveGrid, 0, caveGrid.Length);

        Debug.Log("TotalTime: " + (Time.realtimeSinceStartup - curTimeOri));
    }

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

    public void DigOre(Ray ray, RaycastHit hit)
    {
        Debug.Log("DigOre");

        _convexHull.UpdateOre(hit.point);
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
