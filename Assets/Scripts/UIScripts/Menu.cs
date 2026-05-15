using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public string MainMapName = "MainMap2";

    public void Play()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(MainMapName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
