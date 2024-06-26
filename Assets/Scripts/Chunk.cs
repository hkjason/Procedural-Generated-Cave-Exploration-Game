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
    public Dictionary<(Vector3Int, Vector3Int), List<Vector3Int>> pathDic;
    public Dictionary<(Vector3Int, Vector3Int), int> costDic;

    public Dictionary<Vector3Int, List<Vector3Int>> exitDic;

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

        exitPoints = new List<Vector3Int>();
        pathDic = new Dictionary<(Vector3Int, Vector3Int), List<Vector3Int>>();
        costDic = new Dictionary<(Vector3Int, Vector3Int), int>();
        exitDic = new Dictionary<Vector3Int, List<Vector3Int>>();
    }

    HashSet<Vector3Int> openSet;
    List<Vector3Int> openList;

    public void BuildExit()
    {
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
            if (AStar.Instance.GetGrid(baseLoc) && AStar.Instance.TryGetGrid(baseLoc.x, baseLoc.y, baseLoc.z - 1))
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

                Vector3Int thisExitPoint = (new Vector3Int(x, y, z) / area.Count);
                List<Vector3Int> connections;
                Vector3Int neighbourLoc = chunkPosition + new Vector3Int(0, 0, -8);
                Vector3Int neighbourExit = thisExitPoint + new Vector3Int(0, 0, -1);

                if (!exitPoints.Contains(thisExitPoint))
                {
                    exitPoints.Add(thisExitPoint);
                    connections = new List<Vector3Int>();
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic.Add(thisExitPoint, connections);
                }
                else
                {
                    connections = exitDic[thisExitPoint];
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic[thisExitPoint] = connections;
                }


                Chunk neighbourChunk = ChunkManager.Instance.chunkDic[neighbourLoc];

                if (!neighbourChunk.exitPoints.Contains(neighbourExit))
                {
                    neighbourChunk.exitPoints.Add(neighbourExit);
                    connections = new List<Vector3Int>();
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    exitDic.Add(neighbourExit, connections);
                }
                else
                {
                    connections = neighbourChunk.exitDic[neighbourExit];
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    neighbourChunk.exitDic[neighbourExit] = connections;
                }
                
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
            if (AStar.Instance.GetGrid(baseLoc) && AStar.Instance.TryGetGrid(baseLoc.x, baseLoc.y - 1, baseLoc.z))
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

                Vector3Int thisExitPoint = (new Vector3Int(x, y, z) / area.Count);
                List<Vector3Int> connections;
                Vector3Int neighbourLoc = chunkPosition + new Vector3Int(0, -8, 0);
                Vector3Int neighbourExit = thisExitPoint + new Vector3Int(0, -1, 0);

                if (!exitPoints.Contains(thisExitPoint))
                {
                    exitPoints.Add(thisExitPoint);
                    connections = new List<Vector3Int>();
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic.Add(thisExitPoint, connections);
                }
                else
                {
                    connections = new List<Vector3Int>();
                    connections = exitDic[thisExitPoint];
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic[thisExitPoint] = connections;
                }
                Chunk neighbourChunk = ChunkManager.Instance.chunkDic[neighbourLoc];

                if (neighbourExit == new Vector3Int(119, 215, 143))
                {
                    foreach (var aa in neighbourChunk.exitPoints)
                    {
                        Debug.Log("epoints: " + aa);
                    }

                    foreach (var bb in neighbourChunk.exitDic.Keys)
                    {
                        Debug.Log("key: " + bb);
                    }

                    int countt = 0;
                    int countt2 = 0;
                    foreach (Chunk c in ChunkManager.Instance.chunkDic.Values)
                    {
                        countt += c.exitDic.Count;
                        countt2 += c.exitPoints.Count;

                        foreach (var ca in exitPoints)
                        {
                            if (!c.exitDic.ContainsKey(ca))
                            {
                                Debug.Log("not found: " + ca);
                                foreach (var ke in exitDic.Keys)
                                {
                                    Debug.Log("keys: " + ke);
                                }
                            }
                        }

                        foreach (var cb in exitDic.Keys)
                        {
                            if (!c.exitPoints.Contains(cb))
                            {
                                Debug.Log("not found b: " + cb);
                                foreach (var pt in exitPoints)
                                {
                                    Debug.Log("pt: " + pt);
                                }
                            }
                        }
                    }

                    Debug.Log("pt" + countt2);
                    Debug.Log("dic" + countt);
                }



                if (!neighbourChunk.exitPoints.Contains(neighbourExit))
                {
                    neighbourChunk.exitPoints.Add(neighbourExit);
                    connections = new List<Vector3Int>();
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    neighbourChunk.exitDic.Add(neighbourExit, connections);
                }
                else
                {
                    connections = neighbourChunk.exitDic[neighbourExit];
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    neighbourChunk.exitDic[neighbourExit] = connections;
                }

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
            if (AStar.Instance.GetGrid(baseLoc) && AStar.Instance.TryGetGrid(baseLoc.x - 1, baseLoc.y, baseLoc.z))
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

                Vector3Int thisExitPoint = (new Vector3Int(x, y, z) / area.Count);
                List<Vector3Int> connections;
                Vector3Int neighbourLoc = chunkPosition + new Vector3Int(-8, 0, 0);
                Vector3Int neighbourExit = thisExitPoint + new Vector3Int(-1, 0, 0);

                if (!exitPoints.Contains(thisExitPoint))
                {
                    exitPoints.Add(thisExitPoint);
                    connections = new List<Vector3Int>();
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic.Add(thisExitPoint, connections);
                }
                else
                {
                    connections = exitDic[thisExitPoint];
                    connections.Add(neighbourLoc);
                    connections.Add(neighbourExit);

                    exitDic[thisExitPoint] = connections;
                }


                Chunk neighbourChunk = ChunkManager.Instance.chunkDic[neighbourLoc];
                if (!neighbourChunk.exitPoints.Contains(neighbourExit))
                {
                    neighbourChunk.exitPoints.Add(neighbourExit);
                    connections = new List<Vector3Int>();
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    exitDic.Add(neighbourExit, connections);
                }
                else
                {
                    connections = neighbourChunk.exitDic[neighbourExit];
                    connections.Add(chunkPosition);
                    connections.Add(thisExitPoint);

                    neighbourChunk.exitDic[neighbourExit] = connections;
                }
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

                if (AStar.Instance.GetGrid(loc) && AStar.Instance.TryGetGrid(loc.x, loc.y, loc.z -1))
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

                if (AStar.Instance.GetGrid(loc) && AStar.Instance.TryGetGrid(loc.x, loc.y - 1, loc.z))
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

                if (AStar.Instance.GetGrid(loc) && AStar.Instance.TryGetGrid(loc.x - 1, loc.y, loc.z))
                {
                    fill.Add(loc);
                    fill.AddRange(FloodFillYZ(loc));
                }
            }
        }

        return fill;
    }

    public void BuildPaths()
    {
        HashSet<Vector3Int> pointsSet = new HashSet<Vector3Int>();

        for (int i = 0; i < exitPoints.Count; i++) 
        {
            pointsSet.Add(exitPoints[i]);
            for (int j = 0; j < exitPoints.Count; j++)
            {
                if (pointsSet.Contains(exitPoints[j])) continue;

                List<Vector3Int> path = AStar.Instance.PathFindBase(exitPoints[i], exitPoints[j], chunkPosition, out int cost);
                if (path != null)
                { 
                    pathDic.Add((exitPoints[i], exitPoints[j]), path);
                    costDic.Add((exitPoints[i], exitPoints[j]), cost);
                    path.Reverse();
                    pathDic.Add((exitPoints[j], exitPoints[i]), path);
                    costDic.Add((exitPoints[j], exitPoints[i]), cost);
                }
            }
        }
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
