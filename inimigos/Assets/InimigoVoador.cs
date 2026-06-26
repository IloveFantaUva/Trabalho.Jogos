using UnityEngine;

public class InimigoVoador : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadePerseguicao = 3.5f;

    [Header("Área Delimitada (Perseguição)")]
    public float raioMaximoDistanciamento = 8f;
    private Vector2 posicaoInicial;

    [Header("Sensores de Obstáculos (Paredes)")]
    public Transform sensorParede;
    public float distanciaDoSensor = 0.4f;
    public LayerMask layerObstaculos;

    [Header("Detecção do Player e Ataque")]
    public Transform player;
    public float raioDeVisao = 6f;
    public float distanciaParaAtacar = 1.2f;
    public float tempoDeRecargaAtaque = 1.5f;
    private float cronometroAtaque = 0f;
    private bool estaAtacando = false;

    private Rigidbody2D rb;
    private Animator anim;
    private bool andandoParaDireita = true;

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

        ConfigurarModoFantasma();
    }

    void Update()
    {
        if (cronometroAtaque > 0)
        {
            cronometroAtaque -= Time.deltaTime;
        }

        if (estaAtacando)
        {
            rb.linearVelocity = Vector2.zero;

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
            FicarParado();
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
                if (cronometroAtaque <= 0) Atacar();
                else FicarParado();
            }
            else
            {
                PerseguirJogadorNoAr();
            }
        }
        else
        {
            FicarParado();
        }
    }

    bool TemLinhaDeVisaoLivre()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direcao, distanciaAtePlayer, layerObstaculos);
        return hit.collider == null;
    }

    void PerseguirJogadorNoAr()
    {
        anim.SetBool("isWalking", true);

        Vector2 direcaoDoPlayer = (player.position - transform.position).normalized;

        if (direcaoDoPlayer.x > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoDoPlayer.x < 0 && andandoParaDireita) InverterDirecao();

        rb.linearVelocity = direcaoDoPlayer * velocidadePerseguicao;
    }

    void VoltarParaOrigem()
    {
        anim.SetBool("isWalking", true);
        Vector2 direcaoOrigem = (posicaoInicial - (Vector2)transform.position).normalized;

        if (direcaoOrigem.x > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoOrigem.x < 0 && andandoParaDireita) InverterDirecao();

        rb.linearVelocity = direcaoOrigem * velocidadePerseguicao;

        if (Vector2.Distance(transform.position, posicaoInicial) < 0.2f)
        {
            transform.position = posicaoInicial;
            FicarParado();
        }
    }

    void FicarParado()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);
    }

    void Atacar()
    {
        estaAtacando = true;
        rb.linearVelocity = Vector2.zero;
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

    void ConfigurarModoFantasma()
    {
        Collider2D meuColisor = GetComponent<Collider2D>();
        if (meuColisor == null) return;

        // ATUALIZADO: Sintaxe limpa da Unity 6 para FindObjectsByType
        Collider2D[] todosColisores = FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Collider2D colisor in todosColisores)
        {
            if (colisor.gameObject != this.gameObject)
            {
                string nomeMinusculo = colisor.gameObject.name.ToLower();

                if (nomeMinusculo.Contains("cogu") ||
                    nomeMinusculo.Contains("goblin") ||
                    nomeMinusculo.Contains("esqueleto") ||
                    nomeMinusculo.Contains("enemy") ||
                    nomeMinusculo.Contains("inimigo") ||
                    colisor.GetComponent<InimigoVoador>() != null ||
                    colisor.GetComponent<InimigoSamurai>() != null)
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
        if (Application.isPlaying) Gizmos.DrawWireSphere(posicaoInicial, raioMaximoDistanciamento);
        else Gizmos.DrawWireSphere(transform.position, raioMaximoDistanciamento);
    }
}