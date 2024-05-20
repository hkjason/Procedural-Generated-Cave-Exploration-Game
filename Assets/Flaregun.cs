using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flaregun : Equipment
{
    public GameObject flare;
    public Transform muzzle;
    public float flareTTL;
    public float flareSpeed;

    public int ammo;

    public override void Use(Ray ray)
    {
        if (ammo > 0)
        {
            if (CheckCooldown())
            {
                GameObject flareSpawn = Instantiate(flare, muzzle.position, Quaternion.identity);
                Rigidbody rb = flareSpawn.GetComponent<Rigidbody>();
                rb.AddForce(ray.direction * flareSpeed);
                ammo--;
            }
        }
    }
}
