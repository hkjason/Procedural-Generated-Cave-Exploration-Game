using System;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public static CaveGenerator Instance { get; private set; }

    [SerializeField] public int width = 100;
    [SerializeField] public int height = 100;
    [SerializeField] public int depth = 100;

    [SerializeField] private int _seed;
    public bool randomSeed;

    public Vector3Int startingPt;

    public float[,,] caveGrid;

    public List<Vector3> orePoints;
    public List<Vector3> hitPoints;

    [SerializeField] private CellularAutomata cellularAutomata;
    [SerializeField] private CaveVisualisor caveVisualisor;

    public GameObject prefab;

    public LayerMask groundLayer;

    SimplexNoise simplexNoise;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (orePoints.Count > 0)
        {
            foreach (var orePoint in orePoints)
            {
                Gizmos.DrawWireSphere(orePoint, 1f);
            }
        }

        Gizmos.color = Color.cyan;

        if (hitPoints.Count > 0)
        {
            foreach (var hitPoint in hitPoints)
            {
                Gizmos.DrawWireSphere(hitPoint, 1f);
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CaveGeneration();
            //caveVisualisor.CreateMeshData();
            Debug.Log("Base Cave");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            cellularAutomata.RunCellularAutomata();
            caveVisualisor.CreateMeshData();
            Debug.Log("CA");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            simplexNoise = new SimplexNoise(width, height, depth, _seed);
            simplexNoise.GenerateNoise();
            caveVisualisor.CreateMeshData();
            Debug.Log("SimplexNoise");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OreSpawn();
            Debug.Log("OreSpawn");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            caveGrid[44, 39, 40] = 1f;
            caveVisualisor.CreateMeshData();
            Debug.Log("Test");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            for (int i = 10; i < 90; i++)
            {
                for (int j = 10; j < 90; j++)
                {
                    caveGrid[i, 50, j] = -1f;
                }
            }
            caveVisualisor.CreateMeshData();
        }
    }

    void CaveGeneration() 
    {
        if (randomSeed)
        {
            _seed = UnityEngine.Random.Range(1, 100000);
        }
        UnityEngine.Random.InitState(_seed);

        startingPt = new Vector3Int(width / 2, height / 2, depth / 2);

        GrowAgent mainTunnelAgent = new GrowAgent(startingPt, 500, 3);
        mainTunnelAgent.Walk();

        ExcavationAgent mainCaveAgent = new ExcavationAgent(startingPt, 1, 5);
        mainCaveAgent.Walk();
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

                Debug.DrawRay(raycastOrigin, raycastDirection * 4f, Color.red);
                if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 4f, groundLayer))
                {
                    hitPoints.Add(hit.point);
                    break;
                }
            }
        }
    }


    public void DigCave(Vector3Int position)
    {
        caveGrid[position.x, position.y, position.z] = 1f;
        caveVisualisor.UpdateMeshData(position);
        Debug.Log("Dig");
    }

    public void DigCaveTest(Ray ray, RaycastHit hit)
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

        for (int i = 0; i < Math.Min(5, neighbourList.Count); i++)
        {
            Vector3Int point = neighbourList[i].Key;
            caveGrid[point.x, point.y, point.z] = 1f * simplexNoise.GetNoise(point.x, point.y, point.z);  
        }

        caveVisualisor.CreateMeshData();
    }

}
