using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    public AStar aStar;

    public CaveVisualisor caveVisualisor;
    public Dictionary<Vector3Int, Chunk> chunkDic = new Dictionary<Vector3Int, Chunk>();
    const int CHUNKSIZE = 8;
    private int _width, _height, _depth;
    public float scale;

    public ComputeShader computeShader;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _densityBufferAll;
    private ComputeBuffer _countBuffer;

    public ComputeShader computeShaderMarchAll;

    private int _marchKernelIdx;
    private int _marchAllKernelIdx;

    int GlobalCount;

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

    private void Start()
    {
        _marchKernelIdx = computeShader.FindKernel("CSMarchingCube");
        _marchAllKernelIdx = computeShaderMarchAll.FindKernel("CSMarchingCubeAll");
        GlobalCount = 0;
    }

    public void CreateChunks(int xSize, int ySize, int zSize)
    {
        _width = xSize;
        _height = ySize;
        _depth = zSize;
        for (int i = 0; i < xSize  ; i += CHUNKSIZE)
        {
            for (int j = 0; j < ySize ; j += CHUNKSIZE)
            {
                for (int k = 0; k < zSize ; k += CHUNKSIZE)
                {
                    /*
                    Vector3Int chunkPos = new Vector3Int(i, j, k);

                    Chunk chunk = new Chunk(chunkPos);

                    Mesh mesh = caveVisualisor.CreateMeshData(chunkPos);

                    chunk.BuildChunk(mesh);
                    chunkDic.Add(chunkPos, chunk);
                    */
                    Vector3Int chunkPos = new Vector3Int(i, j, k);

                    Chunk chunk = new Chunk(chunkPos);

                    for (int x = 0; x <= CHUNKSIZE; x++)
                    {
                        for (int y = 0; y <= CHUNKSIZE; y++)
                        {
                            for (int z = 0; z <= CHUNKSIZE; z++)
                            {
                                int idx = x * 81 + y * 9 + z;
                                chunk.density[idx] = CaveGenerator.Instance.GetCave(x + i, y + j, z + k);
                            }
                        }
                    }
                    chunkDic.Add(chunkPos, chunk);
                }
            }
        }


        MarchAll();
        Debug.Log("GCount: " + GlobalCount);
    }

    public void BuildChunks(Chunk chunk)
    {
        _vertexBuffer = new ComputeBuffer(512 * 5, sizeof(float) * 9, ComputeBufferType.Append);
        _densityBufferAll = new ComputeBuffer(729, sizeof(float));
        _countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        _densityBufferAll.SetData(chunk.density);
        _vertexBuffer.SetCounterValue(0);
        computeShader.SetBuffer(_marchKernelIdx, "densityBuffer", _densityBufferAll);
        computeShader.SetBuffer(_marchKernelIdx, "vertexBuffer", _vertexBuffer);

        Vector3 vec = new Vector3(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z);
        computeShader.SetVector("pos", vec);
        computeShader.SetFloat("terrain_surface", 0f);
        computeShader.SetFloat("scale", scale);

        computeShader.Dispatch(_marchKernelIdx, 1, 1, 1);

        ComputeBuffer.CopyCount(_vertexBuffer, _countBuffer, 0);
        int[] totalCountArr = new int[1];
        _countBuffer.GetData(totalCountArr);
        int totalCount = totalCountArr[0];

        Triangle[] trianglesData = new Triangle[totalCount];
        
        _vertexBuffer.GetData(trianglesData);

        Vector3[] vertices = new Vector3[totalCount * 3];
        int[] triangles = new int[totalCount * 3];

        for (int i = 0; i < totalCount; i++)
        {
            int idx = i * 3;

            vertices[idx ] = trianglesData[i].vertA;
            vertices[idx + 1] = trianglesData[i].vertB;
            vertices[idx + 2] = trianglesData[i].vertC;

            triangles[idx] = idx;
            triangles[idx + 1] = idx + 1;
            triangles[idx + 2] = idx + 2;
        }
       
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();

        chunk.BuildChunk(mesh);

        _densityBufferAll.Release();
        _vertexBuffer.Release();
        _countBuffer.Release();
    }

    public void MarchAll()
    {
        int size = 40; //5 each

        for (int iterX = 0; iterX < _width; iterX += size)
        {
            for (int iterY = 0; iterY < _height; iterY += size)
            {
                for (int iterZ = 0; iterZ < _depth; iterZ += size)
                {
                    int vSize = size * size * size;
                    int expandSize = (size + 1) * (size + 1) * (size + 1);

                    ComputeBuffer _vertexBufferAll = new ComputeBuffer(vSize * 5, sizeof(float) * 12, ComputeBufferType.Append);
                    ComputeBuffer _densityBufferAll = new ComputeBuffer(expandSize, sizeof(float));
                    ComputeBuffer _countBufferAll = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

                    ComputeBuffer _countBufferTest = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
                    ComputeBuffer _totalCount = new ComputeBuffer(vSize, sizeof(int), ComputeBufferType.Append);

                    float[] dens = new float[expandSize];
                    for (int i = 0; i < size + 1; i++)
                    {
                        for (int j = 0; j < size + 1; j++)
                        {
                            for (int k = 0; k < size + 1; k++)
                            {
                                dens[i * (size + 1) * (size + 1) + j * (size + 1) + k] = CaveGenerator.Instance.GetCave(iterX + i, iterY + j, iterZ + k);
                            }
                        }
                    }

                    _densityBufferAll.SetData(dens);
                    //_densityBufferAll.SetData(CaveGenerator.Instance.caveGrid, locIdx, 0, 41 * 41 * 41);
                    _vertexBufferAll.SetCounterValue(0);
                    computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "densityBuffer", _densityBufferAll);
                    computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "vertexBuffer", _vertexBufferAll);

                    Vector3 vec = new Vector3(iterX, iterY, iterZ);
                    computeShaderMarchAll.SetVector("pos", vec);
                    computeShaderMarchAll.SetFloat("terrain_surface", 0f);
                    computeShaderMarchAll.SetFloat("scale", scale);

                    computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "totalCount", _totalCount);

                    computeShaderMarchAll.Dispatch(_marchAllKernelIdx, size / 8, size / 8, size / 8);

                    ComputeBuffer.CopyCount(_vertexBufferAll, _countBufferAll, 0);
                    int[] totalCountArr = new int[1];
                    _countBufferAll.GetData(totalCountArr);
                    int totalCount = totalCountArr[0];

                    ComputeBuffer.CopyCount(_totalCount, _countBufferTest, 0);
                    int[] totalCountArr1 = new int[1];
                    _countBufferTest.GetData(totalCountArr1);

                    Debug.Log("tca length: " + totalCountArr1[0]);
                    int wallCount = totalCountArr1[0];
                    int[] wallsData = new int[wallCount];
                    _totalCount.GetData(wallsData);

                    for (int wallIdx = 0; wallIdx < wallCount; wallIdx++)
                    {
                        aStar.UpdateGrid(wallsData[wallIdx], true);
                    }

                    TriangleWithPos[] trianglesData = new TriangleWithPos[totalCount];

                    _vertexBufferAll.GetData(trianglesData);

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                List<int> idxList = new List<int>();
                                Vector3 currPos = new Vector3(i, j, k);

                                for (int ii = 0; ii < totalCount; ii++)
                                {
                                    if (trianglesData[ii].triPos == currPos)
                                    {
                                        idxList.Add(ii);
                                    }
                                }

                                Vector3[] vertices = new Vector3[idxList.Count * 3];
                                int[] triangles = new int[idxList.Count * 3];

                                for (int jj = 0; jj < idxList.Count; jj++)
                                {
                                    int idx = jj * 3;

                                    vertices[idx] = trianglesData[idxList[jj]].vertA;
                                    vertices[idx + 1] = trianglesData[idxList[jj]].vertB;
                                    vertices[idx + 2] = trianglesData[idxList[jj]].vertC;

                                    triangles[idx] = idx;
                                    triangles[idx + 1] = idx + 1;
                                    triangles[idx + 2] = idx + 2;

                                    //GlobalCount += 3;
                                }

                                Mesh mesh = new Mesh();
                                mesh.vertices = vertices;
                                mesh.triangles = triangles;
                                mesh.RecalculateTangents();
                                mesh.RecalculateNormals();

                                Vector3Int chunkLoc = new Vector3Int(iterX + i * 8, iterY + j * 8, iterZ + k * 8);
                                Chunk chunk = chunkDic[chunkLoc];
                                chunk.BuildChunk(mesh);
                            }
                        }
                    }
                    _densityBufferAll.Release();
                    _vertexBufferAll.Release();
                    _countBufferAll.Release();


                    _totalCount.Release();
                    _countBufferTest.Release();
                }
            }
        }
    }


    private void OnDestroy()
    {
        if (_densityBufferAll != null)
        {
            _densityBufferAll.Release();
            _densityBufferAll = null;
        }
        if (_vertexBuffer != null)
        {
            _vertexBuffer.Release();
            _vertexBuffer = null;
        }
        if (_countBuffer != null)
        {
            _countBuffer.Release();
            _countBuffer = null;
        }
    }

    public void UpdateChunks(List<Vector3Int> positionList)
    {
        List<Vector3Int> chunksToUpdate = new List<Vector3Int>();

        foreach (Vector3Int pos in positionList)
        {
            List<Vector3Int> chunkList = GetChunkPosNew(pos);

            foreach (Vector3Int chunkPos in chunkList)
            {
                if (!chunksToUpdate.Contains(chunkPos))
                { 
                    chunksToUpdate.Add(chunkPos);
                }
            }
        }

        foreach (Vector3Int chunks in chunksToUpdate)
        {
            Debug.Log("name" + chunkDic[chunks].chunkObject.name);
            caveVisualisor.UpdateMeshData(chunkDic[chunks]);
        }
    }

    public void DigCS(List<Vector3Int> positionList)
    {
        List<Vector3Int> chunksToUpdate = new List<Vector3Int>();

        foreach (Vector3Int pos in positionList)
        {
            List<Vector3Int> chunkList = GetChunkPosNew(pos);

            foreach (Vector3Int chunkPos in chunkList)
            {
                if (!chunksToUpdate.Contains(chunkPos))
                {
                    chunksToUpdate.Add(chunkPos);
                }
            }
        }

        foreach (Vector3Int chunks in chunksToUpdate)
        {
            chunkDic[chunks].UpdateDensity();
            Debug.Log("name" + chunkDic[chunks].chunkObject.name);
            BuildChunks(chunkDic[chunks]);
        }
    }

    private List<Vector3Int> GetChunkPos(Vector3Int updatePos)
    {
        List<Vector3Int> chunkList = new List<Vector3Int>();
        int xPos, yPos, zPos;

        int modX = updatePos.x % 8;
        int modY = updatePos.y % 8;
        int modZ = updatePos.z % 8;

        if (modX != 1 && modY != 1 && modZ != 1)
        {
            xPos = GetUpdatePos(updatePos.x);
            yPos = GetUpdatePos(updatePos.y);
            zPos = GetUpdatePos(updatePos.z);

            chunkList.Add(new Vector3Int(xPos, yPos, zPos));
        }
        else if (updatePos.x == 1 || updatePos.y == 1 || updatePos.z == 1 || updatePos.x == CaveGenerator.Instance.width - 1 || updatePos.y == CaveGenerator.Instance.height - 1 || updatePos.z == CaveGenerator.Instance.depth - 1)
        {
            if (updatePos.x == 1 && updatePos.y == 1 && updatePos.z == 1)
            {
                chunkList.Add(new Vector3Int(5, 5, 5));
            }
            else if (updatePos.x == CaveGenerator.Instance.width - 1 && updatePos.y == CaveGenerator.Instance.height - 1 && updatePos.z == CaveGenerator.Instance.depth - 1)
            {
                chunkList.Add(new Vector3Int(CaveGenerator.Instance.width - 5, CaveGenerator.Instance.height - 5, CaveGenerator.Instance.depth - 5));
            }
            else if (updatePos.x == 1 && updatePos.y == 1)
            {
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(5, 5, zPos));
            }
            else if (updatePos.x == 1 && updatePos.z == 1)
            {
                yPos = GetUpdatePos(updatePos.y);
                chunkList.Add(new Vector3Int(5, yPos, 5));
            }
            else if (updatePos.y == 1 && updatePos.z == 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                chunkList.Add(new Vector3Int(xPos, 5, 5));
            }
            else if (updatePos.x == 1)
            {
                yPos = GetUpdatePos(updatePos.y);
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(5, yPos, zPos));
            }
            else if (updatePos.y == 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(xPos, 5, zPos));
            }
            else if (updatePos.z == 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                yPos = GetUpdatePos(updatePos.y);
                chunkList.Add(new Vector3Int(xPos, yPos, 5));
            }
            else if (updatePos.x == CaveGenerator.Instance.width - 1 && updatePos.y == CaveGenerator.Instance.height - 1)
            {
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(CaveGenerator.Instance.width - 5, CaveGenerator.Instance.height - 5, zPos));
            }
            else if (updatePos.x == CaveGenerator.Instance.width - 1 && updatePos.z == CaveGenerator.Instance.depth - 1)
            {
                yPos = GetUpdatePos(updatePos.y);
                chunkList.Add(new Vector3Int(CaveGenerator.Instance.width - 5, yPos, CaveGenerator.Instance.depth - 5));
            }
            else if (updatePos.y == CaveGenerator.Instance.height - 1 && updatePos.z == CaveGenerator.Instance.depth - 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                chunkList.Add(new Vector3Int(xPos, CaveGenerator.Instance.height - 5, CaveGenerator.Instance.depth - 5));
            }
            else if (updatePos.x == CaveGenerator.Instance.width - 1)
            {
                yPos = GetUpdatePos(updatePos.y);
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(CaveGenerator.Instance.width - 5, yPos, zPos));
            }
            else if (updatePos.y == CaveGenerator.Instance.height - 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                zPos = GetUpdatePos(updatePos.z);
                chunkList.Add(new Vector3Int(xPos, CaveGenerator.Instance.height - 5, zPos));
            }
            else if (updatePos.z == CaveGenerator.Instance.depth - 1)
            {
                xPos = GetUpdatePos(updatePos.x);
                yPos = GetUpdatePos(updatePos.y);
                chunkList.Add(new Vector3Int(xPos, yPos, CaveGenerator.Instance.depth - 5));
            }
        }
        else if (modX == 1 && modY == 1 && modZ == 1)
        {
            for (int i = -1; i < 2; i += 2)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    for (int k = -1; k < 2; k += 2)
                    {
                        chunkList.Add(new Vector3Int(updatePos.x + i * 4, updatePos.y + j * 4, updatePos.z + k * 4));
                    }
                }
            }
        }
        else if (modX == 1 && modY == 1)
        {
            zPos = GetUpdatePos(updatePos.z);
            for (int i = -1; i < 2; i += 2)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    chunkList.Add(new Vector3Int(updatePos.x + i * 4, updatePos.y + j * 4, zPos));
                }
            }
        }
        else if (modX == 1 && modZ == 1)
        {
            yPos = GetUpdatePos(updatePos.y);
            for (int i = -1; i < 2; i += 2)
            {
                for (int k = -1; k < 2; k += 2)
                {
                    chunkList.Add(new Vector3Int(updatePos.x + i * 4, yPos, updatePos.z + k * 4));
                }
            }
        }
        else if (modY == 1 && modZ == 1)
        {
            xPos = GetUpdatePos(updatePos.x);
            for (int j = -1; j < 2; j += 2)
            {
                for (int k = -1; k < 2; k += 2)
                {
                    chunkList.Add(new Vector3Int(xPos, updatePos.y + j * 4, updatePos.z + k * 4));
                }
            }
        }
        else if (modX == 1)
        {
            yPos = GetUpdatePos(updatePos.y);
            zPos = GetUpdatePos(updatePos.z);
            for (int i = -1; i < 2; i += 2)
            {
                chunkList.Add(new Vector3Int(updatePos.x + i * 4, yPos, zPos));
            }
        }
        else if (modY == 1)
        {
            xPos = GetUpdatePos(updatePos.x);
            zPos = GetUpdatePos(updatePos.z);
            for (int j = -1; j < 2; j += 2)
            {
                chunkList.Add(new Vector3Int(xPos, updatePos.y + j * 4, zPos));
            }
        }
        else if (modZ == 1)
        {
            xPos = GetUpdatePos(updatePos.x);
            yPos = GetUpdatePos(updatePos.y);
            for (int k = -1; k < 2; k += 2)
            {
                chunkList.Add(new Vector3Int(xPos, yPos, updatePos.z + k * 4));
            }
        }

        return chunkList;
    }

    private int GetUpdatePos(int updatePos)
    {
        int pos;
        //int mod = updatePos % 8;

        /*
        if (mod != 0)
        {
            pos = (updatePos / 8) * 8 + 5;
        }
        else
        {
            pos = ((updatePos / 8) - 1) * 8 + 5;
        }
        */
        pos = (updatePos / 8) * 8;


        return pos;
    }


    private List<Vector3Int> GetChunkPosNew(Vector3Int updatePos)
    {
        List<Vector3Int> chunkList = new List<Vector3Int>();
        int xPos, yPos, zPos;

        int modX = updatePos.x % 8;
        int modY = updatePos.y % 8;
        int modZ = updatePos.z % 8;

        if (modX != 0 && modY != 0 && modZ != 0)
        {
            xPos = GetUpdatePos(updatePos.x);
            yPos = GetUpdatePos(updatePos.y);
            zPos = GetUpdatePos(updatePos.z);

            chunkList.Add(new Vector3Int(xPos, yPos, zPos));
        }
        else if (modX == 0 && modY == 0 && modZ == 0)
        {
            for (int i = -1; i < 1; i ++)
            {
                for (int j = -1; j < 1; j ++)
                {
                    for (int k = -1; k < 1; k++)
                    {

                        chunkList.Add(new Vector3Int(updatePos.x + i * 8, updatePos.y + j * 8, updatePos.z + k * 8));
                    }
                }
            }
        }
        else if (modX == 0 && modY == 0)
        {
            zPos = GetUpdatePos(updatePos.z);
            for (int i = -1; i < 1; i ++)
            {
                for (int j = -1; j < 1; j ++)
                {
                    chunkList.Add(new Vector3Int(updatePos.x + i * 8, updatePos.y + j * 8, zPos));
                }
            }
        }
        else if (modX == 0 && modZ == 0)
        {
            yPos = GetUpdatePos(updatePos.y);
            for (int i = -1; i < 1; i++)
            {
                for (int k = -1; k < 1; k ++)
                {
                    chunkList.Add(new Vector3Int(updatePos.x + i * 8, yPos, updatePos.z + k * 8));
                }
            }
        }
        else if (modY == 0 && modZ == 0)
        {
            xPos = GetUpdatePos(updatePos.x);
            for (int j = -1; j < 1; j ++)
            {
                for (int k = -1; k < 1; k ++)
                {
                    chunkList.Add(new Vector3Int(xPos, updatePos.y + j * 8, updatePos.z + k * 8));
                }
            }
        }
        else if (modX == 0)
        {
            yPos = GetUpdatePos(updatePos.y);
            zPos = GetUpdatePos(updatePos.z);
            for (int i = -1; i < 1; i ++)
            {
                chunkList.Add(new Vector3Int(updatePos.x + i * 8, yPos, zPos));
            }
        }
        else if (modY == 0)
        {
            xPos = GetUpdatePos(updatePos.x);
            zPos = GetUpdatePos(updatePos.z);
            for (int j = -1; j < 1; j ++)
            {
                chunkList.Add(new Vector3Int(xPos, updatePos.y + j * 8, zPos));
            }
        }
        else if (modZ == 0)
        {
            xPos = GetUpdatePos(updatePos.x);
            yPos = GetUpdatePos(updatePos.y);
            for (int k = -1; k < 1; k ++)
            {
                chunkList.Add(new Vector3Int(xPos, yPos, updatePos.z + k * 8));
            }
        }

        return chunkList;
    }


    struct Triangle
    {
        public Vector3 vertA;
        public Vector3 vertB;
        public Vector3 vertC;
    }

    struct TriangleWithPos
    {
        public Vector3 vertA;
        public Vector3 vertB;
        public Vector3 vertC;
        public Vector3 triPos;
    }
}