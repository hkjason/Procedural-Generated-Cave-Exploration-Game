using UnityEngine;
public class FlareRound : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private FlareType type;
    [SerializeField]
    private Light light;
    private int[] lightIntensityArr = { 10, 14 };

    private void Start()
    {
        light.range = lightIntensityArr[GameManager.Instance.flareIntensityLevel];
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

    public enum FlareType
    { 
        HandFlare,
        GunFlare
    }
}
