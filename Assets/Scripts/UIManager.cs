using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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



    // Start is called before the first frame update
    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;
        _gameManager = GameManager.Instance;

        _caveGenerator.OnGenComplete += InactivateLoadPanel;
        _player.oreCountChanged += UpdateOreCount;
        _player.flareCountChanged += UpdateFlareCount;
        _player.hpChanged += UpdateHp;

        _gameManager.digWallChanged += FirstWallDig;
        _gameManager.digOreChanged += FirstOreDig;
        _gameManager.shootBulletChanged += FirstBulletShoot;
        _gameManager.reloadChanged += FirstReload;
        _gameManager.throwFlareChanged += FirstFlareThrow;
        _gameManager.shootFlareChanged += FirstFlareShoot;

        map.mapChanged += MapToggle;

        Equipment.OnAmmoInfoUpdated += UpdateAmmoUI;

        loadingSlider.value = 0;
        flareCountSlider.value = 4;

        roundsText.text = "\u221E";

        sensitivitySlider.value = _gameManager.mouseSensitivity;
        inverseYToggle.isOn = _gameManager.inverseY;

        Cursor.lockState = CursorLockMode.Locked;

        sensitivitySlider.onValueChanged.AddListener(delegate { UpdateSettings(); });
        inverseYToggle.onValueChanged.AddListener(delegate { UpdateSettings(); });

        continueButton.onClick.AddListener(ContinueGame);
        quitButton.onClick.AddListener(QuitGameCheck);
        quitCancel.onClick.AddListener(QuitGameBack);
        quitConfirm.onClick.AddListener(QuitGameConfirm);

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
                settingsChanged = false;
                inGameUI.SetActive(false);
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    void SaveSettings()
    {
        if (settingsChanged)
        {
            _gameManager.mouseSensitivity = sensitivitySlider.value;
            _gameManager.inverseY = inverseYToggle.isOn;
            _gameManager.SaveGame();
        }
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
        hpText.text = hp.ToString() + "/100";
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

    void UpdateSettings()
    {
        settingsChanged = true;
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
