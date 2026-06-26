using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float distanciaAtivacao = 5f;
    public float distanciaAtaque = 1.5f;
    public float velocidadePerseguicao = 2f;

    private Animator anim;
    private bool estaAtivo = false;
    private bool podePerseguir = false;
    private bool podeSeMover = false; // Trava de segurança principal

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!estaAtivo && player != null)
        {
            if (Vector2.Distance(transform.position, player.position) <= distanciaAtivacao)
            {
                estaAtivo = true;
                StartCoroutine(EsperarParaSurgir(2.0f));
            }
        }
        else if (podePerseguir && player != null)
        {
            // O movimento agora é controlado unicamente pelo método PerseguirPlayer
            PerseguirPlayer();
        }
    }

    IEnumerator EsperarParaSurgir(float tempo)
    {
        podeSeMover = false; // Garante que ele fique parado enquanto espera
        yield return new WaitForSeconds(tempo);
        anim.SetTrigger("Surgir");
    }

    // Chamado via Animation Event ao final da animação 'surgir_boss'
    public void TerminouDeNascer()
    {
        podePerseguir = true;
        podeSeMover = true; // Agora ele pode começar a perseguir
    }

    void PerseguirPlayer()
    {
        // 1. CHECAGEM DE SEGURANÇA: Se não pode se mover OU está atacando, para tudo.
        bool estaAtacando = anim.GetCurrentAnimatorStateInfo(0).IsName("Atacar");

        if (!podeSeMover || estaAtacando)
        {
            anim.SetBool("Andando", false);
            return; // Sai do método antes de processar qualquer movimento
        }

        // 2. LÓGICA DE ATAQUE
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= distanciaAtaque)
        {
            anim.SetBool("Andando", false);
            anim.SetTrigger("Atacar");
            return; // Ataca e não se move neste frame
        }

        // 3. LÓGICA DE PERSEGUIÇÃO
        anim.SetBool("Andando", true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, velocidadePerseguicao * Time.deltaTime);

        // Flip do sprite (invertido conforme sua necessidade)
        float direcao = player.position.x - transform.position.x;
        if (direcao != 0)
        {
            transform.localScale = new Vector3(-Mathf.Sign(direcao), 1, 1);
        }
    }
}