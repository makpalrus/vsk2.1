using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileGameUI : MonoBehaviour
{
    public static MobileGameUI Instance { get; private set; }
    public const string MainMenuScene = "MainMenu";
    public GameObject mainMenu, hud, pauseMenu, gameOver;
    public Button playButton, quitButton, pauseButton, resumeButton, restartButton, menuButton, retryButton, gameOverMenuButton;
    public Text counter;
    public HeartGraphic[] hearts;
    public Player player;
    public bool IsPlaying { get; private set; }
    public int Collected { get; private set; }
    public int TotalItems { get; private set; }
    InputAction pauseAction;
    void Awake()
    {
        Instance = this;
        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(Menu);
        retryButton.onClick.AddListener(Restart);
        gameOverMenuButton.onClick.AddListener(Menu);
        pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.Enable();
        Collected = 0;
        TotalItems = System.Array.FindAll(FindObjectsByType<CollectibleCoin>(FindObjectsSortMode.None), coin => coin.gameObject.scene == gameObject.scene).Length;
        UpdateCounter();
        RefreshHearts(player.GetComponent<PlayerHealth>().maxHearts);
        Play();
    }
    void Update()
    {
        if (!pauseAction.WasPressedThisFrame() || gameOver.activeSelf) return;
        if (IsPlaying) Pause(); else Resume();
    }
    void SetScreen(GameObject screen)
    {
        // Hiding the controls first releases the virtual stick and button.
        hud.SetActive(false);
        pauseMenu.SetActive(screen == pauseMenu);
        gameOver.SetActive(screen == gameOver);
        hud.SetActive(screen == hud);
        player.ClearInput();
    }

    public void Play() { SetScreen(hud); Time.timeScale = 1; IsPlaying = true; GameAudio.SetPaused(false); }
    public void Pause() { if (!IsPlaying) return; IsPlaying = false; SetScreen(pauseMenu); Time.timeScale = 0; GameAudio.SetPaused(true); }
    public void Resume() { if (!pauseMenu.activeSelf) return; Play(); }
    public void Restart() { Reload(SceneManager.GetActiveScene().name); }
    public void Menu() { Reload(MainMenuScene); }
    void Reload(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }
    public void AddItem() { if (IsPlaying) { Collected++; UpdateCounter(); GameAudio.Play(GameSound.Coin); if (Collected == TotalItems) GameAudio.Play(GameSound.Unlock); } }
    void UpdateCounter() { counter.text = Collected + " / " + TotalItems; }
    public void ShowVictory()
    {
        EndGame(true);
        foreach(var label in gameOver.GetComponentsInChildren<Text>(true)) if(label.name == "Title") label.text = "YOU WIN!";
    }
    public void RefreshHearts(int count)
    {
        for (int i = 0; i < hearts.Length; i++) hearts[i].color = i < count ? new Color32(255, 101, 127, 255) : new Color32(72, 84, 106, 255);
    }
    public void ShowGameOver() { EndGame(false); }
    void EndGame(bool victory)
    {
        IsPlaying = false; SetScreen(gameOver); Time.timeScale = 0;
        GameAudio.SetPaused(true);
        GameAudio.Play(victory ? GameSound.Finish : GameSound.Lose);
    }
    void OnApplicationFocus(bool focused) { if (!focused && IsPlaying) Pause(); }
    void OnApplicationPause(bool paused) { if (paused && IsPlaying) Pause(); }
    public void Quit()
    {
        Time.timeScale = 1;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void OnDestroy()
    {
        pauseAction?.Dispose();
        if (Instance == this) { Instance = null; Time.timeScale = 1; }
    }
}




