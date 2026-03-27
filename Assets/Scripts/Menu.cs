using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private int PlaySceneBuildIndex = 1;
    public void Play()
    {
        SceneManager.LoadSceneAsync(PlaySceneBuildIndex);
    }
}
