using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class FinishGate : MonoBehaviour
{
    public string nextLevel = "Level2";
    public TextMesh message;
    SpriteRenderer flag;
    bool finished;
    public bool IsUnlocked => MobileGameUI.Instance && MobileGameUI.Instance.TotalItems > 0 && MobileGameUI.Instance.Collected >= MobileGameUI.Instance.TotalItems;
    void Awake() { flag = GetComponent<SpriteRenderer>(); }
    void Update()
    {
        bool ready = IsUnlocked;
        flag.color = ready ? new Color32(100,255,150,255) : new Color32(160,160,175,255);
        if(message) message.text = ready ? (string.IsNullOrEmpty(nextLevel) ? "FINISH!" : "LEVEL 2 >") : "COLLECT ALL COINS";
    }
    void OnTriggerEnter2D(Collider2D other) { TryFinish(other); }
    void OnTriggerStay2D(Collider2D other) { TryFinish(other); }
    void TryFinish(Collider2D other)
    {
        var ui = MobileGameUI.Instance;
        if(finished || !IsUnlocked || !ui || !ui.IsPlaying || !other.GetComponentInParent<Player>()) return;
        finished = true;
        if(string.IsNullOrEmpty(nextLevel)) ui.ShowVictory();
        else { GameAudio.Play(GameSound.Finish); Time.timeScale=1; SceneManager.LoadScene(nextLevel); }
    }
}

