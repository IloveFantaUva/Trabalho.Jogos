using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float distanciaAtivacao = 5f;
    public float distanciaAtaque = 1.5f;
    public float velocidadePerseguicao = 2f;
    public int vidaBoss = 10; // VIDA DO BOSS

    private Animator anim;
    private bool estaAtivo = false;
    private bool podePerseguir = false;
    private bool podeSeMover = false;

    void Start() { anim = GetComponent<Animator>(); }

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
        else if (podePerseguir && player != null) PerseguirPlayer();
    }

    IEnumerator EsperarParaSurgir(float tempo) { podeSeMover = false; yield return new WaitForSeconds(tempo); anim.SetTrigger("Surgir"); }
    public void TerminouDeNascer() { podePerseguir = true; podeSeMover = true; }

    void PerseguirPlayer()
    {
        bool estaAtacando = anim.GetCurrentAnimatorStateInfo(0).IsName("Atacar");
        if (!podeSeMover || estaAtacando) { anim.SetBool("Andando", false); return; }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= distanciaAtaque) { anim.SetBool("Andando", false); anim.SetTrigger("Atacar"); return; }

        anim.SetBool("Andando", true);
        transform.position = Vector2.MoveTowards(transform.position, player.position, velocidadePerseguicao * Time.deltaTime);

        float direcao = player.position.x - transform.position.x;
        if (direcao != 0) transform.localScale = new Vector3(-Mathf.Sign(direcao), 1, 1);
    }

    // MÉTODO DE DANO RECEBIDO
    public void TomarDanoBoss(int dano)
    {
        vidaBoss -= dano;
        Debug.Log("Vida do Boss: " + vidaBoss);
        if (vidaBoss <= 0) Destroy(gameObject);
    }

    // DANO CAUSADO AO PLAYER
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerScript = collision.GetComponent<PlayerController>();
            if (playerScript != null) playerScript.TakeDamage(1);
        }
    }
}