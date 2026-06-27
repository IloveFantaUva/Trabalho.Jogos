using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    Character Player;

    // Start is called before the first frame update
    void Start()
    {
        // Procura o script 'Character' que está no objeto pai (o seu Triângulo)
        Player = gameObject.transform.parent.gameObject.GetComponent<Character>();
    }

    // Quando encosta no chão (Layer 8)
    void OnCollisionEnter2D(Collision2D collisor)
    {
        if (collisor.gameObject.layer == 8)
        {
            Player.isJumping = false; // Avisa que não está mais pulando
        }
    }

    // Quando deixa de encostar no chão
    void OnCollisionExit2D(Collision2D collisor)
    {
        if (collisor.gameObject.layer == 8)
        {
            Player.isJumping = true; // Avisa que começou a voar/pular
        }
    }
}