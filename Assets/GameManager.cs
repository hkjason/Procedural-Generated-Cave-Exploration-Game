using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha0))
        {
            player.Spawn(50, 52, 50);
            //player.Spawn(CaveGenerator.Instance.startingPt.x, CaveGenerator.Instance.startingPt.y, CaveGenerator.Instance.startingPt.z);
        }
    }
}
