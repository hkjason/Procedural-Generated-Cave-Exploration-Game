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

    //Exit
    public Button exitButton;
    public Button exitConfirmButton;
    public Button exitCancelButton;
    public GameObject exitGO;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        creditButton.onClick.AddListener(OpenCredit);
        dimmedBackground.onClick.AddListener(CloseCredit);
        closeCreditButton.onClick.AddListener(CloseCredit);

        exitButton.onClick.AddListener(OpenExitCheck);
        exitConfirmButton.onClick.AddListener(ExitConfirm);
        exitCancelButton.onClick.AddListener(CloseExitCheck);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitGO.activeSelf)
            {
                CloseExitCheck();
            }
            else
            {
                OpenExitCheck();
            }
        }
    }

    void OpenCredit()
    {
        creditGO.SetActive(true);
    }

    void CloseCredit()
    {
        creditGO.SetActive(false);
    }

    void OpenExitCheck()
    {
        exitGO.SetActive(true);
    }

    void CloseExitCheck()
    {
        exitGO.SetActive(false);
    }

    void ExitConfirm()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        //startButtonGO.SetActive(false);
        //shopButtonGO.SetActive(false);
        //loadPanel.SetActive(true);
        SceneManager.LoadSceneAsync(1);
    }
}
