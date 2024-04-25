using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CaveGenerator : MonoBehaviour
{
    public static CaveGenerator Instance { get; private set; }

    [SerializeField] public int width = 100;
    [SerializeField] public int height = 100;
    [SerializeField] public int depth = 100;

    [SerializeField] private int _seed;
    public bool randomSeed;

    private Vector3Int _startingPt;

    public bool[,,] caveGrid;

    [SerializeField] private CellularAutomata cellularAutomata;
    [SerializeField] private CaveVisualisor caveVisualisor;

    public GameObject prefab;

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
        caveGrid = new bool[width, height, depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++) 
                {
                    caveGrid[i, j, k] = true;
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaveGeneration();
            caveVisualisor.CreateMeshData();
            Debug.Log("Base Cave");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            cellularAutomata.RunCellularAutomata();
            caveVisualisor.CreateMeshData();
            Debug.Log("CA");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            /*
            PerlinNoise perlinNoise = new PerlinNoise(width, height, depth);
            perlinNoise.GenerateNoise(caveGrid);
            int temp = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        if (caveGrid[i, j, k] == true)
                        {
                            temp++;
                            //Instantiate(prefab, new Vector3(i, j, k), Quaternion.identity);
                        }
                    }
                }
            }
            Debug.Log(temp);
            */
            FastNoiseLite fastNoise = new FastNoiseLite();
            FastNoiseLite fastNoise1 = new FastNoiseLite();
            fastNoise.SetSeed(_seed);
            fastNoise.SetSeed(_seed + 1);


            float max = -10000f;
            float min = 10000f;
            float magnitude = 2f;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {

                        float noise = fastNoise.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);
                        float noise1 = fastNoise1.GetNoise((float)x * magnitude, (float)y * magnitude, (float)z * magnitude);

                        //blend noise
                        float blendFactor = 0.5f;
                        float combinedNoise = Mathf.Lerp(noise, noise1, blendFactor);

                        if (noise > -0.5f)
                        {
                            caveGrid[x, y, z] = false;
                        }
                    }
                }
            }
            Debug.Log(max);
            Debug.Log(min);


            caveVisualisor.CreateMeshData();

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        caveGrid[i, j, k] = Random.value > 0.5f;
                    }
                }
            }

            caveVisualisor.CreateMeshData();
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        caveGrid[width/2 + i, height/2 + j, depth/2 + k] = false;
                    }
                }
            }
            //caveGrid[width / 2 + 1, height / 2, depth / 2] = false;

            caveVisualisor.CreateMeshData();

        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            caveVisualisor.CreateMeshDataF();
        }
    }

    void CaveGeneration() 
    {
        if (randomSeed)
        {
            _seed = Random.Range(1, 100000);
        }
        Random.InitState(_seed);

        //_startingPt = new Vector3Int(Random.Range(0, width), Random.Range(0, depth), Random.Range(0, height));
        _startingPt = new Vector3Int(width / 2, height / 2, depth / 2);


        GrowAgent mainTunnelAgent = new GrowAgent(_startingPt, 500, 3);
        mainTunnelAgent.Walk();

        //for (int i = 0; i < 3; i++)
        //{
        //    TunnelAgent tunnelAgents = new TunnelAgent(_startingPt, 50, 1);
        //    tunnelAgents.Walk();
        //}

        ExcavationAgent mainCaveAgent = new ExcavationAgent(_startingPt, 1, 5);
        mainCaveAgent.Walk();

    }
}
