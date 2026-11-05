using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sprite;
    InputAction move, jump;
    bool isGrounded, jumpQueued;
    float horizontal;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        // Lecture 7: the same actions receive keyboard and virtual gamepad input.
        move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick");
        jump = new InputAction("Jump", InputActionType.Button);
        jump.AddBinding("<Keyboard>/space");
        jump.AddBinding("<Gamepad>/buttonSouth");
    }
    void OnEnable() { move?.Enable(); jump?.Enable(); }
    void OnDisable() { move?.Disable(); jump?.Disable(); ClearInput(); }
    void OnDestroy() { move?.Dispose(); jump?.Dispose(); }
    public void ClearInput() { horizontal = 0; jumpQueued = false; }
    void Update()
    {
        if (Time.timeScale == 0 || (MobileGameUI.Instance && !MobileGameUI.Instance.IsPlaying)) { ClearInput(); return; }
        horizontal = move.ReadValue<Vector2>().x;
        if (jump.WasPressedThisFrame()) jumpQueued = true;
        if (sprite && Mathf.Abs(horizontal) > 0.01f) sprite.flipX = horizontal < 0;
        SetAnimation(horizontal);
    }
    void FixedUpdate()
    {
        isGrounded = groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        if (jumpQueued && isGrounded) { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); isGrounded = false; GameAudio.Play(GameSound.Jump); }
        jumpQueued = false;
    }
    void SetAnimation(float input)
    {
        if (!animator) return;
        string state = isGrounded ? (Mathf.Abs(input) < .01f ? "Player_Idle" : "Player_Ran") : (rb.linearVelocity.y > 0 ? "Player_Jump" : "Player_Fall");
        animator.Play(state);
    }
}

