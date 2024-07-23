using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TMP_Text loadingText;
    public TMP_Text percentageText;
    public TMP_Text oreCountText;
    public Image oreCountImage;

    private CaveGenerator _caveGenerator;
    [SerializeField] private Player _player;

    private string[] textArr =
    {
        "Brave walkers roaming...",
        "Procedurally Generating Life...",
        "PCG always comes with noise...",
        "Marching Cubes parade..."
    };

    // Start is called before the first frame update
    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;

        _caveGenerator.OnGenComplete += InactivateLoadPanel;
        _player.oreCountChanged += UpdateOreCount;

        loadingSlider.value = 0;
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

    private void OnDestroy()
    {
        if (_caveGenerator != null)
        { 
            _caveGenerator.OnGenComplete -= InactivateLoadPanel;
        }
        if (_player != null)
        { 
            _player.oreCountChanged -= UpdateOreCount;
        }
    }
}
