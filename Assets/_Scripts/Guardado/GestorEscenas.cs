using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona las transiciones entre escenas y pasa datos entre ellas.
/// Se coloca en la escena MenuPrincipal. Los métodos estáticos son accesibles desde cualquier escena.
/// </summary>
public class GestorEscenas : MonoBehaviour
{
    public const string ESCENA_GAMEPLAY = "GamePlay";
    public const string ESCENA_MENU = "MenuPrincipal";
    public const string ESCENA_TUTORIAL = "Tutorial";

    /// <summary>Nombre de la partida a cargar al entrar en GamePlay. Vacío = nueva simulación.</summary>
    public static string PartidaACargar { get; private set; } = "";

    /// <summary>Si es true, GamePlay arrancará en modo tutorial.</summary>
    public static bool ModoTutorial { get; private set; } = false;

    // --- Llamado desde el botón "Nueva Simulación" ---
    public void IrANuevaSimulacion()
    {
        PartidaACargar = "";
        SceneManager.LoadScene(ESCENA_GAMEPLAY);
    }

    // --- Llamado desde FichaPartidaUI cuando se está en MenuPrincipal ---
    public static void IrACargarPartida(string nombrePartida)
    {
        PartidaACargar = nombrePartida;
        SceneManager.LoadScene(ESCENA_GAMEPLAY);
    }

    /// <summary>Llamado por GestorGuardado en GamePlay tras cargar la partida.</summary>
    public static void LimpiarPartidaACargar()
    {
        PartidaACargar = "";
    }

    // --- Llamado desde el botón "Tutorial" del Menú Principal ---
    public void IrATutorial()
    {
        PartidaACargar = "";
        ModoTutorial = false; // No hace falta: la escena Tutorial siempre activa el tutorial
        SceneManager.LoadScene(ESCENA_TUTORIAL);
    }

    public static void LimpiarModoTutorial()
    {
        ModoTutorial = false;
    }

    // --- Llamado desde GestorGuardado cuando el jugador confirma volver al menú ---
    public static void IrAMenuPrincipal()
    {
        Time.timeScale = 1f; // Por si el juego estaba pausado
        SceneManager.LoadScene(ESCENA_MENU);
    }
}
