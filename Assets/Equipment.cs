using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : MonoBehaviour
{
    public float cooldown;
    private float lastUsedTime = -Mathf.Infinity;

    public virtual void Equip()
    {
        // Instantiate and position the item in the player's hand
        //itemObject.SetActive(true);
    }

    public virtual void Unequip()
    {
        // Hide or remove the item from the player's hand
        //itemObject.SetActive(false);
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
}
