using System.Collections.Generic;
using UnityEngine;

public class GrowAgent : CaveAgent
{
    List<TunnelAgent> tunnelAgentList;
    int lastAdd;

    public GrowAgent(Vector3Int agentStartPt, int tokens, int weight) : base(agentStartPt, tokens, weight)
    {
        tunnelAgentList = new List<TunnelAgent>();
    }

    public override void Walk()
    {
        int i = 0;
        lastAdd = 10;

        while (i < tokens)
        {

            Vector3Int direction;
            do
            {
                //direction = new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));

                direction = cornerTable[Random.Range(0, 6)];
            }
            while (!WithinBounds(currentPos + (direction * weight) , weight/2));

            currentPos += (direction * weight);

            if (CaveGenerator.Instance.GetCave(currentPos.x, currentPos.y, currentPos.z) != 1f)
            {
                //Not Visited
                i++;
                RandomSpawn(i);
            }

            for (int x = -(weight / 2); x <= (weight / 2); x++)
            {
                for (int y = -(weight / 2); y <= (weight / 2); y++)
                {
                    for (int z = -(weight / 2); z <= (weight / 2); z++)
                    {
                        CaveGenerator.Instance.SetCave(currentPos.x + x, currentPos.y + y, currentPos.z + z, 1f);
                    }
                }
            }
        }

        foreach (TunnelAgent tunnelAgent in tunnelAgentList)
        {
            tunnelAgent.Walk();
        }

    }

    private void RandomSpawn(int i)
    {
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
        if (i > 10)
        {
            if ( randomNumber1 <= 3 + (i - lastAdd) )
            {
                lastAdd = i;
                CaveGenerator.Instance.orePoints.Add(currentPos);
            }
        }

        if (randomNumber1 >= 90 && randomNumber1 <= 99)
        {
            CaveGenerator.Instance.flowerPoints.Add(currentPos);
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
