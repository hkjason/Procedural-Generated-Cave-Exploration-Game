using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowAgent : CaveAgent
{
    List<TunnelAgent> tunnelAgentList;

    public GrowAgent(Vector3Int agentStartPt, int tokens, int weight) : base(agentStartPt, tokens, weight)
    {
        tunnelAgentList = new List<TunnelAgent>();
    }

    public override void Walk()
    {
        for (int i = 0; i < tokens; i++)
        {

            Vector3Int direction;
            do
            {
                //direction = new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));

                direction = cornerTable[Random.Range(0, 6)];
            }
            while (!WithinBounds(currentPos + (direction * weight) , weight/2));

            currentPos += (direction * weight);

            for (int x = -(weight / 2); x <= (weight / 2); x++)
            {
                for (int y = -(weight / 2); y <= (weight / 2); y++)
                {
                    for (int z = -(weight / 2); z <= (weight / 2); z++)
                    {
                        CaveGenerator.Instance.caveGrid[currentPos.x + x, currentPos.y + y, currentPos.z + z] = -1f;
                    }
                }
            }

            //Probability to spawn a smaller agent
            //int randomNumber = Random.Range(0, tokens);
            int randomNumber = Random.Range(0, 100);

            if (randomNumber == 0)
            {
                TunnelAgent tunnelAgent = new TunnelAgent(currentPos, Random.Range(tokens / 4 - tokens / 8, tokens / 4 + tokens / 8), weight / 2);
                tunnelAgentList.Add(tunnelAgent);
                Debug.Log("Spawn");
            }

            int randomNumber1 = Random.Range(0, 100);
            if (randomNumber1 == 0)
            {
                CaveGenerator.Instance.orePoints.Add(currentPos);
            }
        }

        foreach (TunnelAgent tunnelAgent in tunnelAgentList)
        {
            tunnelAgent.Walk();
        }

    }

    Vector3Int[] cornerTable = new Vector3Int[6] {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back,
    };
}
