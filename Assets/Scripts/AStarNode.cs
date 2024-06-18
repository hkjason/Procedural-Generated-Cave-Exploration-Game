using System;
using UnityEngine;
public class AStarNode : IComparable<AStarNode>
{
    public Vector3Int loc;
    public AStarNode parentNode;
    public int gCost = 0; //from start node
    public int hCost = 0; //from end node
    public int qIdx;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public AStarNode(Vector3Int location)
    {
        loc = location;
    }

    public int CompareTo(AStarNode other)
    {
        int compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return compare;
    }

}