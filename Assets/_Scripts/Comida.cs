using UnityEngine;

/// <summary>
/// Component attached to every nutrient prefab instance.
/// Holds the energy value of this nutrient and handles pool return on pickup.
/// </summary>
public class Comida : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------
    private const float EnergiaBase = 40f;
    private const float EscalaEnergiaMultiplicador = 20f;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    /// <summary>Energy granted to a bacteria that picks this up.</summary>
    public float Energia { get; private set; } = EnergiaBase;

    //Llamada por el PoolComida al sacar un nuevo objeto para usar. Ajusta su escala y energía según el tamaño dado.
    public void Inicializar(float tamano)
    {
        float escalaFinal = Mathf.Max(0.1f, tamano);
        transform.localScale = Vector3.one * escalaFinal;
        Energia = EnergiaBase + escalaFinal * EscalaEnergiaMultiplicador;
    }

    /// <summary>Returns this nutrient to the pool.</summary>
    public void Devolver()
    {
        if (PoolComida.Instance != null)
            PoolComida.Instance.Devolver(gameObject);
        else
            gameObject.SetActive(false);
    }
}
