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
        anim = GetComponentInChildren<Animator >();

        heartUI = FindFirstObjectByType<HeartUI>();
        heartUI.AtualizarCoracoes(life);
    }

    void Update()
    {
        // 1. LEITURA DE MOVIMENTO
        moveX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        // 2. CAPTURA DO PULO
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // 3. CAPTURA DO ATAQUE (Testa o Clique, a tecla X ou a tecla J)
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyClick = Keyboard.current != null && (Keyboard.current.zKey.wasPressedThisFrame);

        if (mouseClick || keyClick)
        {
            Attack();
        }

        isGrounded = Physics2D.OverlapCircle(
        groundCheck.position,
        groundRadius,
        groundLayer
        );

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

        // Buscamos o objeto filho chamado "Skin"
        Transform skinTransform = transform.Find("Skin");

        if (skinTransform != null)
        {
            if (moveX > 0f)
            {
                // Olha para a DIREITA: Mantém a escala normal
                skinTransform.localScale = new Vector3(1f, 1f, 1f);

                // Força a Skin a ficar no centro zero do Player
                skinTransform.localPosition = new Vector3(0f, 0f, 0f);

                anim.SetBool("isRun", true);
            }
            else if (moveX < 0f)
            {
                // Olha para a ESQUERDA: Inverte a escala para espelhar
                skinTransform.localScale = new Vector3(-1f, 1f, 1f);

                // COMPENSAÇÃO VISUAL: Se ela pular para fora do colisor, mude o valor abaixo.
                // Altere o -0.3f para -0.2f, -0.4f, etc., até ela encaixar na cápsula olhando para a esquerda!
                skinTransform.localPosition = new Vector3(-0.3f, 0f, 0f);

                anim.SetBool("isRun", true);
            }
            else
            {
                anim.SetBool("isRun", false);
            }
        }
    }

    void HandleJumpLogic()
    {
        // SE ESTIVER NO CHÃO: Reseta a quantidade de pulos extras disponíveis
        if (isGrounded)
        {
            // Se o seu jogo tem pulo duplo, mude o 1 para 2, etc.
            addJumps = 1;
        }

        if (jumpRequested)
        {
            // Permite pular se estiver no chão OU se ainda tiver pulos extras
            if (isGrounded || addJumps > 0)
            {
                Jump();

                // Se o pulo foi no ar (pulo duplo), gasta um pulo extra
                if (!isGrounded)
                {
                    addJumps--;
                }
            }
            jumpRequested = false;
        }
    }


    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.SetBool("isJump", true);
    }

    void Attack()
    {
        // Força o Animator a tocar a animação instantaneamente
        anim.Play("Attack", -1, 0f);
    }

    public void AddLife(int quantidade)
    {
        life += quantidade;

        if (life > maxLife)
            life = maxLife;

        heartUI.AtualizarCoracoes(life);
    }

    public void TakeDamage(int dano)
    {
        life -= dano;

        if (life < 0)
            life = 0;

        heartUI.AtualizarCoracoes(life);

        if (life <= 0)
        {
            Debug.Log("Morreu");
        }
    }
}
