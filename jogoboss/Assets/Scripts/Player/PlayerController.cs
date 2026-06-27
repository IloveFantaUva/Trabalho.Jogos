using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float speed;
    public float jumpForce;
    public int addJumps;
    public bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveX;
    private bool jumpRequested;
    public int life;
    public int maxLife = 3;
    private HeartUI heartUI;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        heartUI = FindFirstObjectByType<HeartUI>();
        heartUI.AtualizarCoracoes(life);
    }

    void Update()
    {
        moveX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpRequested = true;

        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyClick = Keyboard.current != null && (Keyboard.current.zKey.wasPressedThisFrame);

        if (mouseClick || keyClick) Attack();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        anim.SetBool("isJump", !isGrounded);
    }

    void FixedUpdate()
    {
        Move();
        HandleJumpLogic();
    }

    void Move()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
        Transform skinTransform = transform.Find("Skin");
        if (skinTransform != null)
        {
            if (moveX > 0f) { skinTransform.localScale = new Vector3(1f, 1f, 1f); skinTransform.localPosition = Vector3.zero; anim.SetBool("isRun", true); }
            else if (moveX < 0f) { skinTransform.localScale = new Vector3(-1f, 1f, 1f); skinTransform.localPosition = new Vector3(-0.3f, 0f, 0f); anim.SetBool("isRun", true); }
            else { anim.SetBool("isRun", false); }
        }
    }

    void HandleJumpLogic()
    {
        if (isGrounded) addJumps = 1;
        if (jumpRequested)
        {
            if (isGrounded || addJumps > 0) { Jump(); if (!isGrounded) addJumps--; }
            jumpRequested = false;
        }
    }

    void Jump() { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); anim.SetBool("isJump", true); }
    void Attack() { anim.Play("Attack", -1, 0f); }

    // DETECÇÃO DE DANO NO BOSS
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            BossController boss = collision.GetComponent<BossController>();
            if (boss != null) boss.TomarDanoBoss(1);
        }
    }

    public void AddLife(int quantidade) { life += quantidade; if (life > maxLife) life = maxLife; heartUI.AtualizarCoracoes(life); }
    public void TakeDamage(int dano) { life -= dano; if (life < 0) life = 0; heartUI.AtualizarCoracoes(life); if (life <= 0) Debug.Log("Morreu"); }
}