using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider loadingSlider;

    private CaveGenerator _caveGenerator;

    // Start is called before the first frame update
    void Start()
    {
        _caveGenerator = CaveGenerator.Instance;

        _caveGenerator.OnGenComplete += InactivateLoadPanel;

        loadingSlider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_caveGenerator.isGen)
        { 
            loadingSlider.value = Mathf.Lerp(loadingSlider.value, _caveGenerator.generateProgress, Time.deltaTime);
        }
    }

    void InactivateLoadPanel()
    {
        loadingPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_caveGenerator != null)
        { 
            _caveGenerator.OnGenComplete -= InactivateLoadPanel;
        }
    }
}
