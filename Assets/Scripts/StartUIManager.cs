using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{
    public Button startButton;
    public GameObject startButtonGO;
    public GameObject shopButtonGO;

    public GameObject loadPanel;

    public TMP_Text stateText;
    public TMP_Text percentageText;
    public Slider percentageSlider;

    //Credit
    public Button creditButton;
    public Button dimmedBackground;
    public Button closeCreditButton;
    public GameObject creditGO;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        creditButton.onClick.AddListener(OpenCredit);
        dimmedBackground.onClick.AddListener(CloseCredit);
        closeCreditButton.onClick.AddListener(CloseCredit);
    }

    void OpenCredit()
    { 
        creditGO.SetActive(true);
    }

    void CloseCredit()
    {
        creditGO.SetActive(false);
    }

    public void StartGame()
    {
        //startButtonGO.SetActive(false);
        //shopButtonGO.SetActive(false);
        //loadPanel.SetActive(true);
        SceneManager.LoadSceneAsync(1);
    }
}
