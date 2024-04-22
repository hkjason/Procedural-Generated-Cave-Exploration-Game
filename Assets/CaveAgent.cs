using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CaveAgent
{
    public Vector3Int currentPos;
    public int tokens;
    public int weight;

    public CaveAgent(Vector3Int agentStartPt, int tokens, int weight)
    {
        this.currentPos = agentStartPt;
        this.tokens = tokens;
        this.weight = weight;
    }

    public abstract void Walk();

    public bool WithinBounds(Vector3Int Pos)
    {
        if(Pos.x < 0 || Pos.x >= CaveGenerator.Instance.width) { return false; }
        if(Pos.y < 0 || Pos.y >= CaveGenerator.Instance.height) { return false; }
        if(Pos.z < 0 || Pos.z >= CaveGenerator.Instance.depth) { return false; }
        return true;
    }
}
