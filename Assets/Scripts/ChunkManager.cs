using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    public AStar aStar;

    public CaveVisualisor caveVisualisor;
    [System.NonSerialized] public Dictionary<Vector3Int, Chunk> chunkDic = new Dictionary<Vector3Int, Chunk>();
    [System.NonSerialized] public List<Vector3Int> activeChunk = new List<Vector3Int>();
    const int CHUNKSIZE = 8;
    private int _width, _height, _depth;
    public float scale;

    public ComputeShader computeShader;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _densityBuffer;
    private ComputeBuffer _countBuffer;

    public ComputeShader computeShaderMarchAll;
    private ComputeBuffer _vertexBufferAll;
    private ComputeBuffer _densityBufferAll;
    //private ComputeBuffer _countBufferTest;
    private ComputeBuffer _countBufferAll;
    //private ComputeBuffer _totalCount;


    private int _marchKernelIdx;
    private int _marchAllKernelIdx;

    int GlobalCount;


    Vector2Int[] uvTable = new Vector2Int[6]
    {
        new Vector2Int(0,1),
        new Vector2Int(1,0),
        new Vector2Int(0,0),
        new Vector2Int(0,1),
        new Vector2Int(1,1),
        new Vector2Int(1,0),
    };

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

    private void OnDrawGizmosSelected()
    {
        Vector3Int aloc = new Vector3Int(160, 152, 160);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    Vector3Int loc = new Vector3Int(aloc.x + i, aloc.y + j, aloc.z + k);

                    float noise = CaveGenerator.Instance._simplexNoise.GetNoise(loc.x, loc.y, loc.z);

                    if (noise > 0.66f)
                    {
                        Gizmos.color = UnityEngine.Color.red;
                        Gizmos.DrawSphere(new Vector3(loc.x / 4f, loc.y / 4f, loc.z / 4f), 0.1f);
                    }
                    else if (noise >= 0.33f && noise <= 0.66f)
                    {
                        Gizmos.color = UnityEngine.Color.yellow;
                        Gizmos.DrawSphere(new Vector3(loc.x / 4f, loc.y / 4f, loc.z / 4f), 0.1f);
                    }
                    else
                    {
                        Gizmos.color = UnityEngine.Color.green;
                        Gizmos.DrawSphere(new Vector3(loc.x / 4f, loc.y / 4f, loc.z / 4f), 0.1f);
                    }
                }
            }
        }
    }


    public IEnumerator CreateChunks(int xSize, int ySize, int zSize)
    {
        chunkDic = new Dictionary<Vector3Int, Chunk>();
        activeChunk = new List<Vector3Int>();

        _width = xSize;
        _height = ySize;
        _depth = zSize;
        for (int i = 0; i < xSize  ; i += CHUNKSIZE)
        {
            for (int j = 0; j < ySize ; j += CHUNKSIZE)
            {
                for (int k = 0; k < zSize ; k += CHUNKSIZE)
                {
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

        
        yield return StartCoroutine(MarchAll());

        /*
        foreach (Chunk c in chunkDic.Values)
        {
            c.BuildPaths();
        }
        */
    }

    public void BuildChunks(Chunk chunk)
    {
        _vertexBuffer = new ComputeBuffer(512 * 5, sizeof(float) * 9, ComputeBufferType.Append);
        _densityBuffer = new ComputeBuffer(729, sizeof(float));
        _countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        _densityBuffer.SetData(chunk.density);
        _vertexBuffer.SetCounterValue(0);
        computeShader.SetBuffer(_marchKernelIdx, "densityBuffer", _densityBuffer);
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

        _densityBuffer.Release();
        _vertexBuffer.Release();
        _countBuffer.Release();
    }

    IEnumerator MarchAll()
    {
        GlobalCount = 0;
        int size = 40; //5 each

        for (int iterX = 0; iterX < _width; iterX += size)
        {
            for (int iterY = 0; iterY < _height; iterY += size)
            {
                for (int iterZ = 0; iterZ < _depth; iterZ += size)
                {

                    int vSize = size * size * size;
                    int expandSize = (size + 1) * (size + 1) * (size + 1);

                    _vertexBufferAll = new ComputeBuffer(vSize * 5, sizeof(float) * 12, ComputeBufferType.Append);
                    _densityBufferAll = new ComputeBuffer(expandSize, sizeof(float));
                    _countBufferAll = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

                    //_countBufferTest = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
                    //_totalCount = new ComputeBuffer(vSize, sizeof(int), ComputeBufferType.Append);

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
                    //_totalCount.SetCounterValue(0);
                    computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "densityBuffer", _densityBufferAll);
                    computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "vertexBuffer", _vertexBufferAll);

                    Vector3 vec = new Vector3(iterX, iterY, iterZ);
                    computeShaderMarchAll.SetVector("pos", vec);
                    computeShaderMarchAll.SetFloat("terrain_surface", 0f);
                    computeShaderMarchAll.SetFloat("scale", scale);

                    //computeShaderMarchAll.SetBuffer(_marchAllKernelIdx, "totalCount", _totalCount);

                    computeShaderMarchAll.Dispatch(_marchAllKernelIdx, size / 8, size / 8, size / 8);

                    ComputeBuffer.CopyCount(_vertexBufferAll, _countBufferAll, 0);
                    int[] totalCountArr = new int[1];
                    _countBufferAll.GetData(totalCountArr);
                    int totalCount = totalCountArr[0];

                    /*
                    ComputeBuffer.CopyCount(_totalCount, _countBufferTest, 0);
                    int[] totalCountArr1 = new int[1];
                    _countBufferTest.GetData(totalCountArr1);

                    int wallCount = totalCountArr1[0];
                    int[] wallsData = new int[wallCount];
                    _totalCount.GetData(wallsData);

                    for (int wallIdx = 0; wallIdx < wallCount; wallIdx++)
                    {
                        aStar.UpdateGrid(wallsData[wallIdx], true);
                    }
                    */

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
                                Vector2[] uvs = new Vector2[idxList.Count * 3];
        

                                for (int jj = 0; jj < idxList.Count; jj++)
                                {
                                    int idx = jj * 3;

                                    vertices[idx] = trianglesData[idxList[jj]].vertA;
                                    vertices[idx + 1] = trianglesData[idxList[jj]].vertB;
                                    vertices[idx + 2] = trianglesData[idxList[jj]].vertC;

                                    triangles[idx] = idx;
                                    triangles[idx + 1] = idx + 1;
                                    triangles[idx + 2] = idx + 2;

                                    uvs[idx] = uvTable[idx % 6];
                                    uvs[idx + 1] = uvTable[(idx + 1) % 6];
                                    uvs[idx + 2] = uvTable[(idx + 2) % 6];
                                }

                                Mesh mesh = new Mesh();
                                mesh.vertices = vertices;
                                mesh.triangles = triangles;
                                mesh.uv = uvs;
                                mesh.RecalculateTangents();
                                mesh.RecalculateNormals();

                                Vector3Int chunkLoc = new Vector3Int(iterX + i * 8, iterY + j * 8, iterZ + k * 8);
                                Chunk chunk = chunkDic[chunkLoc];
                                chunk.BuildChunk(mesh);
                                //chunk.BuildExit();

                                GlobalCount++;
                                if (idxList.Count > 0)
                                {
                                    activeChunk.Add(chunkLoc);
                                }
                            }
                        }
                    }
                    CaveGenerator.Instance.generateProgress = GlobalCount / 64000f * 0.925f + 0.075f;
                    yield return null;

                    _densityBufferAll.Release();
                    _vertexBufferAll.Release();
                    _countBufferAll.Release();
                }
            }
        }
    }


    private void OnDestroy()
    {
        if (_densityBuffer != null)
        {
            _densityBuffer.Release();
            _densityBuffer = null;
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

        if (_densityBufferAll != null)
        {   
            _densityBufferAll.Release();
            _densityBufferAll = null;
        }
        if (_vertexBufferAll != null)
        {   
            _vertexBufferAll.Release();
            _vertexBufferAll = null;
        }
        if (_countBufferAll != null)
        {   
            _countBufferAll.Release();
            _countBufferAll = null;
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