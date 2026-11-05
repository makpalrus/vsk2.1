using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
    public Sprite[] frames;
    SpriteRenderer picture;
    Vector3 origin;
    float age;
    bool collected;
    void Awake() { picture = GetComponent<SpriteRenderer>(); origin = transform.position; }
    void Update()
    {
        age += Time.deltaTime;
        transform.position = origin + Vector3.up * (Mathf.Sin(age * 3) * .08f);
        if (picture && frames != null && frames.Length > 0) picture.sprite = frames[(int)(age * 8) % frames.Length];
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.GetComponentInParent<Player>() || !MobileGameUI.Instance || !MobileGameUI.Instance.IsPlaying) return;
        collected = true;
        MobileGameUI.Instance.AddItem();
        gameObject.SetActive(false);
    }
}
