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
    }

    public void StartGame()
    {
        //startButtonGO.SetActive(false);
        //shopButtonGO.SetActive(false);
        //loadPanel.SetActive(true);
        SceneManager.LoadSceneAsync(1);
    }
}
