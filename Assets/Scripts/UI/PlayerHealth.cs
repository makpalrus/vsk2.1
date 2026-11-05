using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    public float fallY = -7f;
    public int Hearts { get; private set; }
    Vector3 spawn;
    float nextDamage;
    SpriteRenderer picture;
    Coroutine blink;
    void Awake() { Hearts = maxHearts; spawn = transform.position; picture = GetComponent<SpriteRenderer>(); }
    void Update()
    {
        if (MobileGameUI.Instance && MobileGameUI.Instance.IsPlaying && transform.position.y < fallY) TakeDamage(1);
    }
    // The level's spike tilemap already has the Damage tag. Support solid
    // colliders and triggers, including contact that began during immunity.
    void OnCollisionEnter2D(Collision2D collision) { TouchHazard(collision.collider); }
    void OnCollisionStay2D(Collision2D collision) { TouchHazard(collision.collider); }
    void OnTriggerEnter2D(Collider2D other) { TouchHazard(other); }
    void OnTriggerStay2D(Collider2D other) { TouchHazard(other); }
    void TouchHazard(Collider2D other)
    {
        for (Transform part = other.transform; part; part = part.parent)
            if (part.CompareTag("Damage")) { TakeDamage(1); return; }
    }
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || Time.time < nextDamage || !MobileGameUI.Instance || !MobileGameUI.Instance.IsPlaying) return;
        if (blink != null) StopCoroutine(blink);
        blink = null;
        if (picture) picture.enabled = true;
        Hearts = Mathf.Max(0, Hearts - amount);
        if (Hearts > 0) GameAudio.Play(GameSound.Hurt);
        nextDamage = Time.time + 1;
        MobileGameUI.Instance.RefreshHearts(Hearts);
        GetComponent<Player>().ClearInput();
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        transform.position = spawn;
        if (Hearts == 0) MobileGameUI.Instance.ShowGameOver();
        else if (picture) blink = StartCoroutine(BlinkAfterHit());
    }
    // Lectures 5–6: a coroutine shows the one-second invulnerability window.
    IEnumerator BlinkAfterHit()
    {
        while (Time.time < nextDamage)
        {
            picture.enabled = !picture.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        picture.enabled = true;
        blink = null;
    }
    void OnDisable()
    {
        if (blink != null) StopCoroutine(blink);
        blink = null;
        if (picture) picture.enabled = true;
    }
}

