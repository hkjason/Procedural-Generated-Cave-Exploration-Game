using UnityEngine;
public class Mushroom : MonoBehaviour
{
    bool healUsed = false;
    public GameObject parentMushroom;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!healUsed)
                Player.Instance.EnableHUD(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.DisableHUD(this);
        }
    }

    public bool UseHeal()
    {
        if (healUsed)
            return false;
        healUsed = true;
        Destroy(parentMushroom);
        return true;
    }

    private void OnDestroy()
    {
        Player.Instance.DisableHUD(this);
    }
}
