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

    public int _seed;
    public bool randomSeed;

    private Vector3Int _startingPt;

    public bool[,,] caveGrid;

    public GameObject cubePrefab;

    private List<GameObject> _prefabList;

    [SerializeField] private CellularAutomata cellularAutomata;
    [SerializeField] private CaveVisualisor caveVisualisor;

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

        _prefabList = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaveGeneration();
            caveVisualisor.CreateMeshData();
            //cellularAutomata.RunCellularAutomata();
            //CaveVisualisation();
            Debug.Log("Base Cave");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            cellularAutomata.RunCellularAutomata();
            Debug.Log("CA");
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


        TunnelAgent mainTunnelAgent = new TunnelAgent(_startingPt, 100, 3);
        mainTunnelAgent.Walk();

        for (int i = 0; i < 3; i++)
        {
            TunnelAgent tunnelAgents = new TunnelAgent(_startingPt, 50, 1);
            tunnelAgents.Walk();
        }
    }
}
