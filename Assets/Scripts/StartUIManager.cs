using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartUIManager : MonoBehaviour
{
    private GameManager gameManager;

    public Button startButton;
    public GameObject startButtonGO;
    public GameObject shopButtonGO;

    public GameObject loadPanel;

    public TMP_Text stateText;
    public TMP_Text percentageText;
    public Slider percentageSlider;
    private string[] loadStateText =
    {
        "Brave walkers roaming...",
        "Procedurally Generating Life...",
        "PCG always comes with noise...",
        "Marching Cubes parade..."
    };

    private int[] loadStatePercentage =
    {
        0, 1, 5, 10
    };

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        gameManager = GameManager.Instance;
    }

    public void StartGame()
    {
        //startButtonGO.SetActive(false);
        //shopButtonGO.SetActive(false);
        //loadPanel.SetActive(true);
        StartCoroutine(StartGameCoroutine());
    }

    IEnumerator StartGameCoroutine()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);

        while (!operation.isDone)
        {
            stateText.text = loadStateText[gameManager.loadState];
            percentageText.text = loadStatePercentage[gameManager.loadState].ToString() + "%";
            percentageSlider.value = loadStatePercentage[gameManager.loadState];
            
            /*if (operation.progress < 0.225f)
            {
                stateText.text = loadStateText[0];
            }
            else if (operation.progress < 0.45f)
            {
                stateText.text = loadStateText[1];
            }
            else if (operation.progress < 0.675f)
            {
                stateText.text = loadStateText[2];
            }
            else
            {
                stateText.text = loadStateText[3];
            }
            percentageSlider.value = operation.progress;
            Debug.Log("pro" + operation.progress + "state" + gameManager.loadState);
            */
            yield return null;
        }

    }
}
