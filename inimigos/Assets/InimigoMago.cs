using UnityEngine;

public class InimigoMago : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadePerseguicao = 3f;
    private bool andandoParaDireita = true;

    [Header("Área Delimitada (Perseguição)")]
    public float raioMaximoDistanciamento = 8f;
    private Vector2 posicaoInicial;

    [Header("Detecção do Player e Ataque (Lança-Chamas)")]
    public Transform player;
    public float raioDeVisao = 5f;
    public float distanciaParaAtacar = 2.5f;
    public float tempoDeRecargaAtaque = 2f;
    public LayerMask layerObstaculos;
    private float cronometroAtaque = 0f;
    private bool estaAtacando = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D meuColisor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        meuColisor = GetComponent<Collider2D>();

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
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (cronometroAtaque <= (tempoDeRecargaAtaque - 1.2f))
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
                // CORREÇÃO: Só altera a direção se o player sair de uma "deadzone" horizontal de 0.5f
                float diferencaX = Mathf.Abs(player.position.x - transform.position.x);
                if (diferencaX > 0.5f)
                {
                    OlharParaAlvo(player.position.x);
                }

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
            // Se não está vendo o player, checa se precisa voltar
            float diferencaOrigemX = Mathf.Abs(posicaoInicial.x - transform.position.x);
            if (diferencaOrigemX > 0.4f)
            {
                VoltarParaOrigem();
            }
            else
            {
                FicarParado();
            }
        }
    }

    bool TemLinhaDeVisaoLivre()
    {
        Vector2 direcao = (player.position - transform.position).normalized;
        float distanciaAtePlayer = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direcao, distanciaAtePlayer, layerObstaculos);
        return hit.collider == null;
    }

    void PerseguirJogador()
    {
        anim.SetBool("isWalking", true);

        OlharParaAlvo(player.position.x);

        float vel = andandoParaDireita ? velocidadePerseguicao : -velocidadePerseguicao;
        rb.linearVelocity = new Vector2(vel, rb.linearVelocity.y);
    }

    void VoltarParaOrigem()
    {
        // CORREÇÃO: Foca estritamente na distância horizontal X para evitar bugs com elevação/Y
        float diferencaX = Mathf.Abs(posicaoInicial.x - transform.position.x);

        if (diferencaX <= 0.4f)
        {
            // Força o encaixe perfeito na origem e reseta a velocidade
            transform.position = new Vector2(posicaoInicial.x, transform.position.y);
            FicarParado();
            return;
        }

        anim.SetBool("isWalking", true);
        OlharParaAlvo(posicaoInicial.x);

        float vel = andandoParaDireita ? velocidadePerseguicao : -velocidadePerseguicao;
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

    void OlharParaAlvo(float alvoX)
    {
        float direcao = alvoX - transform.position.x;

        // CORREÇÃO: Só inverte se houver uma distância mínima real para evitar trepidação de frações de pixel
        if (Mathf.Abs(direcao) > 0.1f)
        {
            if (direcao > 0 && !andandoParaDireita) InverterDirecao();
            else if (direcao < 0 && andandoParaDireita) InverterDirecao();
        }
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
        if (meuColisor == null) return;

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
                    colisor.GetComponent<InimigoSamurai>() != null ||
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
        if (Application.isPlaying)
            Gizmos.DrawWireSphere(posicaoInicial, raioMaximoDistanciamento);
        else
            Gizmos.DrawWireSphere(transform.position, raioMaximoDistanciamento);
    }
}