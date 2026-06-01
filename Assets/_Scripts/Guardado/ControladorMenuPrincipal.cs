using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador para la escena de Menú Principal.
/// El panel siempre está visible, el juego nunca se pausa,
/// y conecta los botones a sus acciones de escena.
/// </summary>
public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelMenu;
    public GameObject panelListaPartidas;

    [Header("Botones")]
    public Button botonNuevaSimulacion;
    public Button botonCargarPartida;

    [Header("Referencias")]
    public GestorEscenas gestorEscenas;
    public GestorGuardado gestorGuardado;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelListaPartidas != null) panelListaPartidas.SetActive(false);

        // Cablear botones en runtime para no depender de eventos persistentes del Inspector
        if (botonNuevaSimulacion != null && gestorEscenas != null)
            botonNuevaSimulacion.onClick.AddListener(gestorEscenas.IrANuevaSimulacion);

        if (botonCargarPartida != null && gestorGuardado != null)
            botonCargarPartida.onClick.AddListener(gestorGuardado.abrirGestor);
    }
}
