using UnityEngine;

public class ControladorMenuPausa : MonoBehaviour
{


    [Header("Referencias UI")]
    public GameObject panelPausa; // Arrastra aquí el panel contenedor de tu menú

    public static bool juegoPausado = false;
    private float velocidadAnterior = 1f; // Para recordar si el jugador estaba a x1, x2, etc.

    [Header("Referencias Sub-Menús")]
    public GameObject panelListaPartidas;      // El panel del Gestor de Guardado

    private void Start()
    {
        // Asegurarnos de que el menú de pausa esté oculto al iniciar
        panelPausa.SetActive(false);
    }


    // ... (Mantienes tus variables juegoPausado y velocidadAnterior igual) ...

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Escenario 1: El juego está pausado Y estamos dentro del sub-menú de partidas
            if (juegoPausado && panelListaPartidas.activeSelf)
            {
                // Hacemos por código exactamente lo mismo que hace tu botón "X"
                CerrarSubMenu();
                panelPausa.SetActive(true);
            }
            // Escenario 2: Estamos jugando normal, o en el menú de pausa principal
            else
            {
                AlternarPausa();
            }
        }
    }

    // He creado esta función por si en el futuro añades un menú de Ajustes. 
    // Así todo se centraliza aquí.
    private void CerrarSubMenu()
    {
        panelListaPartidas.SetActive(false);
    }

    // Este método lo llamaremos tanto desde el Update como desde el botón de las 3 líneas
    public void AlternarPausa()
    {
        juegoPausado = !juegoPausado;

        if (juegoPausado)
        {
            PausarJuego();
        }
        else
        {
            ReanudarJuego();
        }
    }

    private void PausarJuego()
    {
        // Encendemos la UI
        panelPausa.SetActive(true);

        // Guardamos a qué velocidad iba el juego antes de pausar
        if (Time.timeScale > 0f)
        {
            velocidadAnterior = Time.timeScale;
        }

        // Congelamos el mundo
        Time.timeScale = 0f;
    }

    // La hacemos pública por si quieres poner un botón de "Continuar" en el menú
    public void ReanudarJuego()
    {
        // Apagamos la UI
        panelPausa.SetActive(false);

        // Devolvemos el juego a la velocidad que tenía
        Time.timeScale = velocidadAnterior;

        juegoPausado = false;
    }
}