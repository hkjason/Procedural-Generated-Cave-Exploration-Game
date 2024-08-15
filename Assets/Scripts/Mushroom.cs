using UnityEngine;

public class Mushroom : MonoBehaviour
{
    bool healUsed = false;
    public Light mushroomLight;

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
        mushroomLight.intensity = 0.2f;
        return true;
    }

    private void OnDestroy()
    {
        Player.Instance.DisableHUD(this);
    }
}
