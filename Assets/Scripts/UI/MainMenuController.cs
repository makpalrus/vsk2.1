using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string levelScene = "SampleScene";
    public Button playButton;
    public Button quitButton;

    void Awake()
    {
        Time.timeScale = 1f;
        playButton.onClick.AddListener(Play);
        quitButton.onClick.AddListener(Quit);
    }
    public void Play()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelScene);
    }
    public void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
