using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public Image[] coracoes;

    public void AtualizarCoracoes(int vida)
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            coracoes[i].enabled = i < vida;
        }
    }
}