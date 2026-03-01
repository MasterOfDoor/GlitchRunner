using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private float moveInput;

    private bool shootPressed;
    private bool jumpPressed;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Yeni Input System
        if (Keyboard.current != null)
        {
            moveInput = Keyboard.current.aKey.isPressed ? -1 :
                        Keyboard.current.dKey.isPressed ? 1 : 0;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                jumpPressed = true;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
                jumpPressed = true;
        }

        // Shoot (sol tık)
        if (Mouse.current != null)
        {
            shootPressed = Mouse.current.leftButton.wasPressedThisFrame;
        }

        // Hareket
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Zıplama
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jump");
        }

        jumpPressed = false;

        // Shoot animasyonu
        if (shootPressed)
        {
            animator.SetTrigger("shoot");
        }

        // Sprite yönü
        if (moveInput < 0) sr.flipX = true;
        else if (moveInput > 0) sr.flipX = false;

        // Animasyon parametreleri
        animator.SetFloat("speed", Mathf.Abs(moveInput));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
