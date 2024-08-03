using System;
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
    private int _spareRounds = 16;
    private int _currentRound = 4;

    private GameManager _gameManager;

    private int[] flareDurationArr = { 25, 30, 35 };

    public int spareRounds
    {
        get { return _spareRounds; }
        set
        {
            _spareRounds = value;
            NotifyAmmoInfoUpdated();
        }
    }

    public int currentRound
    {
        get { return _currentRound; }
        set
        {
            _currentRound = value;
            NotifyAmmoInfoUpdated();
        }
    }

    public float flareTTL;

    private void Start()
    {
        equipPos = new Vector3(0.4252565f, -0.7843642f, 0.9609928f);
        equipRotation = Quaternion.Euler(new Vector3(0f, -170.205f, 0f));

        unequipPos = new Vector3(0.4252565f, -1.12f, 0.9609928f);
        unequipRotation = Quaternion.Euler(new Vector3(-90f, -170.205f, 0f));

        transform.localPosition = unequipPos;
        transform.localRotation = unequipRotation;

        _gameManager = GameManager.Instance;
    }

    public override string GetAmmoInfo()
    {
        return $"{currentRound}/{spareRounds}";
    }

    public override void Use(Ray ray)
    {
        if (isAnimating) return;

        // && !GetComponent<Animation>().isPlaying

        if (currentRound > 0)
        {
            if (CheckCooldown())
            {
                GameManager.Instance.shootFlareQuest = true;

                GetComponent<Animation>().CrossFade("Shoot");
                GetComponent<AudioSource>().PlayOneShot(flareShotSound);

                Rigidbody bulletInstance;
                bulletInstance = Instantiate(flareBullet, barrelEnd.position, barrelEnd.rotation) as Rigidbody; //INSTANTIATING THE FLARE PROJECTILE
                bulletInstance.AddForce(barrelEnd.forward * flareSpeed);

                Light light = bulletInstance.GetComponent<Light>();
                Destroy(light, flareDurationArr[_gameManager.flareDurationLevel]);

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
        isAnimating = true;

        if (currentRound < 4 && spareRounds >= 1)
        {
            int roundsToReload = 4 - currentRound;
            if (spareRounds >= roundsToReload)
            {
                currentRound += roundsToReload;
                spareRounds -= roundsToReload;
            }
            else
            {
                currentRound += spareRounds;
                spareRounds = 0;
            }

            GetComponent<AudioSource>().PlayOneShot(reloadSound);
            GetComponent<Animation>().CrossFade("Reload");

            Invoke("OnFlareReloadEnd", 1.0831f);
        }
        else
        {
            isAnimating = false;
        }
    }

    public void OnFlareReloadEnd()
    {
        isAnimating = false;
    }
}
