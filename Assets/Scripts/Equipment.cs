using System;
using System.Collections;
using UnityEngine;

public abstract class Equipment : MonoBehaviour
{
    public float cooldown;
    private float lastUsedTime = -Mathf.Infinity;

    public Coroutine currentEquipCoroutine;
    public Coroutine currentUnequipCoroutine;

    public Vector3 equipPos;
    public Vector3 unequipPos;

    public Quaternion equipRotation;
    public Quaternion unequipRotation;

    public bool isAnimating;

    public static event Action<string> OnAmmoInfoUpdated;
    public abstract string GetAmmoInfo();

    protected void NotifyAmmoInfoUpdated()
    {
        OnAmmoInfoUpdated?.Invoke(GetAmmoInfo());
    }

    public virtual void Equip()
    {
        if (currentUnequipCoroutine != null)
        {
            StopCoroutine(currentUnequipCoroutine);
            currentUnequipCoroutine = null;
        }

        if (currentEquipCoroutine != null)
        {
            StopCoroutine(currentEquipCoroutine);
        }
        NotifyAmmoInfoUpdated();

        transform.gameObject.SetActive(true);
        currentEquipCoroutine = StartCoroutine(EquipCoroutine());
    }

    public virtual void Unequip()
    {
        if (currentEquipCoroutine != null)
        {
            StopCoroutine(currentEquipCoroutine);
            currentEquipCoroutine = null;
        }

        if (currentUnequipCoroutine != null)
        {
            StopCoroutine(currentUnequipCoroutine);
        }

        currentUnequipCoroutine = StartCoroutine(UnequipCoroutine());
    }

    public bool CheckCooldown()
    {
        if (Time.time >= lastUsedTime + cooldown)
        {
            lastUsedTime = Time.time;
            return true;
        }
        return false;
    }

    public abstract void Use(Ray ray);

    public virtual void Reload()
    {
        //Reload
    }

    public IEnumerator EquipCoroutine()
    {
        isAnimating = true;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(unequipPos, equipPos, t);
            transform.localRotation = Quaternion.Lerp(unequipRotation, equipRotation, t);

            yield return null;
        }

        isAnimating = false;

        currentEquipCoroutine = null;
    }
    public IEnumerator UnequipCoroutine()
    {
        isAnimating = true;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(equipPos, unequipPos, t);
            transform.localRotation = Quaternion.Lerp(equipRotation, unequipRotation, t);

            yield return null;
        }

        transform.gameObject.SetActive(false);

        currentUnequipCoroutine = null;

        isAnimating = false;
    }
}
