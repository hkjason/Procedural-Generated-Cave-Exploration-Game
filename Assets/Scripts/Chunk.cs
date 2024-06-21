using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    public GameObject chunkObject;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;

    public Vector3Int chunkPosition;

    //Chunk Size 8x8x8
    //Point store 9x9x9 for marching cubes
    public float[] density;
    const int pointArrSize = 729;


    //For HPA*
    public List<Vector3Int> exitPoints;

    private Vector3Int[] xyTable = new Vector3Int[8]
    {
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0)
    };
    private Vector3Int[] xzTable = new Vector3Int[8]
    {
        new Vector3Int(-1, 0, -1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, -1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 0, 1)
    };
    private Vector3Int[] yzTable = new Vector3Int[8]
    {
        new Vector3Int(0, -1, -1),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, -1, 1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 1, 1)
    };

    public Chunk(Vector3Int pos)
    {
        chunkObject = new GameObject(pos.x + ", " + pos.y + ", " + pos.z);
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        chunkPosition = pos;

        chunkObject.transform.SetParent(ChunkManager.Instance.transform);

        chunkObject.layer = LayerMask.NameToLayer("Terrain");

        density = new float[pointArrSize];
    }

    HashSet<Vector3Int> openSet;
    List<Vector3Int> openList;

    public void BuildExit()
    {
        exitPoints = new List<Vector3Int>();
        Vector3Int end = chunkPosition + new Vector3Int(8, 8, 8);

        openSet = new HashSet<Vector3Int>();
        openList = new List<Vector3Int>();

        /////
        ///XY
        /////
        openSet = new HashSet<Vector3Int>();
        openList = new List<Vector3Int>();

        for (int x = chunkPosition.x; x < end.x; x++)
        {
            for (int y = chunkPosition.y; y < end.y; y++)
            {
                Vector3Int location = new Vector3Int(x, y, chunkPosition.z);
                openSet.Add(location);
                openList.Add(location);
            }
        }

        while (openSet.Count > 0)
        {
            Vector3Int baseLoc = openList[0];

            openSet.Remove(baseLoc);
            openList.RemoveAt(0);
            List<Vector3Int> area = FloodFillXY(baseLoc);
            if (AStar.Instance.GetGrid(baseLoc))
            {
                area.Add(baseLoc);
            }

            if (area.Count > 0)
            {
                int x = 0;
                int y = 0;
                int z = 0;

                foreach (Vector3Int location in area)
                {
                    x += location.x;
                    y += location.y;
                    z += location.z;
                }

                exitPoints.Add(new Vector3Int(x, y, z) / area.Count);
            }
        }

        /////
        ///XZ
        /////
        for (int x = chunkPosition.x; x < end.x; x++)
        {
            for (int z = chunkPosition.z; z < end.z; z++)
            {
                Vector3Int location = new Vector3Int(x, chunkPosition.y, z);
                openSet.Add(location);
                openList.Add(location);
            }
        }

        while (openSet.Count > 0)
        {
            Vector3Int baseLoc = openList[0];

            openSet.Remove(baseLoc);
            openList.RemoveAt(0);
            List<Vector3Int> area = FloodFillXZ(baseLoc);
            if (AStar.Instance.GetGrid(baseLoc))
            {
                area.Add(baseLoc);
            }

            if (area.Count > 0)
            {
                int x = 0;
                int y = 0;
                int z = 0;

                foreach (Vector3Int location in area) 
                {
                    x += location.x;
                    y += location.y;
                    z += location.z;
                }

                exitPoints.Add(new Vector3Int(x, y, z) / area.Count);
            }
        }
        /////
        ///YZ
        /////
        for (int y = chunkPosition.y; y < end.y; y++)
        {
            for (int z = chunkPosition.z; z < end.z; z++)
            {
                Vector3Int location = new Vector3Int(chunkPosition.x, y, z);
                openSet.Add(location);
                openList.Add(location);
            }
        }

        while (openSet.Count > 0)
        {
            Vector3Int baseLoc = openList[0];

            openSet.Remove(baseLoc);
            openList.RemoveAt(0);
            List<Vector3Int> area = FloodFillYZ(baseLoc);
            if (AStar.Instance.GetGrid(baseLoc))
            {
                area.Add(baseLoc);
            }

            if (area.Count > 0)
            {
                int x = 0;
                int y = 0;
                int z = 0;

                foreach (Vector3Int location in area)
                {
                    x += location.x;
                    y += location.y;
                    z += location.z;
                }

                exitPoints.Add(new Vector3Int(x, y, z) / area.Count);
            }
        }
    }

    public List<Vector3Int> FloodFillXY(Vector3Int aloc)
    {
        List<Vector3Int> fill = new List<Vector3Int>();

        foreach (Vector3Int location in xyTable)
        {
            Vector3Int loc = aloc + location;

            if (openSet.Contains(loc))
            {
                openSet.Remove(loc);
                openList.Remove(loc);

                if (AStar.Instance.GetGrid(loc))
                {
                    fill.AddRange(FloodFillXY(loc));
                    fill.Add(loc);
                }
            }
        }

        return fill;
    }

    public List<Vector3Int> FloodFillXZ(Vector3Int aloc)
    { 
        List<Vector3Int> fill = new List<Vector3Int>();

        foreach (Vector3Int location in xzTable) 
        {
            Vector3Int loc = aloc + location;

            if (openSet.Contains(loc))
            {
                openSet.Remove(loc);
                openList.Remove(loc);

                if (AStar.Instance.GetGrid(loc))
                { 
                    fill.Add(loc);
                    fill.AddRange(FloodFillXZ(loc));
                }
            }
        }

        return fill;
    }

    public List<Vector3Int> FloodFillYZ(Vector3Int aloc)
    {
        List<Vector3Int> fill = new List<Vector3Int>();

        foreach (Vector3Int location in yzTable)
        {
            Vector3Int loc = aloc + location;

            if (openSet.Contains(loc))
            {
                openSet.Remove(loc);
                openList.Remove(loc);

                if (AStar.Instance.GetGrid(loc))
                {
                    fill.Add(loc);
                    fill.AddRange(FloodFillYZ(loc));
                }
            }
        }

        return fill;
    }

    public void BuildChunk(Mesh mesh)
    {
        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;

        meshRenderer.material = Resources.Load<Material>("Materials/ForTesting");
    }

    public void UpdateChunk(Mesh mesh)
    {
        this.meshFilter.mesh = mesh;
        this.meshCollider.sharedMesh = mesh;
    }

    public void UpdateDensity()
    {
        for (int x = 0; x <= 8; x++)
        {
            for (int y = 0; y <= 8; y++)
            {
                for (int z = 0; z <= 8; z++)
                {
                    int idx = x * 81 + y * 9 + z;
                    density[idx] = CaveGenerator.Instance.GetCave(x + chunkPosition.x, y + chunkPosition.y, z + chunkPosition.z);
                }
            }
        }
    }
}
