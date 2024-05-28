using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    public CaveVisualisor caveVisualisor;
    public Dictionary<Vector3Int, Chunk> chunkDic = new Dictionary<Vector3Int, Chunk>();

    public int chunkSize = 8;

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

    public void CreateChunks(int xSize, int ySize, int zSize)
    {
        for (int i = 1; i < xSize - chunkSize ; i += chunkSize)
        {
            for (int j = 1; j < ySize - chunkSize; j += chunkSize)
            {
                for (int k = 1; k < zSize - chunkSize; k += chunkSize)
                {
                    Vector3Int chunkPos = new Vector3Int(i + 4, j + 4, k + 4);

                    Chunk chunk = caveVisualisor.CreateMeshData(chunkPos);
                    
                    chunkDic.Add(chunkPos, chunk);
                }
            }
        }

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
}