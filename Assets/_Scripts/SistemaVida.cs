using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    [Header("Configuración de Energía")]
    [SerializeField] private float energiaMaxima = 100f;
    //Stat de resistencia a membrana positiva (no usado en este script, pero puede ser útil para futuras mecánicas)
    //[SerializeField] private float membranaPositiva = 10;
    
    [SerializeField] private float consumoPorSegundo = 5f; // Cuánta hambre le entra por segundo


    [Header("Monitor (No tocar)")]
    [SerializeField] private float energiaActual; // Para ver en el inspector cuánta vida le queda

    public float EnergiaActual { get => energiaActual; set => energiaActual = value; }

    void Start()
    {
        // Al nacer, empezamos con la batería a tope
        energiaActual = energiaMaxima;
    }

    void Update()
    {
        // 1. Restar energía constantemente (Vivir cansa)
        // Usamos Time.deltaTime para que baje suavemente cada segundo
        energiaActual -= consumoPorSegundo * Time.deltaTime;

        // 2. Comprobar si ha muerto
        if (energiaActual <= 0)
        {
            Morir();
        }
    }

    // Esta función pública la llamaremos cuando toque comida
    public void Alimentar(float cantidad)
    {
        energiaActual += cantidad;
        // Mathf.Clamp asegura que no tengamos más energía de la máxima (no pasar de 100)
        energiaActual = Mathf.Clamp(energiaActual, 0, energiaMaxima);

        Debug.Log(gameObject.name + " ha comido! Energía: " + energiaActual);
    }

    private void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto de inanición.");
        // Destroy elimina el objeto del juego y libera la memoria RAM
        Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // DETECCIÓN DE COMIDA (FÍSICA)
    // ---------------------------------------------------------
    // Esta función mágica de Unity salta sola cuando entramos en un Trigger
    private void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        // ¿Lo que hemos tocado tiene la etiqueta "Comida"?
        if (otroObjeto.CompareTag("Comida"))
        {
            // 1. Recuperamos energía (ej: 30 puntos)
            Alimentar(30f);

            // 2. Destruimos la comida (porque ya nos la hemos comido)
            Destroy(otroObjeto.gameObject);
        }
    }
}