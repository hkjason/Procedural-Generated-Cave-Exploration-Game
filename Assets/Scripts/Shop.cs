using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    private GameManager gameManager;

    public Button pickPowerBtn;
    public Button pickSpeedBtn;
    public Button flareRechargeBtn;
    public Button flareDurationBtn;
    public Button flareIntensityBtn;
    public Button hpBtn;

    public TMP_Text moneyText;
           
    public TMP_Text pickPowerText;
    public TMP_Text pickSpeedText;
    public TMP_Text flareRechargeText;
    public TMP_Text flareDurationText;
    public TMP_Text flareIntensityText;
    public TMP_Text hpText;
           
    public TMP_Text messageText;

    public Slider pickPowerSlider;
    public Slider pickSpeedSlider;
    public Slider flareRechargeSlider;
    public Slider flareDurationSlider;
    public Slider flareIntensitySlider;
    public Slider hpSlider;

    private int pickPowerCost = 60;
    private int[] pickSpeedCost = { 10, 20, 40};
    private int[] flareRechargeCost = { 10, 20, 20};
    private int[] flareDurationCost = { 20, 40 };
    private int flareIntensityCost = 40;
    private int[] hpCost = { 10, 10, 20, 30, 40};

    public GameObject shopPanel;

    public RectTransform topBar;
    public float moveSpeed = 100f;
    public float showDuration = 2f;

    private Coroutine currentCoroutine;
    private Vector3 hiddenPosition = new Vector3(0,122,0);
    private Vector3 shownPosition = Vector3.zero;

    void Start()
    {
        gameManager = GameManager.Instance;

        UpdateMoney();
        UpdatePickPower();
        UpdatePickSpeed();
        UpdateFlareRecharge();
        UpdateFlareDuration();
        UpdateFlareIntensity();
        UpdateHP();

        pickPowerBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradePickaxePower(pickPowerCost))
            {
                messageText.text = "Pickaxe power upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdatePickPower();
            UpdateMoney();
        });

        pickSpeedBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradePickaxeSpeed(pickSpeedCost[gameManager.pickSpeedLevel]))
            {
                messageText.text = "Pickaxe speed upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdatePickSpeed();
            UpdateMoney();
        });

        flareRechargeBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradeFlareRecharge(flareRechargeCost[gameManager.flareRechargeLevel]))
            {
                messageText.text = "Recharge speed upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdateFlareRecharge();
            UpdateMoney();
        });

        flareDurationBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradeFlareDuration(flareDurationCost[gameManager.flareDurationLevel]))
            {
                messageText.text = "Flare duration upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdateFlareDuration();
            UpdateMoney();
        });

        flareIntensityBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradeFlareIntensity(flareIntensityCost))
            {
                messageText.text = "Flare intensity upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdateFlareIntensity();
            UpdateMoney();
        });

        hpBtn.onClick.AddListener(() =>
        {
            if (gameManager.UpgradeHP(hpCost[gameManager.hpLevel]))
            {
                messageText.text = "HP upgraded!";
            }
            else
            {
                messageText.text = "Not enough money.";
            }

            ShowBar();
            UpdateHP();
            UpdateMoney();
        });
    }


    void UpdateMoney()
    {
        moneyText.text = gameManager.money.ToString();
    }

    void UpdatePickPower()
    {
        int level = gameManager.pickPowerLevel;

        if (level == 0)
        {
            pickPowerText.text = pickPowerCost.ToString();
            pickPowerBtn.interactable = true;
        }
        else
        {
            pickPowerText.text = "Max";
            pickPowerBtn.interactable = false;
        }

        pickPowerSlider.value = level;
    }

    void UpdatePickSpeed()
    {
        int level = gameManager.pickSpeedLevel;
        if (level < pickSpeedCost.Length)
        {
            pickSpeedText.text = pickSpeedCost[level].ToString();
            pickSpeedBtn.interactable = true;
        }
        else
        {
            pickSpeedText.text = "Max";
            pickSpeedBtn.interactable = false;
        }

        pickSpeedSlider.value = level;
    }

    void UpdateFlareRecharge()
    {
        int level = gameManager.flareRechargeLevel;
        if (level < flareRechargeCost.Length)
        {
            flareRechargeText.text = flareRechargeCost[level].ToString();
            flareRechargeBtn.interactable = true;
        }
        else
        {
            flareRechargeText.text = "Max";
            flareRechargeBtn.interactable = false;
        }

        flareRechargeSlider.value = level;
    }

    void UpdateFlareDuration()
    {
        int level = gameManager.flareDurationLevel;
        if (level < flareDurationCost.Length)
        {
            flareDurationText.text = flareDurationCost[level].ToString();
            flareDurationBtn.interactable = true;
        }
        else
        {
            flareDurationText.text = "Max";
            flareDurationBtn.interactable = false;
        }

        flareRechargeSlider.value = level;
    }

    void UpdateFlareIntensity()
    {
        int level = gameManager.flareIntensityLevel;

        if (level == 0)
        {
            flareIntensityText.text = flareIntensityCost.ToString();
            flareIntensityBtn.interactable = true;
        }
        else
        {
            flareIntensityText.text = "Max";
            flareIntensityBtn.interactable = false;
        }

        flareIntensitySlider.value = level;
    }

    void UpdateHP()
    {
        int level = gameManager.hpLevel;
        if (level < hpCost.Length)
        {
            hpText.text = hpCost[level].ToString();
            hpBtn.interactable = true;
        }
        else
        {
            hpText.text = "Max";
            hpBtn.interactable = false;
        }

        hpSlider.value = level;
    }

    public void ShowBar()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(ShowAndHideBar());
    }

    private IEnumerator ShowAndHideBar()
    {
        yield return StartCoroutine(MoveBar(shownPosition));

        yield return new WaitForSeconds(showDuration);

        yield return StartCoroutine(MoveBar(hiddenPosition));
    }

    private IEnumerator MoveBar(Vector3 endPos)
    {
        Vector3 startPos = topBar.anchoredPosition;

        float elapsedTime = 0f;
        float distance = Vector2.Distance(startPos, endPos);

        while (elapsedTime < distance / moveSpeed)
        {
            topBar.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / (distance / moveSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        topBar.anchoredPosition = endPos;
    }


    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}
