using UnityEngine;

public class InimigoSamurai : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadPatrulha = 2f;

    [Header("Ataque Investida (Anime Dash)")]
    public float velocidadeDash = 15f;
    public float tempoDeDash = 0.3f;
    public float tempoDeRecargaAtaque = 2f;
    private float cronometroAtaque = 0f;
    private bool estaAtacando = false;
    private float gravidadeOriginal;

    [Header("Sensores (Chão, Visão e Paredes)")]
    public Transform groundCheck;
    public float distanciaDoLaser = 0.5f;
    public float distanciaSensorParede = 0.5f;
    public LayerMask layerDoChao;
    public LayerMask layerObstaculos;
    public Transform player;
    public float raioDeVisao = 5f;
    public float distanciaParaAtacar = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D meuColisor;
    private bool andandoParaDireita = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        meuColisor = GetComponent<Collider2D>();
        gravidadeOriginal = rb.gravityScale;

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

        if (estaAtacando) return;

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

        if (distanciaAtePlayer <= raioDeVisao && TemLinhaDeVisaoLivre())
        {
            if (distanciaAtePlayer <= distanciaParaAtacar && cronometroAtaque <= 0)
            {
                StartCoroutine(ExecutarAtaqueDash());
            }
            else if (!estaAtacando)
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
        if (groundCheck == null) return;

        Vector2 origemSensorParede = new Vector2(transform.position.x, transform.position.y + 0.5f);
        Vector2 direcaoOlhar = andandoParaDireita ? Vector2.right : Vector2.left;

        int mascaraCombinada = layerObstaculos | layerDoChao;
        RaycastHit2D temParede = Physics2D.Raycast(origemSensorParede, direcaoOlhar, distanciaSensorParede, mascaraCombinada);
        bool temChao = Physics2D.Raycast(groundCheck.position, Vector2.down, distanciaDoLaser, layerDoChao);

        if (!temChao || (temParede.collider != null && !temParede.collider.isTrigger))
        {
            InverterDirecao();
        }

        anim.SetBool("isWalking", true);
        float direcao = andandoParaDireita ? 1 : -1;
        rb.linearVelocity = new Vector2(direcao * velocidadPatrulha, rb.linearVelocity.y);
    }

    void PerseguirJogador()
    {
        float direcaoX = player.position.x - transform.position.x;

        if (direcaoX > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoX < 0 && andandoParaDireita) InverterDirecao();

        Vector2 origineSensorParede = new Vector2(transform.position.x, transform.position.y + 0.5f);
        Vector2 direcaoOlhar = andandoParaDireita ? Vector2.right : Vector2.left;

        int mascaraCombinada = layerObstaculos | layerDoChao;
        RaycastHit2D temParede = Physics2D.Raycast(origineSensorParede, direcaoOlhar, distanciaSensorParede, mascaraCombinada);

        if (temParede.collider == null)
        {
            anim.SetBool("isWalking", true);
            float direcao = andandoParaDireita ? 1 : -1;
            rb.linearVelocity = new Vector2(direcao * velocidadPatrulha, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isWalking", false);
        }
    }

    System.Collections.IEnumerator ExecutarAtaqueDash()
    {
        estaAtacando = true;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("attack");

        float direcaoX = player.position.x - transform.position.x;
        if (direcaoX > 0 && !andandoParaDireita) InverterDirecao();
        else if (direcaoX < 0 && andandoParaDireita) InverterDirecao();

        float direcaoDash = andandoParaDireita ? 1f : -1f;

        Collider2D[] playerColliders = player.GetComponents<Collider2D>();
        if (meuColisor != null)
        {
            foreach (Collider2D pCol in playerColliders)
            {
                Physics2D.IgnoreCollision(meuColisor, pCol, true);
            }
        }

        if (meuColisor != null) meuColisor.isTrigger = true;

        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(direcaoDash * velocidadeDash, 0);

        yield return new WaitForSeconds(tempoDeDash);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravidadeOriginal;

        if (meuColisor != null) meuColisor.isTrigger = false;

        if (meuColisor != null)
        {
            foreach (Collider2D pCol in playerColliders)
            {
                Physics2D.IgnoreCollision(meuColisor, pCol, false);
            }
        }

        cronometroAtaque = tempoDeRecargaAtaque;
        estaAtacando = false;

        InverterDirecao();
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

        Collider2D[] todosColisores = Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include);

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
                    colisor.GetComponent<InimigoSamurai>() != null)
                {
                    Physics2D.IgnoreCollision(meuColisor, colisor);
                }
            }
        }
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioDeVisao);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaParaAtacar);
    }
}