using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public FadingScript FadingScript;
    public GameObject GameOverUI;
    public TMP_Text DisplaySurvivalTime;

    private float StartTime;
    private float SurvivalTime = 0f;

    void Awake()
    {
        StartTime = Time.time;
    }

    public void Setup()
    {
        SurvivalTime = Time.time - StartTime;
        StartCoroutine(ShowGameOver());
    }

    public void MenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(2f);
        FadingScript.FadeOut(0.1f);

        // Show UI
        GameOverUI.SetActive(true);
        DisplaySurvivalTime.text = $"Survival Time: {SurvivalTime:00.00}";

        // Unlock mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
