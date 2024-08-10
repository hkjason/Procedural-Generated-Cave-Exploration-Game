using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text fpsText;

    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TMP_Text loadingText;
    public TMP_Text percentageText;
    public TMP_Text oreCountText;
    public Image oreCountImage;
    public Slider flareCountSlider;

    public Slider hpSlider;
    private Coroutine hpCoroutine;
    public TMP_Text hpText;

    public GameObject pauseMenu;
    public Slider sensitivitySlider;
    public Toggle inverseYToggle;

    //Sound
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public GameObject inGameUI;

    public TMP_Text roundsText;

    public Map map;
    public GameObject mapGO;

    private CaveGenerator _caveGenerator;
    private GameManager _gameManager;
    [SerializeField] private Player _player;

    public Button continueButton;
    public Button quitButton;

    private bool settingsChanged = false;

    public float ringRadius;

    private string[] textArr =
    {
        "Brave walkers roaming...",
        "Procedurally Generating Life...",
        "PCG always comes with noise...",
        "Marching Cubes parade..."
    };

    //TUTORIALS
    public GameObject tutorialGO;
    //public RectTransform tutorialRect;

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

    public GameObject quitCheckPanel;
    public Button quitCancel;
    public Button quitConfirm;

    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Button victoryQuit;
    public Button defeatQuit;
    public TMP_Text victoryOre;
    public TMP_Text victoryCoin;
    public TMP_Text defeatOre;
    public TMP_Text defeatCoin;

    public Transform triangle;
    public Transform indicator;

    // Start is called before the first frame update
    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;
        _gameManager = GameManager.Instance;

        _caveGenerator.OnGenComplete += InactivateLoadPanel;
        _player.oreCountChanged += UpdateOreCount;
        _player.flareCountChanged += UpdateFlareCount;
        _player.hpChanged += UpdateHp;
        _player.OnDamageTaken += ShowDamageDirection;

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
        fpsText.text = (1.0f / Time.deltaTime).ToString();

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
                settingsChanged = false;
                inGameUI.SetActive(false);
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    void ShowDamageDirection(Vector3 attackerPosition, Vector3 playerPosition)
    {
        Vector3 direction = (attackerPosition - playerPosition).normalized;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        triangle.rotation = rotation;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(attackerPosition);

        indicator.localPosition = screenPosition;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        Vector2 directionFromCenter = new Vector2(screenPosition.x - screenCenter.x, screenPosition.y - screenCenter.y);

        Vector2 normalizedDirection = directionFromCenter.normalized;

        Vector2 ringPosition = screenCenter + normalizedDirection * ringRadius;

        //indicator.localPosition = ringPosition;
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
                victoryOre.text = Player.Instance.oreCount.ToString();
                victoryCoin.text = Mathf.FloorToInt(Player.Instance.oreCount / 4).ToString();
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
            _player.OnDamageTaken -= ShowDamageDirection;
        }
        if (_gameManager != null)
        {
            _gameManager.digWallChanged -= FirstWallDig;
            _gameManager.digOreChanged -= FirstOreDig;
            _gameManager.shootBulletChanged -= FirstBulletShoot;
            _gameManager.reloadChanged -= FirstReload;
            _gameManager.throwFlareChanged -= FirstFlareThrow;
            _gameManager.shootFlareChanged -= FirstFlareShoot;
        }
        if (map != null)
        { 
            map.mapChanged -= MapToggle;
        }

        Equipment.OnAmmoInfoUpdated -= UpdateAmmoUI;
    }
}
