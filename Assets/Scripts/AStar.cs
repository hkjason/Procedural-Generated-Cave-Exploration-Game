using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

public class AStar : MonoBehaviour
{
    private bool[,,] pointGrid;

    [Header("Size")]
    public int width;
    public int height;
    public int depth;

    List<Vector3Int> path;

    public LayerMask groundLayer;

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = UnityEngine.Color.red;
        if (path != null && path.Count > 0)
        {
            foreach (var p in path)
            {
                Gizmos.DrawSphere(p, 0.1f);
            }
        }
        Gizmos.DrawRay(new Vector3(20.4239998f, 59.4669991f, 54.6850014f), Vector3.down);
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawSphere(new Vector3Int(1, 1, 1), 0.3f);
        Gizmos.DrawSphere(new Vector3Int(width / 2, depth / 4, height / 3), 0.3f);

        Gizmos.color = UnityEngine.Color.cyan;


        /*
        int drawn = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (pointGrid[x, y, z] && drawn < 1000)
                    {
                        drawn++;
                        Gizmos.DrawSphere(new Vector3(x, y, z) /4 + new Vector3(0.125f, 0.125f, 0.125f), 0.05f);
                    }
                }
            }
        }
        */

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                    {
                        Gizmos.color = UnityEngine.Color.cyan;
                    }
                    else if (pointGrid[81 + x, 236 + y, 218 + z])
                    {
                        Gizmos.color = UnityEngine.Color.green;
                    }
                    else
                    {
                        Gizmos.color = UnityEngine.Color.red;
                    }
                    Gizmos.DrawSphere(new Vector3(81 + x, 236 + y, 218 + z) / 4 + new Vector3(0.125f, 0.125f, 0.125f), 0.05f);
                }
            }
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
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


        }
    }


    private void Start()
    {
        path = new List<Vector3Int>();

        pointGrid = new bool[width, height, depth];

        path = PathFind(new Vector3Int(1, 1, 1), new Vector3Int(width / 2, depth / 4, height / 3));
    }

    public void UpdateGrid(int loc, bool val)
    {
        int x = loc / (320 * 320);
        int remainder = loc % (320 * 320);
        int y = remainder / 320;
        int z = remainder % 320;
        pointGrid[x, y, z] = val;
    }

    List<Vector3Int> PathFind(Vector3Int startLoc, Vector3Int endLoc)
    {
        Debug.Log("s" + startLoc);
        Debug.Log("e" + endLoc);

        Dictionary<Vector3Int, AstarNode> openList = new Dictionary<Vector3Int, AstarNode>();
        List<Vector3Int> closeList = new List<Vector3Int>();

        openList.Add(startLoc, new AstarNode(startLoc));
        while (openList.Count > 0)
        {
            
            AstarNode currentNode = null;

            foreach (AstarNode node in openList.Values)
            {
                if (currentNode == null || node.fCost < currentNode.fCost ||
                    node.fCost == currentNode.fCost && node.hCost < currentNode.hCost)
                {
                    currentNode = node;
                }
            }

            if (currentNode.loc == endLoc)
            {
                return BuildPath(startLoc, currentNode);
            }

            openList.Remove(currentNode.loc);
            closeList.Add(currentNode.loc);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighbourLocation = new Vector3Int(currentNode.loc.x + x, currentNode.loc.y + y, currentNode.loc.z + z);

                        if (pointGrid[neighbourLocation.x, neighbourLocation.y, neighbourLocation.z] == false
                            || closeList.Contains(neighbourLocation))
                        {
                            continue;
                        }

                        AstarNode neighbourNode;
                        if (openList.ContainsKey(neighbourLocation))
                        {
                            neighbourNode = openList[neighbourLocation];

                            int cost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            if (cost < neighbourNode.gCost)
                            {
                                neighbourNode.gCost = cost;
                                neighbourNode.parentNode = currentNode;
                            }
                        }
                        else
                        {
                            neighbourNode = new AstarNode(neighbourLocation);
                            openList.Add(neighbourLocation, neighbourNode);

                            neighbourNode.gCost = currentNode.gCost + CalDist(currentNode.loc, neighbourNode.loc);
                            neighbourNode.hCost = CalDist(neighbourNode.loc, endLoc);
                            neighbourNode.parentNode = currentNode;
                        }
                    }
                }
            }

        }

        return null;
    }

    List<Vector3Int> BuildPath(Vector3Int startLoc, AstarNode endNode)
    {
        List<Vector3Int> paths = new List<Vector3Int>();

        AstarNode currentNode = endNode;
        while (currentNode.loc != startLoc)
        {
            paths.Add(currentNode.loc);
            currentNode = currentNode.parentNode;
        }

        paths.Add(currentNode.loc);

        paths.Reverse();

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

    public class AstarNode
    {
        public Vector3Int loc;
        public AstarNode parentNode;
        public int gCost = 0; //from start node
        public int hCost = 0; //from end node
        public int fCost 
        {
            get { return gCost + hCost; }
        }

        public AstarNode(Vector3Int location)
        {
            loc = location;
        }
    }
}
