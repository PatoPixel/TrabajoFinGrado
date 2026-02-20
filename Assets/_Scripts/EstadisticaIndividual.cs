using UnityEngine;

public class CajonIndividual : MonoBehaviour
{
    [Header("¿Qué objeto apaga este botón?")]
    [SerializeField] private GameObject contenidoAOcultar;

    private bool estaAbierto = true;

    // Esta función la usará CADA botón por separado
    public void Alternar()
    {
        estaAbierto = !estaAbierto;

        if (contenidoAOcultar != null)
        {
            contenidoAOcultar.SetActive(estaAbierto);
        }
    }
}