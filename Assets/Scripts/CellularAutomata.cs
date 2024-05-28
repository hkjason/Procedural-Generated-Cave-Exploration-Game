using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] int iterations;

    CaveGenerator caveGenerator;

    private int[,,] _lastStateCaveGrid;
    private int[,,] _thisStateCaveGrid;

    void Start()
    {
        caveGenerator = CaveGenerator.Instance;
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

    int GetNeighborCountQuick(int locX, int locY, int locZ)
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

                    //if (neighborCount > 1) return 2;
                }
            }
        }
        return neighborCount;
    }

    int GetNeighborCountVM(int locX, int locY, int locZ)
    {
        int neighborCount = 0;

        if (_lastStateCaveGrid[locX - 1, locY, locZ] > 0) neighborCount++;
        if (_lastStateCaveGrid[locX + 1, locY, locZ] > 0) neighborCount++;
        if (_lastStateCaveGrid[locX, locY - 1, locZ] > 0) neighborCount++;
        if (_lastStateCaveGrid[locX, locY + 1, locZ] > 0) neighborCount++;
        if (_lastStateCaveGrid[locX, locY, locZ - 1] > 0) neighborCount++;
        if (_lastStateCaveGrid[locX, locY, locZ + 1] > 0) neighborCount++;

        if (neighborCount >= 2) return neighborCount;

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
                    if (caveGenerator.caveGrid[i, j, k] > 0)
                    { _thisStateCaveGrid[i, j, k] = 0; }
                    else
                    { _thisStateCaveGrid[i, j, k] = 4; }
                }
            }
        }

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
                        if (neighborCount >= 13)
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
                        { caveGenerator.caveGrid[i, j, k] = 1f; }
                        else
                        { caveGenerator.caveGrid[i, j, k] = -1f; }
                    }
                }
            }
        }

        CleanUp();
    }

    void CleanUp()
    {
        for (int i = 1; i < caveGenerator.width - 1; i++)
        {
            for (int j = 1; j < caveGenerator.height - 1; j++)
            {
                for (int k = 1; k < caveGenerator.depth - 1; k++)
                {
                    int neighborCount = GetNeighborCountQuick(i, j, k);

                    if (neighborCount <= 4)
                    {
                        caveGenerator.caveGrid[i, j, k] = -1f;
                    }
                }
            }
        }
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
