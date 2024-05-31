using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    public CaveVisualisor caveVisualisor;
    public Dictionary<Vector3Int, Chunk> chunkDic = new Dictionary<Vector3Int, Chunk>();
    const int CHUNKSIZE = 8;
    private int _width, _height, _depth;

    public ComputeShader computeShader;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _triangleBuffer;
    private ComputeBuffer _densityBuffer;
    private ComputeBuffer _countBuffer;

    private int _marchKernelIdx;


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
    }

    public void CreateChunks(int xSize, int ySize, int zSize)
    {
        _width = xSize;
        _height = ySize;
        _depth = zSize;
        for (int i = 0; i < xSize - CHUNKSIZE ; i += CHUNKSIZE)
        {
            for (int j = 0; j < ySize - CHUNKSIZE; j += CHUNKSIZE)
            {
                for (int k = 0; k < zSize - CHUNKSIZE; k += CHUNKSIZE)
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
                                chunk.density[idx] = CaveGenerator.Instance.caveGrid[x + i, y + j, z + k];
                            }
                        }
                    }
                    if (chunkPos == new Vector3Int(0, 0, 0)) ;
                        BuildChunks(chunk);
                    chunkDic.Add(chunkPos, chunk);
                }
            }
        }
    }

    public void BuildChunks(Chunk chunk)
    {

        _vertexBuffer = new ComputeBuffer(512, sizeof(float) * 9, ComputeBufferType.Append);
        _densityBuffer = new ComputeBuffer(729, sizeof(float));
        _countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer _debugBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        computeShader.SetBuffer(_marchKernelIdx, "debugBuffer", _debugBuffer);

        _densityBuffer.SetData(chunk.density);
        _vertexBuffer.SetCounterValue(0);
        computeShader.SetBuffer(_marchKernelIdx, "densityBuffer", _densityBuffer);
        computeShader.SetBuffer(_marchKernelIdx, "vertexBuffer", _vertexBuffer);

        Vector3 vec = new Vector3(chunk.chunkPosition.x, chunk.chunkPosition.y, chunk.chunkPosition.z);
        computeShader.SetVector("pos", vec);
        computeShader.SetFloat("terrain_surface", 0f);

        computeShader.Dispatch(_marchKernelIdx, 1, 1, 1);


        if (vec == new Vector3(0, 0, 0))
        {
            var debugArray = new float[3];
            _debugBuffer.GetData(debugArray);

            for (int i = 0; i < 8; i++)
            {
                int bot = 0 * 81 + 0 * 9 + i;
                Debug.Log("bot num: " + chunk.density[bot]);
                int top = 0 * 81 + 1 * 9 + i;
                Debug.Log("top num: " + chunk.density[top]);
            }
        }

        /*
        foreach (var variable in debugArray)
        {
            Debug.Log("var" + variable);
        }
        */

        _debugBuffer.Release();


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
    private void OnDestroy()
    {
        _densityBuffer.Release();
        _vertexBuffer.Release();
        _countBuffer.Release();
        if (_densityBuffer != null) _densityBuffer.Dispose();
        if (_vertexBuffer != null) _vertexBuffer.Dispose();
        if (_triangleBuffer != null) _triangleBuffer.Dispose();
        if (_countBuffer != null) _countBuffer.Dispose();
    }

    public void UpdateChunks(List<Vector3Int> positionList)
    {
        List<Vector3Int> chunksToUpdate = new List<Vector3Int>();

        foreach (Vector3Int pos in positionList)
        {
            List<Vector3Int> chunkList = GetChunkPos(pos);

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
            caveVisualisor.UpdateMeshData(chunkDic[chunks]);
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
        int mod = updatePos % 8;

        if (mod != 0)
        {
            pos = (updatePos / 8) * 8 + 5;
        }
        else
        {
            pos = ((updatePos / 8) - 1) * 8 + 5;
        }

        return pos;
    }


    struct Triangle
    {
        public Vector3 vertA;
        public Vector3 vertB;
        public Vector3 vertC;
    }
}