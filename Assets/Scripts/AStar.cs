using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private bool[,,] pointGrid;

    [Header("Size")]
    public int width;
    public int height;
    public int depth;

    List<Vector3Int> path;

    List<Vector3Int> path1;

    public LayerMask groundLayer;

    public Player player;

    public int testCount = 0;

    List<Vector3Int> chunkLocList = new List<Vector3Int>();

    public static AStar Instance { get; private set; }

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

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = UnityEngine.Color.red;
        if (path != null && path.Count > 0)
        {
            foreach (var p in path)
            {
                Gizmos.DrawSphere(new Vector3(p.x/4f, p.y/4f, p.z/4f), 0.1f);
            }
        }
        Gizmos.DrawRay(new Vector3(20.4239998f, 59.4669991f, 54.6850014f), Vector3.down);
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawSphere(new Vector3Int(1, 1, 1)/4, 0.3f);
        Gizmos.DrawSphere(new Vector3Int(width / 2, depth / 4, height / 3)/4, 0.3f);

        /*
        Gizmos.color = UnityEngine.Color.cyan;
        if (chunkLocList != null && chunkLocList.Count > 0)
        {
            foreach (Vector3Int val in chunkLocList)
            {
                List<Vector3Int> cEP = ChunkManager.Instance.chunkDic[val].exitPoints;

                foreach (Vector3Int loc in cEP)
                {
                    Gizmos.DrawSphere(new Vector3(loc.x / 4f, loc.y / 4f, loc.z / 4f), 0.1f);
                }
            }
        }*/

        Gizmos.color = UnityEngine.Color.cyan;
        if (path1 != null && path1.Count > 0)
        {
            foreach (var p in path1)
            {
                Gizmos.DrawSphere(new Vector3(p.x / 4f, p.y / 4f, p.z / 4f), 0.1f);
            }
        }
        Gizmos.DrawRay(new Vector3(20.4239998f, 59.4669991f, 54.6850014f), Vector3.down);
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawSphere(new Vector3Int(1, 1, 1) / 4, 0.3f);
        Gizmos.DrawSphere(new Vector3Int(width / 2, depth / 4, height / 3) / 4, 0.3f);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
           // AStar.Instance.PathFindBase(new Vector3Int(116,256,255), new Vector3Int(116,256,256), new Vector3Int (112), out int cost);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Chunk c1 = ChunkManager.Instance.chunkDic[new Vector3Int(80, 232, 216)];
            Chunk c2 = ChunkManager.Instance.chunkDic[new Vector3Int(72, 232, 216)];

            string pStr = new string("");
            for (int aa = 7; aa >= 0; aa--)
            {
                for (int bb = 0; bb < 8; bb++)
                {
                    pStr += Convert.ToInt32(GetGrid(new Vector3Int(80, 232 + aa, 216 + bb)));
                }
                pStr += "\n";
            }

            string pStr1 = new string("");
            for (int aa = 7; aa >= 0; aa--)
            {
                for (int bb = 0; bb < 8; bb++)
                {
                    pStr1 += Convert.ToInt32(GetGrid(new Vector3Int(79, 232 + aa, 216 + bb)));
                }
                pStr1 += "\n";
            }

            Debug.Log(pStr);
            Debug.Log(pStr1);




            Vector3 raycastOrigin = new Vector3(20.4239998f, 59.4669991f, 54.6850014f);

            Vector3 raycastDirection = Vector3.down;

            RaycastHit hit;

            if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, 2f, groundLayer))
            {
                Debug.Log("hitpoint:" + hit.point);
            }

            Vector3 hitVec = hit.point * 4;
            Vector3Int debugVec = new Vector3Int(Mathf.FloorToInt(hitVec.x), Mathf.FloorToInt(hitVec.y), Mathf.FloorToInt(hitVec.z));

            Debug.Log("hitpoint mul:" + debugVec);

            Debug.Log("ptGrid:" + pointGrid[debugVec.x, debugVec.y, debugVec.z]);

            Vector3Int playerPos = player.GetCurrentGridPos();

            Debug.Log("playerGrid:" + pointGrid[playerPos.x, playerPos.y, playerPos.z]);

            float pfTime = Time.realtimeSinceStartup;
            path = PathFind(debugVec, playerPos);
            Debug.Log("pathfindtime: " + (Time.realtimeSinceStartup - pfTime));
            Debug.Log(path.Count);

            chunkLocList = new List<Vector3Int>();
            foreach (Vector3Int aaa in path)
            {
                Vector3Int chunkPos = GetChunkPos(aaa);
                if (!chunkLocList.Contains(chunkPos))
                {
                    chunkLocList.Add(chunkPos);
                };
            }

            float hpfTime = Time.realtimeSinceStartup;
            path1 = HPASPathFind(debugVec, playerPos);
            Debug.Log("Hpathfindtime: " + (Time.realtimeSinceStartup - hpfTime));
            Debug.Log(path1.Count);

        }
    }


    private void Start()
    {
        pointGrid = new bool[width, depth, height];

        /*
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < depth; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    pointGrid[i, j, k] = true;
                }
            }
        }
        */

        path = new List<Vector3Int>();

        //path = PathFind(new Vector3Int(1, 1, 1), new Vector3Int(width / 2, depth / 4, height / 3));
        //Debug.Log("pCOunt" + path.Count);
    }

    public void UpdateGrid(int loc, bool val)
    {
        int x = loc / (320 * 320);
        int remainder = loc % (320 * 320);
        int y = remainder / 320;
        int z = remainder % 320;
        pointGrid[x, y, z] = val;
    }

    public List<Vector3Int> HPASPathFind(Vector3Int startLoc, Vector3Int endLoc)
    {
        Dictionary<Vector3Int, AStarNode> dict = new Dictionary<Vector3Int, AStarNode>();
        PriorityQueue openList = new PriorityQueue();
        HashSet<Vector3Int> closeList = new HashSet<Vector3Int>();

        //If both chunk equal return;

        ChunkManager cm = ChunkManager.Instance;
        Vector3Int startChunkPos = GetChunkPos(startLoc);

        foreach (Vector3Int exit in cm.chunkDic[startChunkPos].exitPoints)
        {
            int cost;

            if (PathFindBase(startLoc, exit, startChunkPos, out cost) != null)
            {
                AStarNode node = new AStarNode(exit);
                node.gCost = cost;
                node.hCost = CalDist(exit, endLoc);

                openList.Enqueue(node);
                dict.Add(exit, node);
            }
        }
        Vector3Int endChunkPos = GetChunkPos(endLoc);

        List<Vector3Int> testList = new List<Vector3Int>();
        int count = 0;

        while (openList.Count > 0)
        {
            count++;
            AStarNode currentNode = openList.Dequeue();
            testList.Add(currentNode.loc);
            Vector3Int currentChunkLoc = GetChunkPos(currentNode.loc);
            Chunk currentChunk = cm.chunkDic[currentChunkLoc];

            if (chunkLocList.Contains(currentChunkLoc))
            {
                chunkLocList.Remove(currentChunkLoc);
            }

            closeList.Add(currentNode.loc);


            for (int x = 0; x < currentChunk.exitDic[currentNode.loc].Count; x += 2)
            {

                Vector3Int neighbourChunkLoc = currentChunk.exitDic[currentNode.loc][x];
                Vector3Int neighbourLocation = currentChunk.exitDic[currentNode.loc][x + 1];


                if (closeList.Contains(neighbourLocation))
                {
                    continue;
                }
                
                AStarNode nextNode;

                
                if (dict.TryGetValue(neighbourLocation, out nextNode))
                {
                    if (currentNode.loc == new Vector3Int(116, 256, 256))
                    {
                        Debug.Log("PartAa");
                        Debug.Log("NextNode" + nextNode.loc);

                        foreach 
                    }

                    int nodeCost = currentNode.gCost + 100;
                    if (nodeCost < nextNode.gCost)
                    {
                        nextNode.gCost = nodeCost;
                        nextNode.parentNode = currentNode;
                        openList.UpdateItem(nextNode);
                    }
                }
                else
                {
                    nextNode = new AStarNode(neighbourLocation);

                    if (currentNode.loc == new Vector3Int(116, 256, 256))
                    {
                        Debug.Log("PartAb");
                        Debug.Log("NextNode" + nextNode.loc);
                    }

                    nextNode.gCost = currentNode.gCost + 100;
                    nextNode.hCost = CalDist(nextNode.loc, endLoc);
                    nextNode.parentNode = currentNode;
                    dict.Add(neighbourLocation, nextNode);
                }

                closeList.Add(neighbourLocation);

                if (neighbourChunkLoc == endChunkPos)
                {
                    List<Vector3Int> result;
                    result = PathFindBase(neighbourLocation, endLoc, neighbourChunkLoc, out int cost);
                    if (result != null)
                    {
                        Vector3Int startExit;

                        int countt = 0;
                        AStarNode testNode = nextNode;
                        
                        /*
                        while (testNode.parentNode != null)
                        {

                            Debug.Log(countt + "" + testNode.loc);
                            testNode = testNode.parentNode;
                            countt++;
                        }*/
                        


                        result.AddRange(BuildPathH(nextNode, out startExit));
                        result.AddRange(PathFindBase(startLoc, startExit, startChunkPos, out cost));
                        result.Reverse();
                        return result;
                    }
                }




                Chunk neighbourChunk = cm.chunkDic[neighbourChunkLoc];


                for (int y = 0; y < neighbourChunk.exitPoints.Count; y++)
                {
                    Vector3Int connectedExit = neighbourChunk.exitPoints[y];

                    if (closeList.Contains(connectedExit))
                    {
                        continue;
                    }

                    int cost;

                    if (neighbourChunk.costDic.TryGetValue((neighbourLocation, connectedExit), out cost))
                    {
                        AStarNode neighbourNode;
                        if (dict.TryGetValue(connectedExit, out neighbourNode))
                        {
                            if (nextNode.loc == new Vector3Int(116, 256, 256))
                            {
                                Debug.Log("PartBa");
                                Debug.Log("NextNode" + neighbourNode.loc);
                            }

                            int nodeCost = nextNode.gCost + cost;
                            if (nodeCost < neighbourNode.gCost)
                            {
                                neighbourNode.gCost = nodeCost;
                                neighbourNode.parentNode = nextNode;
                                openList.UpdateItem(neighbourNode);
                            }
                        }
                        else
                        {
                            if (nextNode.loc == new Vector3Int(116, 256, 256))
                            {
                                Debug.Log("PartBb");
                                Debug.Log("NextNode" + neighbourNode.loc);
                            }

                            neighbourNode = new AStarNode(connectedExit);
                            neighbourNode.gCost = currentNode.gCost + cost;
                            neighbourNode.hCost = CalDist(neighbourNode.loc, endLoc);
                            neighbourNode.parentNode = nextNode;
                            openList.Enqueue(neighbourNode);
                            dict.Add(connectedExit, neighbourNode);
                        }
                    }
                }
            }
        }

        return null;
    }

    public List<Vector3Int> PathFind(Vector3Int startLoc, Vector3Int endLoc)
    {
        Dictionary<Vector3Int, AStarNode> dict = new Dictionary<Vector3Int, AStarNode> ();
        PriorityQueue openList = new PriorityQueue();
        HashSet<Vector3Int> closeList = new HashSet<Vector3Int>();

        AStarNode aStarNode = new AStarNode(startLoc);
        openList.Enqueue(aStarNode);
        dict.Add(startLoc, aStarNode);
        while (openList.Count > 0)
        {
            AStarNode currentNode = openList.Dequeue();

            if (currentNode.loc == endLoc)
            {
                return BuildPath(startLoc, currentNode);
            }

            closeList.Add(currentNode.loc);


            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighbourLocation = new Vector3Int(currentNode.loc.x + x, currentNode.loc.y + y, currentNode.loc.z + z);

                        if (pointGrid[neighbourLocation.x, neighbourLocation.y, neighbourLocation.z] == false)
                        {
                            continue;
                        }

                        if (closeList.Contains(neighbourLocation))
                        {
                            continue;
                        }

                        AStarNode neighbourNode;
                        if (dict.TryGetValue(neighbourLocation, out neighbourNode))
                        {
                            int cost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            if (cost < neighbourNode.gCost)
                            {
                                neighbourNode.gCost = cost;
                                neighbourNode.parentNode = currentNode;
                                openList.UpdateItem(neighbourNode);
                            }
                        }
                        else
                        {
                            neighbourNode = new AStarNode(neighbourLocation);
                            neighbourNode.gCost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            neighbourNode.hCost = CalDist(neighbourNode.loc, endLoc);
                            neighbourNode.parentNode = currentNode;
                            openList.Enqueue(neighbourNode);
                            dict.Add(neighbourLocation, neighbourNode);
                        }
                    }
                }
            }
        }

        return null;
    }

    public List<Vector3Int> PathFindBase(Vector3Int startLoc, Vector3Int endLoc, Vector3Int chunkPos, out int totalCost)
    {
        Vector3Int end = chunkPos + new Vector3Int(8, 8, 8);

        Dictionary<Vector3Int, AStarNode> dict = new Dictionary<Vector3Int, AStarNode>();
        PriorityQueue openList = new PriorityQueue();
        HashSet<Vector3Int> closeList = new HashSet<Vector3Int>();

        AStarNode aStarNode = new AStarNode(startLoc);
        openList.Enqueue(aStarNode);
        dict.Add(startLoc, aStarNode);
        while (openList.Count > 0)
        {
            AStarNode currentNode = openList.Dequeue();

            if (currentNode.loc == endLoc)
            {
                return BuildPathNoRe(startLoc, currentNode, out totalCost);
            }

            closeList.Add(currentNode.loc);


            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighbourLocation = new Vector3Int(currentNode.loc.x + x, currentNode.loc.y + y, currentNode.loc.z + z);

                        if (neighbourLocation.x < chunkPos.x || neighbourLocation.x >= end.x ||
                            neighbourLocation.y < chunkPos.y || neighbourLocation.y >= end.y ||
                            neighbourLocation.z < chunkPos.z || neighbourLocation.z >= end.z )
                        {
                            continue;
                        }
                        if (pointGrid[neighbourLocation.x, neighbourLocation.y, neighbourLocation.z] == false)
                        {
                            continue;
                        }

                        if (closeList.Contains(neighbourLocation))
                        {
                            continue;
                        }

                        AStarNode neighbourNode;
                        if (dict.TryGetValue(neighbourLocation, out neighbourNode))
                        {
                            int cost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            if (cost < neighbourNode.gCost)
                            {
                                neighbourNode.gCost = cost;
                                neighbourNode.parentNode = currentNode;
                                openList.UpdateItem(neighbourNode);
                            }
                        }
                        else
                        {
                            neighbourNode = new AStarNode(neighbourLocation);
                            openList.Enqueue(neighbourNode);
                            dict.Add(neighbourLocation, neighbourNode);

                            neighbourNode.gCost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            neighbourNode.hCost = CalDist(neighbourNode.loc, endLoc);
                            neighbourNode.parentNode = currentNode;

                        }
                    }
                }
            }
        }

        totalCost = 0;
        return null;
    }

    List<Vector3Int> BuildPathH(AStarNode endNode, out Vector3Int startExit)
    {
        List<Vector3Int> paths = new List<Vector3Int>();

        Vector3Int lastNodeLoc;
        AStarNode currentNode = endNode;

        int counter = 0;
        while (currentNode.parentNode != null)
        {
            //paths.Add(currentNode.loc);

            lastNodeLoc = currentNode.loc;
            currentNode = currentNode.parentNode;

            if (counter % 2 == 1)
            { 
                Chunk c = ChunkManager.Instance.chunkDic[GetChunkPos(lastNodeLoc)];

                paths.AddRange(c.pathDic[(lastNodeLoc, currentNode.loc)]);
            }

            counter++;
        }

        startExit = currentNode.loc;

        //paths.Add(currentNode.loc);

        //paths.Reverse();

        return paths;
    }

    List<Vector3Int> BuildPath(Vector3Int startLoc, AStarNode endNode)
    {
        List<Vector3Int> paths = new List<Vector3Int>();

        AStarNode currentNode = endNode;
        while (currentNode.loc != startLoc)
        {
            paths.Add(currentNode.loc);
            currentNode = currentNode.parentNode;
        }

        paths.Add(currentNode.loc);

        paths.Reverse();

        return paths;
    }

    List<Vector3Int> BuildPathNoRe(Vector3Int startLoc, AStarNode endNode, out int cost)
    {
        List<Vector3Int> paths = new List<Vector3Int>();

        AStarNode currentNode = endNode;
        while (currentNode.loc != startLoc)
        {
            paths.Add(currentNode.loc);
            currentNode = currentNode.parentNode;
        }

        paths.Add(currentNode.loc);

        cost = endNode.gCost;
        return paths;
    }


    int CalDist(Vector3Int locA, Vector3Int locB)
    {
        int distX = Mathf.Abs(locA.x - locB.x);
        int distY = Mathf.Abs(locA.y - locB.y);
        int distZ = Mathf.Abs(locA.z - locB.z);

        if (distX > distY && distX > distZ)
        {
            if (distY > distZ)
            {
                return 173 * distZ + 141 * (distY - distZ) + 100 * (distX - distY);
            }
            else
            { 
                return 173 * distY + 141 * (distZ - distY) + 100 * (distX - distZ);
            }
        }
        else if (distY > distX && distY > distZ)
        {
            if (distX > distZ)
            {
                return 173 * distZ + 141 * (distX - distZ) + 100 * (distY - distX);
            }
            else
            {
                return 173 * distX + 141 * (distZ - distX) + 100 * (distY - distZ);
            }
        }
        else
        {
            if (distX > distY)
            {
                return 173 * distY + 141 * (distX - distY) + 100 * (distZ - distX);
            }
            else
            {
                return 173 * distX + 141 * (distY - distX) + 100 * (distZ - distY);
            }
        }
    }


    public bool GetGrid(Vector3Int loc)
    {
        return pointGrid[loc.x, loc.y, loc.z];
    }

    public bool GetGrid(int x, int y, int z)
    {
        return pointGrid[x, y, z];
    }
    public bool TryGetGrid(Vector3Int loc)
    {
        if (loc.x < 0 || loc.y < 0 || loc.z < 0 ||
            loc.x > 319 || loc.y > 319 || loc.z > 319)
            return false;

        return pointGrid[loc.x, loc.y, loc.z];
    }

    public bool TryGetGrid(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 ||
            x > 319 || y > 319 || z > 319)
            return false;

        return pointGrid[x, y, z];
    }

    private Vector3Int GetChunkPos(Vector3Int pos)
    {
        return (pos / 8) * 8;
    }
}
