using UnityEngine;

public class InimigoCogumelo : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadePatrulha = 2f;
    public float velocidadePerseguicao = 3.5f;
    private bool andandoParaDireita = true;

    [Header("Área Delimitada (Perseguição)")]
    public float raioMaximoDistanciamento = 8f;
    private Vector2 posicaoInicial;

    [Header("Sensores de Borda e Obstáculos")]
    public Transform groundCheck;
    public float distanciaDoLaser = 0.5f;
    public float distanciaSensorParede = 0.5f;
    public LayerMask layerDoChao;
    public LayerMask layerObstaculos;

    [Header("Detecção do Player e Ataque")]
    public Transform player;
    public float raioDeVisao = 5f;
    public float distanciaParaAtacar = 1.2f;
    public float tempoDeRecargaAtaque = 1.5f;
    private float cronometroAtaque = 0f;
    private bool estaAtacando = false;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        posicaoInicial = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (cronometroAtaque > 0)
        {
            cronometroAtaque -= Time.deltaTime;
        }

        if (estaAtacando)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (cronometroAtaque <= (tempoDeRecargaAtaque - 0.8f))
            {
                FinalizarAtaque();
            }
            return;
        }

        VerificarComportamento();
    }

    void VerificarComportamento()
    {
        if (player == null)
        {
            Patrulhar();
            return;
        }

        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);
        float distanciaDaOrigem = Vector2.Distance(posicaoInicial, transform.position);

        if (distanciaDaOrigem > raioMaximoDistanciamento)
        {
            VoltarParaOrigem();
            return;
        }

        if (distanciaAtePlayer <= raioDeVisao && TemLinhaDeVisaoLivre())
        {
            if (distanciaAtePlayer <= distanciaParaAtacar)
            {
                if (cronometroAtaque <= 0)
                {
                    Atacar();
                }
                else
                {
                    FicarParado();
                }
            }
            else
            {
                PerseguirJogador();
            }
        }
        else
        {
            Patrulhar();
        }
    }

    bool TemLinhaDeVisaoLivre()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direcao, distanciaAtePlayer, layerObstaculos);
        return hit.collider == null;
    }

    void Patrulhar()
    {
        // Dispara o sensor do peito (Y + 0.5f) e aceita tanto a layerObstaculos quanto a layerDoChao (groud)
        Vector2 origemSensorParede = new Vector2(transform.position.x, transform.position.y + 0.5f);
        Vector2 direcaoOlhar = andandoParaDireita ? Vector2.right : Vector2.left;

        int mascaraCombinada = layerObstaculos | layerDoChao;
        RaycastHit2D temParede = Physics2D.Raycast(origemSensorParede, direcaoOlhar, distanciaSensorParede, mascaraCombinada);
        bool temChaoAFrente = Physics2D.Raycast(groundCheck.position, Vector2.down, distanciaDoLaser, layerDoChao);

        if (!temChaoAFrente || (temParede.collider != null && !temParede.collider.isTrigger))
        {
            InverterDirecao();
        }

        float vel = andandoParaDireita ? velocidadePatrulha : -velocidadePatrulha;
        rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
        anim.SetBool("isWalking", true);
    }

    void PerseguirJogador()
    {
        anim.SetBool("isWalking", true);

        float direcaoDoPlayer = player.position.x - transform.position.x;

        if (direcaoDoPlayer > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoDoPlayer < 0 && andandoParaDireita) InverterDirecao();

        Vector2 origemSensorParede = new Vector2(transform.position.x, transform.position.y + 0.5f);
        Vector2 direcaoOlhar = andandoParaDireita ? Vector2.right : Vector2.left;

        int mascaraCombinada = layerObstaculos | layerDoChao;
        RaycastHit2D temParede = Physics2D.Raycast(origemSensorParede, direcaoOlhar, distanciaSensorParede, mascaraCombinada);
        bool temChaoAFrente = Physics2D.Raycast(groundCheck.position, Vector2.down, distanciaDoLaser, layerDoChao);

        if (temChaoAFrente && temParede.collider == null)
        {
            float vel = andandoParaDireita ? velocidadePerseguicao : -velocidadePerseguicao;
            rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }
    }

    void VoltarParaOrigem()
    {
        anim.SetBool("isWalking", true);
        float direcaoParaOrigem = posicaoInicial.x - transform.position.x;

        if (direcaoParaOrigem > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoParaOrigem < 0 && andandoParaDireita) InverterDirecao();

        Vector2 origemSensorParede = new Vector2(transform.position.x, transform.position.y + 0.5f);
        Vector2 direcaoOlhar = andandoParaDireita ? Vector2.right : Vector2.left;

        int mascaraCombinada = layerObstaculos | layerDoChao;
        RaycastHit2D temParede = Physics2D.Raycast(origemSensorParede, direcaoOlhar, distanciaSensorParede, mascaraCombinada);

        if (temParede.collider != null && !temParede.collider.isTrigger)
        {
            FicarParado();
            return;
        }

        float vel = andandoParaDireita ? velocidadePatrulha : -velocidadePatrulha;
        rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
    }

    void FicarParado()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isWalking", false);
    }

    void Atacar()
    {
        estaAtacando = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        anim.SetBool("isWalking", false);
        anim.ResetTrigger("attack");
        anim.SetTrigger("attack");

        cronometroAtaque = tempoDeRecargaAtaque;
    }

    public void FinalizarAtaque()
    {
        estaAtacando = false;
    }

    void InverterDirecao()
    {
        andandoParaDireita = !andandoParaDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * distanciaDoLaser);
        }

        Gizmos.color = Color.purple;
        Vector3 origemGizmo = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Vector3 direcao = andandoParaDireita ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(origemGizmo, origemGizmo + direcao * distanciaSensorParede);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaParaAtacar);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDeVisao);

        Gizmos.color = Color.white;
        if (Application.isPlaying)
            Gizmos.DrawWireSphere(posicaoInicial, raioMaximoDistanciamento);
        else
            Gizmos.DrawWireSphere(transform.position, raioMaximoDistanciamento);
    }
}