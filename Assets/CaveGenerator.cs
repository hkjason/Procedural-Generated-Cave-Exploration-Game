using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public static CaveGenerator Instance { get; private set; }

    [SerializeField] public int width = 106;
    [SerializeField] public int height = 106;
    [SerializeField] public int depth = 106;

    [SerializeField] private int _seed;
    public bool randomSeed;

    public Vector3Int startingPt;

    public float[,,] caveGrid;

    public List<Vector3> orePoints;
    public List<Vector3> oreHitPoints;

    public List<Vector3> flowerPoints;
    public List<Vector3> flowerHitPoints;
    public List<Vector3> normalPoints;

    public List<GameObject> flowerPrefab;
    public List<GameObject> plantPrefab;

    [SerializeField] private CellularAutomata cellularAutomata;
    [SerializeField] private CaveVisualisor caveVisualisor;
    [SerializeField] private ChunkManager chunkManager;

    public LayerMask groundLayer;

    SimplexNoise simplexNoise;

    private void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.white;

        if (orePoints.Count > 0)
        {
            foreach (var orePoint in orePoints)
            {
                Gizmos.DrawWireSphere(orePoint, 1f);
            }
        }

        Gizmos.color = UnityEngine.Color.cyan;

        if (oreHitPoints.Count > 0)
        {
            foreach (var hitPoint in oreHitPoints)
            {
                Gizmos.DrawWireSphere(hitPoint, 0.2f);
            }
        }

        /*
        Gizmos.color = UnityEngine.Color.green;

        if (oreHitPoints.Count > 0)
        {
            foreach (var hitPoint in oreHitPoints)
            {
                Debug.Log(hitPoint);

                float xDown = Mathf.Floor(hitPoint.x);
                float xUp = Mathf.Ceil(hitPoint.x);
                float yDown = Mathf.Floor(hitPoint.y);
                float yUp = Mathf.Ceil(hitPoint.y);
                float zDown = Mathf.Floor(hitPoint.z);
                float zUp = Mathf.Ceil(hitPoint.z);

                Gizmos.DrawWireSphere(new Vector3(xDown, yDown, zDown), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xDown, yDown, zUp), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xDown, yUp, zDown), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xDown, yUp, zUp), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xUp, yDown, zDown), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xUp, yDown, zUp), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xUp, yUp, zDown), 0.3f);
                Gizmos.DrawWireSphere(new Vector3(xUp, yUp, zUp), 0.3f);

                break;
            }
        }
        */

        /*
        Gizmos.color = UnityEngine.Color.red;

        if (flowerHitPoints.Count > 0)
        {
            for (int i = 0; i < flowerHitPoints.Count; i++)
            {
                Gizmos.DrawLine(flowerPoints[i], flowerHitPoints[i]);
            }
        }

        Gizmos.color = UnityEngine.Color.white;

        if (flowerHitPoints.Count > 0)
        {
            for (int i = 0; i < flowerHitPoints.Count; i++)
            {
                Gizmos.DrawLine(flowerHitPoints[i], normalPoints[i]);
            }
        }
        */
    }

    private void Awake()
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
        caveGrid = new float[width, height, depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++) 
                {
                    caveGrid[i, j, k] = 1f;
                }
            }
        }

        orePoints = new List<Vector3>();
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
        if (randomSeed)
        {
            _seed = UnityEngine.Random.Range(1, 100000);
        }
        UnityEngine.Random.InitState(_seed);

        startingPt = new Vector3Int(width / 2, height / 2, depth / 2);

        GrowAgent mainTunnelAgent = new GrowAgent(startingPt, 200, 3);
        mainTunnelAgent.Walk();

        ExcavationAgent mainCaveAgent = new ExcavationAgent(startingPt, 1, 5);
        mainCaveAgent.Walk();

        Debug.Log("BaseCave");

        cellularAutomata.RunCellularAutomata();

        Debug.Log("CellularAutomata");

        simplexNoise = new SimplexNoise(width, height, depth, _seed);
        simplexNoise.GenerateNoise();

        chunkManager.CreateChunks(width, height, depth);
        Debug.Log("SimplexNoise");
    }


    void OreSpawn()
    {
        foreach (Vector3 orePoint in orePoints)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 raycastOrigin = orePoint;

                Vector3 raycastDirection = UnityEngine.Random.insideUnitSphere * 10f;

                RaycastHit hit;

                Debug.DrawRay(raycastOrigin, raycastDirection * 4f, UnityEngine.Color.red);
                if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 4f, groundLayer))
                {
                    oreHitPoints.Add(hit.point);
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

                if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 4f, groundLayer))
                {
                    Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90, 0, 0); ;

                    //Quaternion rotation = Quaternion.FromToRotation(hit.point, hit.normal);
                    if (hit.normal.y > 0)
                    {
                        int randomIdx = UnityEngine.Random.Range(0, flowerPrefab.Count);
                        Instantiate(flowerPrefab[randomIdx], hit.point - hit.normal * 0.05f, rotation);
                    }
                    else
                    {
                        int randomIdx = UnityEngine.Random.Range(0, plantPrefab.Count);
                        Instantiate(plantPrefab[randomIdx], hit.point - hit.normal * 0.05f, rotation);
                    }

                    flowerHitPoints.Add(hit.point);
                    normalPoints.Add(hit.point + hit.normal);
                    break;
                }
            }
        }
    }


    public void DigCave(Ray ray, RaycastHit hit)
    {
        Debug.Log("Hit: " + hit.point);
        Debug.Log("ray: " + ray.direction);

        Vector3 hitPos = hit.point;
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

        List<KeyValuePair<Vector3Int, float>> neighbourList = new List<KeyValuePair<Vector3Int, float>>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    if (caveGrid[x + i, y + j, z + k] < 0)
                    {
                        Vector3Int neighbourSpot = new Vector3Int(x + i, y + j, z + k);
                        float distance = Vector3.Distance(digSpot, neighbourSpot);

                        neighbourList.Add(new KeyValuePair<Vector3Int, float>(neighbourSpot, distance));
                    }
                }
            }
        }

        List<Vector3Int> updatedPoint = new List<Vector3Int>();

        for (int i = 0; i < Math.Min(5, neighbourList.Count); i++)
        {
            Vector3Int point = neighbourList[i].Key;
            caveGrid[point.x, point.y, point.z] = 1f * simplexNoise.GetNoise(point.x, point.y, point.z); 

            updatedPoint.Add(point);
        }

        chunkManager.UpdateChunks(updatedPoint);
    }

}
