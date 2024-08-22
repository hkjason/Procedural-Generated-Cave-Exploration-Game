using UnityEngine;

public class TunnelAgent : CaveAgent
{
    public TunnelAgent(Vector3Int agentStartPt, int tokens, int weight) : base(agentStartPt, tokens, weight)
    {

    }

    public override void Walk()
    {
        for (int i = 0; i < tokens; i++)
        {

            Vector3Int direction;
            do
            {
                direction = cornerTable[Random.Range(0, 6)];
            }
            while (!WithinBounds(currentPos + (direction * weight)));

            currentPos += (direction * weight);

            for (int x = -(weight / 2); x <= (weight / 2); x++)
            {
                for (int y = -(weight / 2); y <= (weight / 2); y++)
                {
                    for (int z = -(weight / 2); z <= (weight / 2); z++)
                    {
                        if (WithinBounds(currentPos + new Vector3Int(x, y, z)))
                            CaveGenerator.Instance.SetCave(currentPos.x + x, currentPos.y + y, currentPos.z + z, 1f);
                    }
                }
            }
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
