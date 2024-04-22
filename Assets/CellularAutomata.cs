using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] int iterations;

    CaveGenerator caveGenerator;

    private int[,,] _lastStateCaveGrid;
    private int[,,] _thisStateCaveGrid;

    public GameObject cubePrefab;

    private List<GameObject> _prefabList;

    //temp
    public CaveVisualisor caveVisualisor;

    void Start()
    {
        caveGenerator = CaveGenerator.Instance;

        _prefabList = new List<GameObject>();
    }

    int GetNeighborCount(int locX, int locY, int locZ)
    {
        int neighborCount = 0;

        for (int i = locX - 1; i <= locX + 1; i++)
        {
            for (int j = locY - 1; j <= locY + 1; j++)
            {
                for (int k = locZ - 1; k <= locZ + 1; k++)
                {
                    if (i == locX && j == locY && k == locZ) continue;

                    if (_lastStateCaveGrid[i, j, k] > 0)
                    {
                        neighborCount++;
                    }

                    if (neighborCount >= 13) return neighborCount;
                }
            }
        }
        return neighborCount;
    }

    //Surivival condition: 1,4,8,11,13-26
    //Birth condition: 13-26
    //State: 5 (0-4)
    //Moore neighborhood
    //https://softologyblog.wordpress.com/2019/12/28/3d-cellular-automata-3/
    public void RunCellularAutomata()
    {
        _thisStateCaveGrid = new int[caveGenerator.width,caveGenerator.height,caveGenerator.depth];

        for (int i = 0; i < caveGenerator.width; i++)
        {
            for (int j = 0; j < caveGenerator.height; j++)
            {
                for (int k = 0; k < caveGenerator.depth; k++)
                {
                    //Works when 0, 4 print if ==0
                    if (caveGenerator.caveGrid[i, j, k] == true)
                    { _thisStateCaveGrid[i, j, k] = 0; }
                    else
                    { _thisStateCaveGrid[i, j, k] = 4; }
                }
            }
        }

        //Iter
        
        for (int iter = 0; iter < iterations; iter++)
        {

            _lastStateCaveGrid = DeepClone(_thisStateCaveGrid);

            //Skip boundaries
            for (int i = 1; i < caveGenerator.width -1; i++)
            {
                for (int j = 1; j < caveGenerator.height -1 ; j++)
                {
                    for (int k = 1; k < caveGenerator.depth -1; k++)
                    { 
                        int neighborCount = GetNeighborCount(i, j, k);
                        if (neighborCount >= 13 && neighborCount <= 26)
                        {
                            _thisStateCaveGrid[i, j, k]++;
                        }
                        else if (neighborCount == 1 || neighborCount == 4 || neighborCount == 8 || neighborCount == 11)
                        {
                            _thisStateCaveGrid[i, j, k]++;
                        }
                        else
                        {
                            _thisStateCaveGrid[i, j, k]--;
                        }

                        if (_thisStateCaveGrid[i, j, k] > 4) _thisStateCaveGrid[i, j, k] = 4;
                        else if (_thisStateCaveGrid[i, j, k] < 0) _thisStateCaveGrid[i, j, k] = 0;
                    }
                }
            }
            for (int i = 1; i < caveGenerator.width; i++)
            {
                for (int j = 1; j < caveGenerator.height; j++)
                {
                    for (int k = 1; k < caveGenerator.depth; k++)
                    {
                        if (_thisStateCaveGrid[i, j, k] > 0)
                        { caveGenerator.caveGrid[i, j, k] = true; }
                        else
                        { caveGenerator.caveGrid[i, j, k] = false; }
                    }
                }
            }

            caveVisualisor.CreateMeshData();
        }
        

        //non iter
        /*
        _lastStateCaveGrid = DeepClone(_thisStateCaveGrid);

        //Skip boundaries
        for (int i = 1; i < caveGenerator.width - 1; i++)
        {
            for (int j = 1; j < caveGenerator.height - 1; j++)
            {
                for (int k = 1; k < caveGenerator.depth - 1; k++)
                {
                    int neighborCount = GetNeighborCount(i, j, k);
                    if (neighborCount >= 13 && neighborCount <= 26)
                    {
                        _thisStateCaveGrid[i, j, k]++;
                    }
                    else if (neighborCount == 1 || neighborCount == 4 || neighborCount == 8 || neighborCount == 11)
                    {
                        _thisStateCaveGrid[i, j, k]++;
                    }
                    else
                    {
                        _thisStateCaveGrid[i, j, k]--;
                    }

                    if (_thisStateCaveGrid[i, j, k] > 4) _thisStateCaveGrid[i, j, k] = 4;
                    else if (_thisStateCaveGrid[i, j, k] < 0) _thisStateCaveGrid[i, j, k] = 0;
                }
            }
        }
        for (int i = 1; i < caveGenerator.width; i++)
        {
            for (int j = 1; j < caveGenerator.height; j++)
            {
                for (int k = 1; k < caveGenerator.depth; k++)
                {
                    if (_thisStateCaveGrid[i, j, k] > 0)
                    { caveGenerator.caveGrid[i, j, k] = true; }
                    else
                    { caveGenerator.caveGrid[i, j, k] = false; }
                }
            }
        }
        */

        caveVisualisor.CreateMeshData();
    }

    int[,,] DeepClone(int[,,] obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (int[,,])formatter.Deserialize(ms);
        }
    }

}
