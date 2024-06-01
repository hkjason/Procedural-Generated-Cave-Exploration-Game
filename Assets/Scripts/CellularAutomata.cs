using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Analytics;

public class CellularAutomata : MonoBehaviour
{
    [SerializeField] int _iterations;
    private CaveGenerator _caveGenerator;
    private int[,,] _lastStateCaveGrid;
    private int[,,] _thisStateCaveGrid;

    public ComputeShader computeShader;
    private int _CAKernelIdx;

    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;
        _CAKernelIdx = computeShader.FindKernel("CSCA");
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


                    if (i < 0 || j < 0 || k < 0 || i >= _caveGenerator.width || j >= _caveGenerator.width || k >= _caveGenerator.width)
                    {
                        continue;
                    }

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

    //Surivival condition: 1,4,8,11,13-26
    //Birth condition: 13-26
    //State: 5 (0-4) / 2
    //Moore neighborhood
    //https://softologyblog.wordpress.com/2019/12/28/3d-cellular-automata-3/
    public void RunCellularAutomata()
    {
        _thisStateCaveGrid = new int[_caveGenerator.width,_caveGenerator.height,_caveGenerator.depth];

        for (int i = 0; i < _caveGenerator.width; i++)
        {
            for (int j = 0; j < _caveGenerator.height; j++)
            {
                for (int k = 0; k < _caveGenerator.depth; k++)
                {
                    //Works when 0, 4 print if ==0
                    if (_caveGenerator.GetCave(i, j, k) > 0)
                    { _thisStateCaveGrid[i, j, k] = 1; }
                    else
                    { _thisStateCaveGrid[i, j, k] = 0; }
                }
            }
        }

        for (int iter = 0; iter < _iterations; iter++)
        {

            _lastStateCaveGrid = DeepClone(_thisStateCaveGrid);

            //Skip boundaries
            for (int i = 0; i < _caveGenerator.width ; i++)
            {
                for (int j = 0; j < _caveGenerator.height  ; j++)
                {
                    for (int k = 0; k < _caveGenerator.depth ; k++)
                    {

                        int neighborCount = GetNeighborCount(i, j, k);
                        if (neighborCount >= 13)
                        {
                            _thisStateCaveGrid[i, j, k]++;
                        }
                        else if (neighborCount >= 8)
                        {
                            if (iter <2)
                                _thisStateCaveGrid[i, j, k]++;
                        }
                        else
                        {
                            _thisStateCaveGrid[i, j, k]--;
                        }

                        
                        if (_thisStateCaveGrid[i, j, k] > 1)
                        {
                            _thisStateCaveGrid[i, j, k] = 1;
                        }
                        if (_thisStateCaveGrid[i, j, k] < 0)
                        {
                            _thisStateCaveGrid[i, j, k] = 0;
                        }
                        
                    }
                }
            }
            for (int i = 0; i < _caveGenerator.width; i++)
            {
                for (int j = 0; j < _caveGenerator.height; j++)
                {
                    for (int k = 0; k < _caveGenerator.depth; k++)
                    {
                        if (_thisStateCaveGrid[i, j, k] > 0)
                        { _caveGenerator.SetCave(i, j, k, 1f); }
                        else
                        { _caveGenerator.SetCave(i, j, k, -1f); }
                    }
                }
            }
        }
    }


    public void RunCSCA(int sizeX, int sizeY, int sizeZ)
    {
        ComputeBuffer _caveBuffer1 = new ComputeBuffer((_caveGenerator.width + 1) * (_caveGenerator.depth + 1) * (_caveGenerator.height + 1), sizeof(float));
        ComputeBuffer _caveBuffer2 = new ComputeBuffer((_caveGenerator.width + 1) * (_caveGenerator.depth + 1) * (_caveGenerator.height + 1), sizeof(float));

        //float[] flattenArray = new float[104 * 104 * 104];

        _caveBuffer1.SetData(CaveGenerator.Instance.caveGrid);
        
        computeShader.SetBuffer(_CAKernelIdx, "caveBuffer1", _caveBuffer1);
        computeShader.SetBuffer(_CAKernelIdx, "caveBuffer2", _caveBuffer2);
        computeShader.SetInt("size", _caveGenerator.width);

        computeShader.SetBool("expand", true);
        computeShader.SetBool("use1", true);
        computeShader.Dispatch(_CAKernelIdx, sizeX / 8, sizeY / 8, sizeZ / 8);
        computeShader.SetBool("use1", false);
        computeShader.Dispatch(_CAKernelIdx, sizeX / 8, sizeY / 8, sizeZ / 8);

        computeShader.SetBool("expand", false);
        computeShader.SetBool("use1", true);
        computeShader.Dispatch(_CAKernelIdx, sizeX / 8, sizeY / 8, sizeZ / 8);
        computeShader.SetBool("use1", false);
        computeShader.Dispatch(_CAKernelIdx, sizeX / 8, sizeY / 8, sizeZ / 8);

        _caveBuffer1.GetData(CaveGenerator.Instance.caveGrid);

        /*
        for (int i = 0; i < 104; i++)
        {
            for (int j = 0; j < 104; j++)
            {
                for (int k = 0; k < 104; k++)
                {
                    CaveGenerator.Instance.caveGrid[i, j, k] = flattenArray[i * 104 * 104 + j * 104 + k];
                }
            }
        }*/

        if (_caveBuffer1 != null)
        {              
            _caveBuffer1.Release();
        }
        if (_caveBuffer2 != null)
        {              
            _caveBuffer2.Release();
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
