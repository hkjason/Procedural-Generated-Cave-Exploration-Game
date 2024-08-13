using UnityEngine;

public class FlareRound : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private FlareType type;

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

    public enum FlareType
    { 
        HandFlare,
        GunFlare
    }
}
