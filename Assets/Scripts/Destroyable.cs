using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public float timeBetweenHits = 2f;
    public int hitCount = 0;
    public float lastHitTime = 0f;
    public DestroyableType destroyableType;
    public enum DestroyableType
    {
        DestroyablePlant,
        DestroyableMushroom,
        DestroyableRock
    }

    public void Hit()
    {
        if (Time.time - lastHitTime > timeBetweenHits)
        {
            hitCount = 0;
        }

        hitCount++;
        lastHitTime = Time.time;

        switch (destroyableType)
        {
            case DestroyableType.DestroyablePlant:
                int randomIdx = Random.Range(0, 3);
                switch (randomIdx)
                {
                    case 0:
                        AudioManager.instance.PlayOnUnusedTrack(transform.position, "Plant_Hit1");
                        break;
                    case 1:
                        AudioManager.instance.PlayOnUnusedTrack(transform.position, "Plant_Hit2");
                        break;
                    case 2:
                        AudioManager.instance.PlayOnUnusedTrack(transform.position, "Plant_Hit3");
                        break;
                }
                break;
            case DestroyableType.DestroyableMushroom:
                AudioManager.instance.PlayOnUnusedTrack(transform.position, "Mushroom_Hit", 0.4f);
                break;
            case DestroyableType.DestroyableRock:
                AudioManager.instance.PlayOnUnusedTrack(transform.position, "Break_rock");
                break;
        }

        if (hitCount >= 2)
        {
            DigUp();
        }
    }

    public void DigUp()
    {
        Destroy(gameObject);
    }
}
