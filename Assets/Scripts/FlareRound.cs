using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareRound : MonoBehaviour
{
    [SerializeField]
    private float flareTTL;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private FlareType type;

    
    void Start()
    {
        Destroy(gameObject, flareTTL);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (type == FlareType.GunFlare)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
        else if (type == FlareType.HandFlare)
        {
            //HandleBounce(collision);
        }
    }

    void HandleBounce(Collision collision)
    { 
        
    }

    public enum FlareType
    { 
        HandFlare,
        GunFlare
    }
}
