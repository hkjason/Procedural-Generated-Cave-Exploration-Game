using System.Collections;
using UnityEngine;

public class Pickaxe : Equipment
{
    public LayerMask terrainLayer;
    public LayerMask oreLayer;
    private int terrainLayerIndex;
    private int oreLayerIndex;

    public Vector3 digPos;
    public Quaternion digRotation;

    public Vector3 startPos1;
    public Quaternion startRotation1;
    public Vector3 digPos1;
    public Quaternion digRotation1;

    private Ray ray;

    private void Start()
    {
        equipPos = new Vector3(0.3f, 0f, 0.6f);
        digPos = new Vector3(0.1f, -0.01f, 1f);
        equipRotation = Quaternion.Euler(new Vector3(0f, 270f, 360f));
        digRotation = Quaternion.Euler(new Vector3(0f, 249.3f, 342.7f));

        unequipPos = new Vector3(0.3f, -1f, 0.6f);
        unequipRotation = Quaternion.Euler(new Vector3(0f, 270f, 175f));

        transform.localPosition = equipPos;
        transform.localRotation = equipRotation;

        terrainLayerIndex = Mathf.RoundToInt(Mathf.Log(terrainLayer.value, 2));
        oreLayerIndex = Mathf.RoundToInt(Mathf.Log(oreLayer.value, 2));

        startPos1 = new Vector3(-0.13f, 0, 0.6f);
        startRotation1 = Quaternion.Euler(new Vector3(29.2f, 295.2f, 355f));

        digPos1 = new Vector3(-0.13f, 0, 0.78f);
        digRotation1 = Quaternion.Euler(new Vector3(29.2f, 295.2f, 334f));
    }

    public override void Use(Ray ray)
    {
        if (isAnimating) return;

        this.ray = ray;

        int randomInt = Random.Range(0, 3);
        if (randomInt < 2)
        {
            StartCoroutine(MoveAxe());
        }
        else
        {
            StartCoroutine(MoveAxe1());
        }
    }

    IEnumerator MoveAxe()
    {
        isAnimating = true;

        float duration = cooldown / 2;
        float elapsed = 0f;

        while (elapsed < duration) 
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(equipPos, digPos, t);
            transform.localRotation = Quaternion.Lerp(equipRotation, digRotation, t);

            yield return null;
        }

        Dig();

        duration = cooldown / 2;
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(digPos, equipPos, t);
            transform.localRotation = Quaternion.Lerp(digRotation, equipRotation, t);

            yield return null;
        }

        isAnimating = false;
    }

    IEnumerator MoveAxe1()
    {
        isAnimating = true;

        float duration = cooldown / 3;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(equipPos, startPos1, t);
            transform.localRotation = Quaternion.Lerp(equipRotation, startRotation1, t);

            yield return null;
        }

        duration = cooldown / 3;
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(startPos1, digPos1, t);
            transform.localRotation = Quaternion.Lerp(startRotation1, digRotation1, t);

            yield return null;
        }

        Dig();

        duration = cooldown / 3;
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(digPos1, equipPos, t);
            transform.localRotation = Quaternion.Lerp(digRotation1, equipRotation, t);

            yield return null;
        }

        isAnimating = false;
    }

    void Dig()
    {
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 2f))
        {
            Debug.Log("Hit:" + hit.point);

            if (hit.transform.gameObject.layer == oreLayerIndex)
            {
                CaveGenerator.Instance.DigOre(ray, hit);
            }
            else if (hit.transform.gameObject.layer == terrainLayerIndex)
            {
                float curTime = Time.realtimeSinceStartup;
                CaveGenerator.Instance.DigCave(ray, hit);
                Debug.Log("DigTime" + (Time.realtimeSinceStartup - curTime));
            }
        }
    }
}
