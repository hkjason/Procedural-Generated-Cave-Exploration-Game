using UnityEngine;

public class ExcavationAgent : CaveAgent
{
    public ExcavationAgent(Vector3Int agentStartPt, int tokens, int weight) : base(agentStartPt, tokens, weight)
    {
    }

    public override void Walk()
    {
        Vector3Int direction;
        for (int i = - weight/2; i < weight/2 + 1; i++)
        {
            for (int j = - weight/2; j <  weight /2 +1; j++)
            {
                for (int k = - weight /2; k < weight /2 +1; k++)
                {
                    direction = new Vector3Int(i, j, k);
                    if (WithinBounds(currentPos + direction))
                    {
                        CaveGenerator.Instance.SetCave(currentPos.x + direction.x, currentPos.y + direction.y, currentPos.z + direction.z, 1f); 
                    }
                }
            }
        }

    }
}
