using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Platformer-Shooter hareket sistemi
/// - Smooth acceleration/deceleration
/// - Coyote time (zemin bıraktıktan sonra kısa süre zıplayabilme)
/// - Jump buffer (tuşa basıp zemine gelince zıplama)
/// - Variable jump height (tuş tutma süresiyle kontrol)
/// - Air control
/// - Basit raycast zemin kontrolü
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Move : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.5f;

    [Header("Zıplama Ayarları")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField, Range(0.1f, 1f)] private float lowJumpMultiplier = 0.55f;

    [SerializeField] private UnityEngine.InputSystem.Key jumpKey = UnityEngine.InputSystem.Key.Space;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;
    [SerializeField] private bool enableDebugLogs = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private float horizontalInput;
    private int groundContactCount;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private bool wasGroundedLastFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        rb.freezeRotation = true;
        rb.gravityScale = 1f;
        groundContactCount = 0;
        
        Debug.Log($"=== [Move] START ===");
        Debug.Log($"GameObject: '{gameObject.name}' | Rigidbody2D: {(rb != null ? "✓" : "✗")} | Collider2D: {(col != null ? "✓" : "✗")}");
        Debug.Log($"Jump Key: {jumpKey} | Keyboard: {(UnityEngine.InputSystem.Keyboard.current != null ? "✓" : "✗")}");
        Debug.Log($"Using Collision-based Ground Detection");
        Debug.Log($"=== [Move] READY ===");
    }

    private void Update()
    {
        HandleInput();
        UpdateTimers();
        HandleJumpInputRelease();
    }

    private void FixedUpdate()
    {
        UpdateGroundState();
        ApplyHorizontalMovement();
        TryPerformJump();
    }

    /// <summary>
    /// Yatay ve dikey (zıplama) input'u işle
    /// </summary>
    private void HandleInput()
    {
        horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            // Yatay hareket
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput = 1f;

            // Zıplama tuşu kontrolü - Ayrıntılı debug
            var jumpControl = Keyboard.current[jumpKey];
            
            if (enableDebugLogs)
            {
                Debug.Log($"[Input] jumpKey={jumpKey} | control={jumpControl?.ToString() ?? "null"} | pressed={jumpControl?.wasPressedThisFrame ?? false}");
            }

            if (jumpControl != null && jumpControl.wasPressedThisFrame)
            {
                jumpBufferTimer = jumpBufferTime;
                Debug.Log($"[Input] ✓✓✓ Jump key '{jumpKey}' PRESSED - buffer={jumpBufferTime:F3}s");
            }
        }
        else
        {
            Debug.LogError("[Input] ✗✗✗ Keyboard.current is NULL - Input System not working!");
        }
    }

    private void UpdateTimers()
    {
        if (coyoteTimer > 0f) coyoteTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        
        // Detailed logging for jump timers
        if (enableDebugLogs && (jumpBufferTimer > 0f || coyoteTimer > 0f))
        {
            Debug.Log($"[Timers] jumpBuffer={jumpBufferTimer:F4} coyote={coyoteTimer:F4}");
        }
    }

    /// <summary>
    /// Basit collision-based zemin tespiti
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        groundContactCount++;
        if (enableDebugLogs) Debug.Log($"[Collision] Enter - contact count: {groundContactCount}");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Zemine temas sağlandı
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundContactCount--;
        if (groundContactCount <= 0)
        {
            groundContactCount = 0;
            if (enableDebugLogs) Debug.Log("[Collision] Exit - airborne");
        }
    }

    /// <summary>
    /// Her frame'de zemin durumunu güncelle
    /// </summary>
    private void UpdateGroundState()
    {
        bool wasGrounded = isGrounded;
        isGrounded = groundContactCount > 0;

        // Zemine çıktığımız framede coyote zamanını yenile
        if (isGrounded && !wasGrounded)
        {
            coyoteTimer = coyoteTime;
            if (enableDebugLogs) Debug.Log("[Ground] ✓ Landed - coyote time reset");
        }
        // Havaya çıktığımız framede coyote zamanını başlat (ilk defa)
        else if (!isGrounded && wasGrounded)
        {
            coyoteTimer = coyoteTime;
            if (enableDebugLogs) Debug.Log("[Ground] ✗ Left ground - coyote active");
        }
    }

    /// <summary>
    /// Yatay hareketi uygula (smoothly accelerate/decelerate)
    /// </summary>
    private void ApplyHorizontalMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float currentSpeed = rb.linearVelocity.x;
        float speedDiff = targetSpeed - currentSpeed;

        // Hızlanma veya yavaşlama oranı seç
        float accelRate = (Mathf.Abs(horizontalInput) > 0.01f) ? acceleration : deceleration;

        // Havada iken daha az kontrol
        accelRate *= isGrounded ? 1f : airControl;

        // Hızı yumuşakça değiştir
        float movement = Mathf.Clamp(speedDiff, -accelRate * Time.fixedDeltaTime, accelRate * Time.fixedDeltaTime);

        Vector2 newVel = rb.linearVelocity;
        newVel.x = Mathf.Clamp(currentSpeed + movement, -maxSpeed, maxSpeed);
        rb.linearVelocity = newVel;
    }

    /// <summary>
    /// Zıplama denemesi - coyote time ve jump buffer ile
    /// </summary>
    private void TryPerformJump()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[Jump Check] jumpBuffer={jumpBufferTimer:F3} coyote={coyoteTimer:F3} isGrounded={isGrounded} canJump={(jumpBufferTimer > 0f && (isGrounded || coyoteTimer > 0f))}");
        }

        if (jumpBufferTimer > 0f && (isGrounded || coyoteTimer > 0f))
        {
            PerformJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            if (enableDebugLogs) Debug.Log($"[Jump] ✓ PERFORMED");
        }
    }

    private void PerformJump()
    {
        Vector2 newVel = rb.linearVelocity;
        newVel.y = jumpForce;
        rb.linearVelocity = newVel;
        isGrounded = false;
    }

    /// <summary>
    /// Tuş bırakıldığında variable jump: kısa basış = kısa zıplama
    /// </summary>
    private void HandleJumpInputRelease()
    {
        if (Keyboard.current != null)
        {
            var jumpControl = Keyboard.current[jumpKey];
            if (jumpControl != null && jumpControl.wasReleasedThisFrame)
            {
                // Yukarı doğru gidiyorsa ve tuş bırakıldıysa, zıplamayı kısalt
                if (rb.linearVelocity.y > 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * lowJumpMultiplier);
                    if (enableDebugLogs) Debug.Log($"[Jump] Cut short (new_y={rb.linearVelocity.y:F2})");
                }
            }
        }
    }

    // ========== Public API ==========

    public bool IsGrounded() => isGrounded;
    public Vector2 GetVelocity() => rb.linearVelocity;
    public float GetHorizontalSpeed() => rb.linearVelocity.x;

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = Mathf.Max(0f, newSpeed);
        if (enableDebugLogs) Debug.Log($"[Move] Speed set to {newSpeed}");
    }

    public void SetJumpForce(float newForce)
    {
        jumpForce = Mathf.Max(0f, newForce);
        if (enableDebugLogs) Debug.Log($"[Move] Jump force set to {newForce}");
    }

    public void AddImpulse(Vector2 impulse)
    {
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    public void StopHorizontal()
    {
        Vector2 vel = rb.linearVelocity;
        vel.x = 0f;
        rb.linearVelocity = vel;
    }
}
