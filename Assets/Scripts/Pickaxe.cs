using GLTFast.Schema;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Pickaxe : Equipment
{
    public LayerMask allLayers;
    public LayerMask terrainLayer;
    public LayerMask oreLayer;
    public LayerMask plantLayer;
    private int terrainLayerIndex;
    private int oreLayerIndex;
    private int plantLayerIndex;

    public Vector3 digPos;
    public Quaternion digRotation;

    public Vector3 startPos1;
    public Quaternion startRotation1;
    public Vector3 digPos1;
    public Quaternion digRotation1;

    private Ray ray;

    private float[] pickSpeedArr = {1f, 0.8f, 0.65f};

    private GameManager gameManager;

    public UnityEngine.Camera _camera;

    private void Start()
    {
        gameManager = GameManager.Instance;

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
        plantLayerIndex = Mathf.RoundToInt(Mathf.Log(plantLayer.value, 2));

        startPos1 = new Vector3(-0.13f, 0, 0.6f);
        startRotation1 = Quaternion.Euler(new Vector3(29.2f, 295.2f, 355f));

        digPos1 = new Vector3(-0.13f, 0, 0.78f);
        digRotation1 = Quaternion.Euler(new Vector3(29.2f, 295.2f, 334f));
    }

    public override string GetAmmoInfo()
    {
        return "\u221E";
    }

    public override void Use(Ray ray)
    {
        if (isAnimating) return;
        isAnimating = true;

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

    public void TemporaryDig(Equipment lastEquipment)
    {
        if (isAnimating) return;
        isAnimating = true;

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

        StartCoroutine(DigOnce(lastEquipment));
    }

    IEnumerator DigOnce(Equipment lastEquipment)
    {
        yield return currentEquipCoroutine = StartCoroutine(EquipCoroutineNoReset());

        this.ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        yield return StartCoroutine(MoveAxeNoReset());

        if (currentEquipCoroutine != null)
        {
            StopCoroutine(currentEquipCoroutine);
            currentEquipCoroutine = null;
        }

        if (currentUnequipCoroutine != null)
        {
            StopCoroutine(currentUnequipCoroutine);
        }

        Unequip();
        Player.Instance.currentEquipment = lastEquipment;
        lastEquipment.Equip();
    }

    private IEnumerator EquipCoroutineNoReset()
    {
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

        currentEquipCoroutine = null;
    }

    IEnumerator MoveAxe()
    {
        float duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 2;
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

        duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 2;
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
        float duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 3;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(equipPos, startPos1, t);
            transform.localRotation = Quaternion.Lerp(equipRotation, startRotation1, t);

            yield return null;
        }

        duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 3;
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

        duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 3;
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

    IEnumerator MoveAxeNoReset()
    {
        float duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 2;
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

        duration = cooldown * pickSpeedArr[gameManager.pickSpeedLevel] / 2;
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localPosition = Vector3.Lerp(digPos, equipPos, t);
            transform.localRotation = Quaternion.Lerp(digRotation, equipRotation, t);

            yield return null;
        }
    }

    void Dig()
    {
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, 2f, allLayers))
        {
            if (hit.transform.gameObject.layer == terrainLayerIndex)
            {
                AudioManager.instance.PlayOnUnusedTrack(transform.position, "Pick_dirt");
                if (hit.transform.gameObject.tag == "Platform")
                {
                    Destroy(hit.transform.gameObject);
                }
                else
                {
                    CaveGenerator.Instance.DigCave(ray, hit);
                }
            }
            else if (hit.transform.gameObject.layer == oreLayerIndex)
            {
                int oreDigIdx = Random.Range(0, 5);
                switch (oreDigIdx)
                {
                    case 0:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining1");
                        break;
                    case 1:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining2");
                        break;
                    case 2:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining3");
                        break;
                    case 3:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining4");
                        break;
                    case 4:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining5");
                        break;
                    default:
                        AudioManager.instance.PlayOnUnusedTrack(hit.point, "Mining1");
                        break;
                }
                AudioManager.instance.PlayOnUnusedTrack(hit.point, "Pick_rock", 0.45f);
                CaveGenerator.Instance.DigOre(ray, hit);
            }
            else if (hit.transform.gameObject.layer == plantLayerIndex)
            {
                Destroyable destroyable = hit.collider.GetComponent<Destroyable>();
                if (destroyable != null)
                {
                    destroyable.Hit();
                }
            }
            else if (hit.transform.gameObject.tag == "Enemy")
            {
                Bat bat = hit.transform.GetComponent<Bat>();
                AudioManager.instance.PlayOnUnusedTrack(hit.point, "Bat_Hit");
                bat.BatHpChange(-20);
            }
        }
    }
}
