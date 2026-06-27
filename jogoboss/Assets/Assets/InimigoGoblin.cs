using UnityEngine;
using System.Collections;

public class InimigoGoblin : MonoBehaviour
{
    private enum EstadoGoblin { Oculto, Emboscando, Atacando, Fugindo, Descansando }
    private EstadoGoblin estadoAtual = EstadoGoblin.Oculto;

    [Header("Configurações de Velocidade")]
    public float velocidadeFrenesi = 6f;
    public float velocidadeFuga = 5f;
    private bool andandoParaDireita = true;

    [Header("Área de Emboscada")]
    private Vector2 posicaoInicial;

    [Header("Configurações Invisível/Sutil")]
    [Range(0f, 1f)] public float opacidadeOculto = 0.2f;
    public float tempoInvisivelEscondido = 4.5f; // Tempo de delay (4 a 5 segundos) antes de poder atacar de novo
    private SpriteRenderer spriteRenderer;

    [Header("Sensores and Alvo")]
    public Transform player;
    public float raioDeVisao = 5f;
    public float distanciaParaAtacar = 1.2f;
    public LayerMask layerObstaculos;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D meuColisor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        meuColisor = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        posicaoInicial = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        EntrarEstadoOculto();
        ConfigurarModoFantasmaBase();
    }

    void Update()
    {
        if (estadoAtual == EstadoGoblin.Atacando || estadoAtual == EstadoGoblin.Descansando) return;

        VerificarComportamento();
    }

    void VerificarComportamento()
    {
        if (player == null)
        {
            if (estadoAtual != EstadoGoblin.Oculto) VoltarParaOrigem();
            return;
        }

        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);

        switch (estadoAtual)
        {
            case EstadoGoblin.Oculto:
                if (distanciaAtePlayer <= raioDeVisao && TemLinhaDeVisaoLivre())
                {
                    RevelarEPerseguir();
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
                break;

            case EstadoGoblin.Emboscando:
                if (distanciaAtePlayer <= distanciaParaAtacar)
                {
                    StartCoroutine(ExecutarComboDuplo());
                }
                else
                {
                    CorrerAtrasDoPlayer();
                }
                break;

            case EstadoGoblin.Fugindo:
                VoltarParaOrigem();
                break;
        }
    }

    bool TemLinhaDeVisaoLivre()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direcao, distanciaAtePlayer, layerObstaculos);
        return hit.collider == null;
    }

    void EntrarEstadoOculto()
    {
        estadoAtual = EstadoGoblin.Oculto;
        anim.SetBool("isWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = opacidadeOculto;
            spriteRenderer.color = c;
        }

        if (meuColisor != null) meuColisor.isTrigger = true;
    }

    void RevelarEPerseguir()
    {
        estadoAtual = EstadoGoblin.Emboscando;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        if (meuColisor != null) meuColisor.isTrigger = false;
    }

    void CorrerAtrasDoPlayer()
    {
        anim.SetBool("isWalking", true);

        float direcaoX = player.position.x - transform.position.x;
        if (direcaoX > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoX < 0 && andandoParaDireita) InverterDirecao();

        float vel = andandoParaDireita ? velocidadeFrenesi : -velocidadeFrenesi;
        rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
    }

    IEnumerator ExecutarComboDuplo()
    {
        estadoAtual = EstadoGoblin.Atacando;
        anim.SetBool("isWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // ---- ATAQUE 1 ----
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(0.4f);

        // ---- ATAQUE 2 ----
        anim.ResetTrigger("attack");
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(0.4f);

        estadoAtual = EstadoGoblin.Fugindo;
    }

    void VoltarParaOrigem()
    {
        float diferencaX = Mathf.Abs(posicaoInicial.x - transform.position.x);

        if (diferencaX <= 0.3f)
        {
            transform.position = new Vector2(posicaoInicial.x, transform.position.y);
            // CORREÇÃO: Em vez de ficar pronto para atacar, inicia o tempo de descanso/recarga
            StartCoroutine(DescansarNaToca());
            return;
        }

        anim.SetBool("isWalking", true);

        float direcaoOrigemX = posicaoInicial.x - transform.position.x;
        if (direcaoOrigemX > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoOrigemX < 0 && andandoParaDireita) InverterDirecao();

        float vel = andandoParaDireita ? velocidadeFuga : -velocidadeFuga;
        rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
    }

    // CORREÇÃO: Força ele a ficar invisível e inativo pelo tempo que você escolher antes de resetar o bote
    IEnumerator DescansarNaToca()
    {
        estadoAtual = EstadoGoblin.Descansando;

        // Aplica o visual invisível/fantasma imediatamente ao chegar
        anim.SetBool("isWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = opacidadeOculto;
            spriteRenderer.color = c;
        }
        if (meuColisor != null) meuColisor.isTrigger = true;

        // Espera o delay na toca (ajustável pelo Inspector em "Tempo Invisivel Escondido")
        yield return new WaitForSeconds(tempoInvisivelEscondido);

        // Só agora ele volta para o estado Oculto padrão que permite escanear o Player
        estadoAtual = EstadoGoblin.Oculto;
    }

    void InverterDirecao()
    {
        andandoParaDireita = !andandoParaDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    void ConfigurarModoFantasmaBase()
    {
        if (meuColisor == null) return;

        Collider2D[] todosColisores = FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Collider2D colisor in todosColisores)
        {
            if (colisor.gameObject != this.gameObject)
            {
                if (colisor.GetComponent<InimigoSamurai>() != null ||
                    colisor.GetComponent<InimigoVoador>() != null ||
                    colisor.GetComponent<InimigoMago>() != null)
                {
                    Physics2D.IgnoreCollision(meuColisor, colisor);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaParaAtacar);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDeVisao);

        Gizmos.color = Color.white;
        if (Application.isPlaying) Gizmos.DrawWireSphere(posicaoInicial, 0.5f);
    }
}