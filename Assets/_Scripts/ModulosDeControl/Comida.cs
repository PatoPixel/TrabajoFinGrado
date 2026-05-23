using UnityEngine;

/*
- Representa un objeto de comida que las bacterias pueden recoger para ganar energía.
- La energía que otorga se basa en su tamaño, con una cantidad base y un multiplicador.
- Tiene un método para inicializar su tamaño y energía, y otro para devolverlo a la pool cuando ya no se necesita.
*/

public class Comida : MonoBehaviour
{
    private const float EnergiaBase = 40f;
    private const float EscalaEnergiaMultiplicador = 20f;

    public float Energia { get; private set; } = EnergiaBase;

    //Llamada por el PoolComida al sacar un nuevo objeto para usar. Ajusta su escala y energia según el tamanno dado.
    public void Inicializar(float tamano)
    {
        float escalaFinal = Mathf.Max(0.1f, tamano);
        transform.localScale = Vector3.one * escalaFinal;
        Energia = EnergiaBase + escalaFinal * EscalaEnergiaMultiplicador;
    }

    // Devuelve el objeto a la pool para que pueda ser reutilizado, o lo desactiva si la pool no esta disponible
    public void Devolver()
    {
        if (PoolComida.Instance != null)
            PoolComida.Instance.Devolver(gameObject);
        else
            gameObject.SetActive(false);
    }
}
