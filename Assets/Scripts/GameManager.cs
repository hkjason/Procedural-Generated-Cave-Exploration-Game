using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public CaveGenerator caveGenerator;

    // Start is called before the first frame update
    void Start()
    {
        caveGenerator = CaveGenerator.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            caveGenerator.CaveGeneration();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            player.Spawn(24, 44, 46);
            //player.Spawn(CaveGenerator.Instance.startingPt.x, CaveGenerator.Instance.startingPt.y, CaveGenerator.Instance.startingPt.z);
        }
    }
}
