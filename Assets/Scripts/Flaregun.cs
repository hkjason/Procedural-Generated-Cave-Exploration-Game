using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flaregun : Equipment
{
    public Rigidbody flareBullet;
    public Transform barrelEnd;
    public GameObject muzzleParticles;
    public AudioClip flareShotSound;
    public AudioClip noAmmoSound;
    public AudioClip reloadSound;
    public int flareSpeed = 2000;
    public int maxSpareRounds = 5;
    public int spareRounds = 3;
    public int currentRound = 0;

    public float flareTTL;

    private void Start()
    {
        equipPos = new Vector3(0.4252565f, -0.7843642f, 0.9609928f);
        equipRotation = Quaternion.Euler(new Vector3(0f, -170.205f, 0f));

        unequipPos = new Vector3(0.4252565f, -1.12f, 0.9609928f);
        unequipRotation = Quaternion.Euler(new Vector3(-90f, -170.205f, 0f));

        transform.localPosition = unequipPos;
        transform.localRotation = unequipRotation;
    }

    public override void Use(Ray ray)
    {
        if (isAnimating) return;

        // && !GetComponent<Animation>().isPlaying

        if (currentRound > 0)
        {
            if (CheckCooldown())
            {
                GetComponent<Animation>().CrossFade("Shoot");
                GetComponent<AudioSource>().PlayOneShot(flareShotSound);

                Rigidbody bulletInstance;
                bulletInstance = Instantiate(flareBullet, barrelEnd.position, barrelEnd.rotation) as Rigidbody; //INSTANTIATING THE FLARE PROJECTILE
                bulletInstance.AddForce(barrelEnd.forward * flareSpeed);

                //Instantiate(muzzleParticles, barrelEnd.position, barrelEnd.rotation);   //INSTANTIATING THE GUN'S MUZZLE SPARKS	
                currentRound--;
            }
        }
        else
        {
            GetComponent<Animation>().Play("noAmmo");
            GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
        }
    }

    public override void Reload()
    {
        if (isAnimating) return;

        if (spareRounds >= 1 && currentRound == 0)
        {
            GetComponent<AudioSource>().PlayOneShot(reloadSound);
            spareRounds--;
            currentRound++;
            GetComponent<Animation>().CrossFade("Reload");
        }
    }
}
