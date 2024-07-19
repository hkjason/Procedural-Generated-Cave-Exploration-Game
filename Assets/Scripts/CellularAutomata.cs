using System.Collections;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    private CaveGenerator _caveGenerator;

    public ComputeShader computeShader;
    private int _CAKernelIdx;

    private ComputeBuffer _caveBuffer1;
    private ComputeBuffer _caveBuffer2;

    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;
        _CAKernelIdx = computeShader.FindKernel("CSCA");
    }

    public IEnumerator RunCSCA(int sizeX, int sizeY, int sizeZ)
    {
        _caveBuffer1 = new ComputeBuffer((_caveGenerator.width + 1) * (_caveGenerator.depth + 1) * (_caveGenerator.height + 1), sizeof(float));
        _caveBuffer2 = new ComputeBuffer((_caveGenerator.width + 1) * (_caveGenerator.depth + 1) * (_caveGenerator.height + 1), sizeof(float));

        _caveBuffer1.SetData(CaveGenerator.Instance.caveGrid);

        CaveGenerator.Instance.generateProgress = 0.0083f;
        yield return null;

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

        if (_caveBuffer1 != null)
        {              
            _caveBuffer1.Release();
        }
        if (_caveBuffer2 != null)
        {              
            _caveBuffer2.Release();
        }

        CaveGenerator.Instance.generateProgress = 0.05f;
        yield return null;
    }

    private void OnDestroy()
    {
        if (_caveBuffer1 != null)
        {
            _caveBuffer1.Release();
            _caveBuffer1 = null;
        }
        if (_caveBuffer2 != null)
        {              
            _caveBuffer2.Release();
            _caveBuffer2 = null;
        }
    }


    /*
    The code below are celluar automata without using compute shader
    Uncomment to use for testing
    * Add
    * using System.IO;
    * using System.Runtime.Serialization.Formatters.Binary;

    //Surivival condition: 1,4,8,11,13-26
    //Birth condition: 13-26
    //State: 5 (0-4) / 2
    //Moore neighborhood
    //https://softologyblog.wordpress.com/2019/12/28/3d-cellular-automata-3/

    private int _iterations = 4;
    private int[,,] _lastStateCaveGrid;
    private int[,,] _thisStateCaveGrid;

    public void RunCellularAutomata()
    {
        _thisStateCaveGrid = new int[_caveGenerator.width,_caveGenerator.height,_caveGenerator.depth];

        for (int i = 0; i < _caveGenerator.width; i++)
        {
            for (int j = 0; j < _caveGenerator.height; j++)
            {
                for (int k = 0; k < _caveGenerator.depth; k++)
                {
                    if (_caveGenerator.GetCave(i, j, k) > 0)
                    { _thisStateCaveGrid[i, j, k] = 1; }
                    else
                    { _thisStateCaveGrid[i, j, k] = -1; }
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
                            _thisStateCaveGrid[i, j, k] = 1;
                        }
                        else if (neighborCount >= 8)
                        {
                            if (iter <2)
                                _thisStateCaveGrid[i, j, k] = 1;
                        }
                        else
                        {
                            _thisStateCaveGrid[i, j, k] = - 1;
                        }
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
                    _caveGenerator.SetCave(i, j, k, _thisStateCaveGrid[i, j, k]);
                }
            }
        }
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

    */
}
