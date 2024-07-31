using System;
using System.Collections;
using UnityEngine;

public class Gun : Equipment
{
    public GameObject bullet;
    public Transform barrelEnd;

    public int bulletSpeed = 2000;
    private int _spareRounds = 180;
    private int _currentRound = 30;

    private GameManager _gameManager;

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

    private void Start()
    {
        _gameManager = GameManager.Instance;

        equipPos = new Vector3(0.2f, -0.25f, 0.55f);
        equipRotation = Quaternion.Euler(new Vector3(-1f, -1f, 0f));

        unequipPos = new Vector3(0.2f, -0.7f, 0.55f);
        unequipRotation = Quaternion.Euler(new Vector3(60f, 0f, 0f));

        transform.localPosition = unequipPos;
        transform.localRotation = unequipRotation;
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
                _gameManager.shootBulletQuest = true;

                GameObject bulletInstance = Instantiate(bullet, barrelEnd.position, barrelEnd.rotation);

                Rigidbody bulletRb = bulletInstance.GetComponent<Rigidbody>();
                bulletRb.AddForce(barrelEnd.forward * bulletSpeed);

                //Instantiate(muzzleParticles, barrelEnd.position, barrelEnd.rotation);   //INSTANTIATING THE GUN'S MUZZLE SPARKS	
                currentRound--;
            }
        }
        else
        {
            //GetComponent<Animation>().Play("noAmmo");
            //GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
        }
    }

    public override void Reload()
    {
        if (isAnimating) return;

        isAnimating = true;

        if (currentRound < 30 && spareRounds >= 1)
        {
            _gameManager.reloadQuest = true;
            StartCoroutine(WaitReload());
        }
    }

    IEnumerator WaitReload()
    { 
        yield return StartCoroutine(ReloadCoroutine());

        int roundsToReload = 30 - currentRound;
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
    }


    IEnumerator ReloadCoroutine()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(equipPos, unequipPos, t);
            transform.localRotation = Quaternion.Lerp(equipRotation, unequipRotation, t);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(unequipPos, equipPos, t);
            transform.localRotation = Quaternion.Lerp(unequipRotation, equipRotation, t);

            yield return null;
        }

        isAnimating = false;
    }
}
