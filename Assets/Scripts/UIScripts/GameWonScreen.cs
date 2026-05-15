using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StarterAssets;

public class GameWonScreen : MonoBehaviour
{
    public FadingScript FadingScript;
    public GameObject GameWonUI;
    public GameObject UI;
    public TMP_Text DisplayCompletionTime;
    public TMP_Text DisplayBulletsShot;
    public Gun gun;

    public CharacterController _characterController;
    public FirstPersonController _firstPersonController;
    public PlayerInputHandler _playerInputHandler;

    private float StartTime;
    private float SurvivalTime = 0f;

    private AudioSource AmbientSound;

    void Awake()
    {
        StartTime = Time.time;

        AmbientSound = GetComponent<AudioSource>();
    }

    public void Setup()
    {
        SurvivalTime = Time.time - StartTime;
        StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(2f);
        //FadingScript.FadeOut(0.1f);

        AmbientSound.enabled = false;

        UI.SetActive(false);
        _characterController.enabled = false;
        _firstPersonController.enabled = false;
        _playerInputHandler.enabled = false;

        // Show UI
        GameWonUI.SetActive(true);
        DisplayCompletionTime.text = $"Completion Time: {SurvivalTime:00.00}";
        DisplayBulletsShot.text = $"Bullets Shot: {gun.GetNumShots()}";

        // Unlock mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
