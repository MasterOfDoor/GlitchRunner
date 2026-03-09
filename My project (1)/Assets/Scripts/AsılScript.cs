using UnityEngine;
using System.Collections;

public class AsılScript : MonoBehaviour // Dosya adınla aynı olmalı!
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float dashForce = 20f;

    [Header("Silah ve Mermi Ayarları")]
    public int maxAmmo = 30;
    public float reloadTime = 2f;
    private int currentAmmo;
    private bool isReloading = false;

    // --- HATAYI ÇÖZEN DEĞİŞKENLER BURADA ---
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private float currentSpeed;
    private int weaponType = 0; // 0=Normal, 1=Gun, 2=Spear
    // ---------------------------------------

    void Start()
    {
        // Bilgisayara bu isimlerin ne olduğunu öğretiyoruz
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // 1. HAREKET GİRDİLERİ
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // 2. KOŞMA KONTROLÜ (R Tuşu)
        bool isRunning = Input.GetKey(KeyCode.R);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 3. SİLAH DEĞİŞTİRME (1, 2, 3)
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponType = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponType = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponType = 2;

        // 4. DASH (Sol Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            anim.SetTrigger("doDash");
            rb.AddForce(moveInput * dashForce, ForceMode2D.Impulse);
        }

        // 5. ZIPLAMA (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("doJump");
        }

        // 6. SALDIRI / ATEŞ (Sol Tık)
        HandleAttack();

        // 7. ANIMATOR GÜNCELLEME
        UpdateAnimator();
    }

    void HandleAttack()
    {
        if (weaponType == 1) // GUN
        {
            if (Input.GetMouseButton(0) && currentAmmo > 0 && !isReloading)
            {
                anim.SetBool("isShooting", true);
                if (Time.frameCount % 10 == 0) 
                {
                    currentAmmo--;
                }
            }
            else
            {
                anim.SetBool("isShooting", false);
            }

            if ((currentAmmo <= 0 || (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)) && !isReloading)
            {
                StartCoroutine(ReloadRoutine());
            }
        }
        else if (weaponType == 2) // SPEAR
        {
            if (Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("doAttack");
            }
        }
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        anim.SetBool("isShooting", false);
        anim.SetBool("isReloading", true);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        anim.SetBool("isReloading", false);
        isReloading = false;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimator()
    {
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);

        float animSpeed = moveInput.magnitude;
        if (animSpeed > 0)
        {
            animSpeed = Input.GetKey(KeyCode.R) ? 1f : 0.5f;
        }
        anim.SetFloat("Speed", animSpeed);
        anim.SetInteger("WeaponType", weaponType);
        
        bool isAiming = Input.GetMouseButton(1);
        anim.SetBool("isAiming", isAiming);
    }
}