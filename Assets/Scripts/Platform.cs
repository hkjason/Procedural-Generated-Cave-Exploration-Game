using System.Collections;
using UnityEngine;

public class Platform : MonoBehaviour
{
    bool expanding = false;
    Vector3 startScale = new Vector3(0.07f, 0.2f, 0.07f);
    Vector3 endScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Rigidbody rb;


    void OnCollisionEnter(Collision collision)
    {
        if (!expanding)
        {
            expanding = true;
            gameObject.layer = LayerMask.NameToLayer("Terrain");
            rb.isKinematic = true;
            transform.rotation = Quaternion.identity;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            StartCoroutine(ExpandPlatform());
        }
    }

    IEnumerator ExpandPlatform()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        Destroy(this);
    }
}
