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

    public float[,,] caveGrid;

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
        if (Input.GetKeyDown(KeyCode.S))
        {
            SimplexNoise simplexNoise = new SimplexNoise(width, height, depth, _seed);
            simplexNoise.GenerateNoise();
            caveVisualisor.CreateMeshData();
            Debug.Log("SimplexNoise");
        }
    }

    void CaveGeneration() 
    {
        if (randomSeed)
        {
            _seed = Random.Range(1, 100000);
        }
        Random.InitState(_seed);

        _startingPt = new Vector3Int(width / 2, height / 2, depth / 2);

        GrowAgent mainTunnelAgent = new GrowAgent(_startingPt, 500, 3);
        mainTunnelAgent.Walk();

        ExcavationAgent mainCaveAgent = new ExcavationAgent(_startingPt, 1, 5);
        mainCaveAgent.Walk();
    }
}
