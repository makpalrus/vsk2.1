using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameSound { Jump, Coin, Hurt, Click, Unlock, Finish, Lose }

// Lecture 5–6: AudioSource.PlayOneShot lets short effects overlap naturally.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }
    public AudioSource Music { get; private set; }
    public AudioSource Effects { get; private set; }
    public AudioSource Interface { get; private set; }
    readonly Dictionary<GameSound, AudioClip> clips = new Dictionary<GameSound, AudioClip>();
    string track;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Instance = null; AudioListener.pause = false; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() { Ensure(); }
    static GameAudio Ensure()
    {
        if (!Instance) new GameObject("Game Audio").AddComponent<GameAudio>();
        return Instance;
    }
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Music = Source(.28f, true, false);
        Effects = Source(.65f, false, false);
        Interface = Source(.6f, false, true);
        foreach (GameSound sound in System.Enum.GetValues(typeof(GameSound)))
        {
            string file = sound == GameSound.Finish ? "win" : sound.ToString().ToLowerInvariant();
            clips[sound] = Resources.Load<AudioClip>("Audio/" + file);
        }
        SceneManager.sceneLoaded += SceneLoaded;
    }
    AudioSource Source(float volume, bool loop, bool ignorePause)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.volume = volume;
        source.loop = loop;
        source.ignoreListenerPause = ignorePause;
        return source;
    }
    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioListener.pause = false;
        string next = scene.name == "MainMenu" ? "menu" : scene.name == "Level2" ? "level2" : "level";
        if (track != next || Music.clip == null)
        {
            Music.Stop();
            Music.clip = Resources.Load<AudioClip>("Audio/" + next);
            track = next;
            if (Music.clip) Music.Play();
        }
        foreach (var root in scene.GetRootGameObjects())
            foreach (var button in root.GetComponentsInChildren<Button>(true)) button.onClick.AddListener(Click);
    }
    static void Click() { Play(GameSound.Click); }
    public static void Play(GameSound sound)
    {
        var audio = Ensure();
        if (!audio.clips.TryGetValue(sound, out var clip) || !clip) return;
        var source = sound == GameSound.Click || sound == GameSound.Finish || sound == GameSound.Lose ? audio.Interface : audio.Effects;
        source.PlayOneShot(clip);
    }
    public static void SetPaused(bool paused) { Ensure(); AudioListener.pause = paused; }
    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= SceneLoaded;
        AudioListener.pause = false;
        Instance = null;
    }
}
