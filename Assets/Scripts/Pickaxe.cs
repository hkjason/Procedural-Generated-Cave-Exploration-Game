using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Pickaxe : Equipment
{
    public LayerMask terrainLayer;
    public LayerMask oreLayer;
    private int terrainLayerIndex;
    private int oreLayerIndex;

    public Vector3 digPos;
    public Quaternion digRotation;


    private Ray ray;

    private void Start()
    {
        equipPos = new Vector3(0.3f, 0f, 0.6f);
        digPos = new Vector3(0.1f, -0.01f, 1f);
        equipRotation = Quaternion.Euler(new Vector3(0f, 270f, 355f));
        digRotation = Quaternion.Euler(new Vector3(0f, 249.3f, 327.2f));

        unequipPos = new Vector3(0.3f, -1f, 0.6f);
        unequipRotation = Quaternion.Euler(new Vector3(0f, 270f, 175f));

        transform.localPosition = equipPos;
        transform.localRotation = equipRotation;

        terrainLayerIndex = Mathf.RoundToInt(Mathf.Log(terrainLayer.value, 2));
        oreLayerIndex = Mathf.RoundToInt(Mathf.Log(oreLayer.value, 2));
    }

    public override void Use(Ray ray)
    {
        if (isAnimating) return;

        if (CheckCooldown())
        {
            this.ray = ray;

            StartCoroutine(MoveAxe());
        }
    }

    IEnumerator MoveAxe()
    {
        isAnimating = true;

        float duration = 0.5f;
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

        duration = 0.5f;
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
                CaveGenerator.Instance.DigCave(ray, hit);
            }
        }
    }
}
