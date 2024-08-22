using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{
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
        GameManager.Instance.ResetQuest();
        SceneManager.LoadSceneAsync(1);
    }
}
