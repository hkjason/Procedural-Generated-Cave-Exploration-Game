using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcavationAgent : CaveAgent
{
    public ExcavationAgent(Vector3Int agentStartPt, int tokens, int weight) : base(agentStartPt, tokens, weight)
    {
        // Any additional initialization for TunnelAgent
    }

    public override void Walk()
    {
        while (tokens > 0)
        {

            Vector3Int direction;
            do
            {
                direction = new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));

                //direction = cornerTable[Random.Range(0, 6)];
            }
            while (!WithinBounds(currentPos + (direction * weight)));

            currentPos += (direction* weight);

            CaveGenerator.Instance.caveGrid[currentPos.x, currentPos.y, currentPos.z] = false;

            tokens--;
        }

    }
}
