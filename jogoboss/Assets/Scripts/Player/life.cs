using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    private void Start()
    {


    }

    private void Update()
    { 
        

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player.life < player.maxLife)
            {
                player.life++;

                FindFirstObjectByType<HeartUI>()
                    .AtualizarCoracoes(player.life);
            }

            Destroy(gameObject);
        }
    }
}
