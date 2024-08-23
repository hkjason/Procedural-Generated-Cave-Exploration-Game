using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Loading
    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TMP_Text loadingText;
    public TMP_Text percentageText;
    private string[] textArr =
    {
        "Brave walkers roaming...",
        "Procedurally Generating Life...",
        "PCG always comes with noise...",
        "Marching Cubes parade..."
    };

    //All inGameUI
    public GameObject inGameUI;
    public CanvasGroup pickaxeCG;
    public CanvasGroup gunCG;
    public CanvasGroup flaregunCG;
    public CanvasGroup platformLauncherCG;
    //OreCount
    public TMP_Text oreCountText;
    public Image oreCountImage;
    public TMP_Text earlyReturnText;
    //FlareCount
    public Slider flareCountSlider;
    //Ammo
    public TMP_Text roundsText;
    //HP
    public Slider hpSlider;
    public TMP_Text hpText;
    private Coroutine hpCoroutine;

    //Pause
    public GameObject pauseMenu;
    //Settings
    public Slider sensitivitySlider;
    public Toggle inverseYToggle;
    //Sound
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;
    //Return
    public Button continueButton;
    public Button continueButton2;
    public Button quitButton;
    public GameObject quitCheckPanel;
    public Button quitCancel;
    public Button quitConfirm;

    //Map
    public Map map;
    public GameObject mapGO;

    //Reference Scripts
    private CaveGenerator _caveGenerator;
    private GameManager _gameManager;
    [SerializeField] private Player _player;


    //TUTORIALS
    public GameObject tutorialGO;
    public GameObject set1;
    public Image set1check1;
    public Image set1check2;
    public GameObject set2;
    public Image set2check1;
    public Image set2check2;
    public GameObject set3;
    public Image set3check1;
    public Image set3check2;

    private int set1Completed;
    private int set2Completed;
    private int tutorialCompleted;

    //GameEndLogic
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Button victoryQuit;
    public Button defeatQuit;
    public TMP_Text victoryOre;
    public TMP_Text victoryCoin;
    public TMP_Text defeatOre;
    public TMP_Text defeatCoin;

    //HealHUD
    public GameObject pickupHUDGO;

    //manual
    public Image mineImg;
    public Image naviImg;
    public Image traImg;
    public Image combatImg;

    public Sprite activeSprite;
    public Sprite inactiveSprite;

    public GameObject mineGO;
    public GameObject naviGO;
    public GameObject traGO;
    public GameObject combatGO;

    public Image settingImg;
    public Image guideImg;
    public Sprite tabOnSprite;
    public Sprite tabOffSprite;
    public GameObject settingGO;
    public GameObject guideGO;

    // Start is called before the first frame update
    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;
        _gameManager = GameManager.Instance;

        _caveGenerator.OnGenComplete += InactivateLoadPanel;

        _player.oreCountChanged += UpdateOreCount;
        _player.flareCountChanged += UpdateFlareCount;
        _player.hpChanged += UpdateHp;
        _player.OnEquipmentChanged += HandleEquipmentChange;
        _player.showHUD += PickupHUDToggle;

        _gameManager.digWallChanged += FirstWallDig;
        _gameManager.digOreChanged += FirstOreDig;
        _gameManager.shootBulletChanged += FirstBulletShoot;
        _gameManager.reloadChanged += FirstReload;
        _gameManager.throwFlareChanged += FirstFlareThrow;
        _gameManager.shootFlareChanged += FirstFlareShoot;
        _gameManager.gameEndEvent += GameEndEvent;

        map.mapChanged += MapToggle;

        Equipment.OnAmmoInfoUpdated += UpdateAmmoUI;

        loadingSlider.value = 0;
        flareCountSlider.value = 4;

        roundsText.text = "\u221E";

        sensitivitySlider.value = _gameManager.mouseSensitivity;
        inverseYToggle.isOn = _gameManager.inverseY;

        masterSlider.value = AudioManager.instance.AudioSetting.masterVolume;
        bgmSlider.value = AudioManager.instance.AudioSetting.BGMVolume;
        sfxSlider.value = AudioManager.instance.AudioSetting.sfxVolume;

        Cursor.lockState = CursorLockMode.Locked;

        masterSlider.onValueChanged.AddListener(MasterChange);
        bgmSlider.onValueChanged.AddListener(BGMChange);
        sfxSlider.onValueChanged.AddListener(SfxChange);

        continueButton.onClick.AddListener(ContinueGame);
        continueButton2.onClick.AddListener(ContinueGame);
        quitButton.onClick.AddListener(QuitGameCheck);
        quitCancel.onClick.AddListener(QuitGameBack);
        quitConfirm.onClick.AddListener(QuitGameConfirm);

        victoryQuit.onClick.AddListener(QuitGameConfirm);
        defeatQuit.onClick.AddListener(QuitGameConfirm);

        if (!_gameManager.tutorial)
        {
            tutorialGO.SetActive(true);
            set1.gameObject.SetActive(true);
            tutorialCompleted = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_caveGenerator.isGen)
        {
            float progress = _caveGenerator.generateProgress;
            //float progress = Mathf.Lerp(loadingSlider.value, _caveGenerator.generateProgress, Time.deltaTime);
            loadingSlider.value = progress;
            percentageText.text = (progress * 100f).ToString("F1") + "%";

            loadingText.text = textArr[_caveGenerator.progressInt];
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;

                SaveSettings();

                pauseMenu.SetActive(false);
                inGameUI.SetActive(true);
                _gameManager.isPause = false;
                Time.timeScale = 1;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                _gameManager.isPause = true;
                Cursor.lockState = CursorLockMode.None;
                inGameUI.SetActive(false);
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
    private void HandleEquipmentChange(Equipment newEquipment)
    {
        pickaxeCG.alpha = 0.4f;
        gunCG.alpha = 0.4f;
        flaregunCG.alpha = 0.4f;
        platformLauncherCG.alpha = 0.4f;
        if (newEquipment is Pickaxe)
        {
            pickaxeCG.alpha = 1f;
        }
        else if (newEquipment is Gun)
        {
            gunCG.alpha = 1f;
        }
        else if (newEquipment is Flaregun)
        {
            flaregunCG.alpha = 1f;
        }
        else if (newEquipment is PlatformLauncher)
        {
            platformLauncherCG.alpha = 1f;
        }
    }

    void SaveSettings()
    {
        _gameManager.mouseSensitivity = sensitivitySlider.value;
        _gameManager.inverseY = inverseYToggle.isOn;
        _gameManager.SaveGame();
    }

    void ContinueGame()
    {
        Cursor.lockState = CursorLockMode.Locked;

        SaveSettings();

        pauseMenu.SetActive(false);
        inGameUI.SetActive(true);
        _gameManager.isPause = false;
        Time.timeScale = 1;
    }

    void QuitGameCheck()
    {
        quitCheckPanel.SetActive(true);
    }

    void QuitGameConfirm()
    {
        _gameManager.isPause = false;
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
    }

    void QuitGameBack()
    {
        quitCheckPanel.SetActive(false);
    }

    void InactivateLoadPanel()
    {
        loadingPanel.SetActive(false);
    }

    void UpdateOreCount(float oreCount)
    {
        oreCountText.text = "Collect Gold " + oreCount.ToString() + "/ 200";

        if (oreCount >= 200)
        {
            oreCountImage.enabled = true;
            earlyReturnText.enabled = true;
        }
    }

    void UpdateHp(int hp)
    {
        if (hpCoroutine != null)
        {
            StopCoroutine(hpCoroutine);
        }
        hpCoroutine = StartCoroutine(UpdateHpCoroutine(hp));
        hpText.text = hp.ToString() + "/" + Player.Instance.maxHp;
    }

    IEnumerator UpdateHpCoroutine(int newHp)
    {
        float elapsedTime = 0f;
        float startingHp = hpSlider.value;

        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;
            hpSlider.value = Mathf.Lerp(startingHp, newHp, elapsedTime / 0.2f);
            yield return null;
        }
        hpSlider.value = newHp;
    }

    void UpdateFlareCount(int flareCount)
    {
        flareCountSlider.value = flareCount;
    }

    void UpdateAmmoUI(string ammoInfo)
    {
        roundsText.text = ammoInfo;
    }

    void MasterChange(float f)
    {
        AudioManager.instance.AudioSetting.masterVolume = f;
    }

    void BGMChange(float f)
    {
        AudioManager.instance.AudioSetting.BGMVolume = f;
    }

    void SfxChange(float f)
    {
        AudioManager.instance.AudioSetting.sfxVolume = f;
    }

    void MapToggle(bool b)
    {
        mapGO.SetActive(b);
    }

    void PickupHUDToggle(bool b)
    { 
        pickupHUDGO.SetActive(b);
    }

    void FirstWallDig(bool b)
    {
        set1check1.enabled = b;
        tutorialCompleted++;
        set1Completed++;
        TutChange();
        CompleteTut();
    }

    void FirstOreDig(bool b)
    {
        set1check2.enabled = b;
        tutorialCompleted++;
        set1Completed++;
        TutChange();
        CompleteTut();
    }
    void FirstBulletShoot(bool b)
    {
        set2check1.enabled = b;
        tutorialCompleted++;
        set2Completed++;
        TutChange();
        CompleteTut();
    }

    void FirstReload(bool b)
    {
        set2check2.enabled = b;
        tutorialCompleted++;
        set2Completed++;
        TutChange();
        CompleteTut();
    }

    void FirstFlareShoot(bool b)
    {
        set3check1.enabled = b;
        tutorialCompleted++;
        CompleteTut();
    }
    void FirstFlareThrow(bool b)
    {
        set3check2.enabled = b;
        tutorialCompleted++;
        CompleteTut();
    }

    void GameEndEvent(bool b)
    {
        if (b)
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                victoryOre.text = Player.Instance.oreCount.ToString();
                victoryCoin.text = Mathf.FloorToInt(Player.Instance.oreCount / 2).ToString();
            }
        }
        else
        {
            if (defeatPanel != null)
            { 
                defeatPanel.SetActive(true);
                defeatOre.text = Player.Instance.oreCount.ToString();
                defeatCoin.text = Mathf.FloorToInt(Player.Instance.oreCount / 4).ToString();
            }
        }
    }

    void TutChange()
    {
        if (set1Completed == 2)
        {
            set1.SetActive(false);
            if (set2Completed == 2)
            {
                set2.SetActive(false);
                set3.SetActive(true);
            }
            else
            {
                set2.SetActive(true);
            }
        }
    }

    void CompleteTut()
    {
        if (tutorialCompleted == 6)
        {
            tutorialGO.SetActive(false);
            _gameManager.tutorial = true;
            _gameManager.SaveGame();
        }
    }

    #region guidebook
    public void ListItemOnClick(int i)
    {
        DisableItems();

        switch (i)
        {
            case 0:
                mineImg.sprite = activeSprite;
                mineGO.SetActive(true);
                break;
            case 1:
                naviImg.sprite = activeSprite;
                naviGO.SetActive(true);
                break;
            case 2:
                traImg.sprite = activeSprite;
                traGO.SetActive(true);
                break;
            case 3:
                combatImg.sprite = activeSprite;
                combatGO.SetActive(true);
                break;
        }
    }

    private void DisableItems()
    {
        mineImg.sprite = inactiveSprite;
        naviImg.sprite = inactiveSprite;
        traImg.sprite = inactiveSprite;
        combatImg.sprite = inactiveSprite;

        mineGO.SetActive(false);
        naviGO.SetActive(false);
        traGO.SetActive(false);
        combatGO.SetActive(false);
    }

    public void TabOnClick(int i)
    {
        switch (i)
        {
            case 0:
                settingImg.sprite = tabOnSprite;
                settingGO.SetActive(true);
                guideImg.sprite = tabOffSprite;
                guideGO.SetActive(false);
                break;
            case 1:
                settingImg.sprite = tabOffSprite;
                settingGO.SetActive(false);
                guideImg.sprite = tabOnSprite;
                guideGO.SetActive(true);
                break;
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (_caveGenerator != null)
        { 
            _caveGenerator.OnGenComplete -= InactivateLoadPanel;
        }
        if (_player != null)
        { 
            _player.oreCountChanged -= UpdateOreCount;
            _player.flareCountChanged -= UpdateFlareCount;
            _player.hpChanged -= UpdateHp;
            _player.OnEquipmentChanged -= HandleEquipmentChange;
            _player.showHUD -= PickupHUDToggle;
        }
        if (_gameManager != null)
        {
            _gameManager.digWallChanged -= FirstWallDig;
            _gameManager.digOreChanged -= FirstOreDig;
            _gameManager.shootBulletChanged -= FirstBulletShoot;
            _gameManager.reloadChanged -= FirstReload;
            _gameManager.throwFlareChanged -= FirstFlareThrow;
            _gameManager.shootFlareChanged -= FirstFlareShoot;
            _gameManager.gameEndEvent -= GameEndEvent;
        }
        if (map != null)
        { 
            map.mapChanged -= MapToggle;
        }

        Equipment.OnAmmoInfoUpdated -= UpdateAmmoUI;
    }
}
