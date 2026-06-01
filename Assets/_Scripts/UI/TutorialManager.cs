using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-200)]
public class TutorialManager : MonoBehaviour
{
    public static bool TutorialActivo => _instance != null && _instance._activo;
    public static bool BloquearCambiarTiempo { get; private set; } = true;

    private static TutorialManager _instance;

    [Header("Configuracion")]
    public bool siempreActivo = false;

    [Header("Panel Tutorial UI")]
    public CanvasGroup overlay;
    public Image overlayFondo;          // La imagen oscura hija de PanelTutorial (para controlar raycast sin quitar el overlay)
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoMensaje;
    public Button botonSiguiente;
    public TextMeshProUGUI textoBotonSiguiente;

    [Header("UI del Juego a controlar")]
    public GameObject uiControladorVelocidad;
    public GameObject uiBotonesControles;
    public GameObject uiBandejaEspecies;
    public GameObject uiHudEvolucion;
    public GameObject uiPanelInspector;

    // ── Estado ─────────────────────────────────────────────────────────────
    private bool _activo = false;
    private int _pasoActual = -1;
    private bool _esperandoAccion = false;
    private AccionRequerida _accionActual;
    private ControladorTiempo _controladorTiempo;
    private ControladorInteraccion _controladorInteraccion;
    private bool _camaraCentrada   = false;
    private bool _bacteriaColocada = false;
    private bool _comidaColocada   = false;
    private bool _spawnerColocado  = false;

    private const float TIEMPO_MINIMO_LECTURA = 2.5f;
    private float _timerLectura = 0f;

    private enum AccionRequerida
    {
        Ninguna,
        CambiarTiempo,
        MoverCamara,
        HacerZoom,
        SeleccionarBacteria,
        AbrirLaboratorio,
        ActivarHerramienta,
        CentrarCamara,
        ColocarBacteria,
        ColocarComida,
        ColocarSpawner
    }

    private class Paso
    {
        public string titulo;
        public string mensaje;
        public bool botonVisible = true;
        public AccionRequerida accion = AccionRequerida.Ninguna;
        /// <summary>Si true, el overlay no bloquea el raycast para que el jugador pueda interactuar con la UI del juego.</summary>
        public bool interactivo = false;
        public System.Action onEntrar;
    }

    private List<Paso> _pasos;

    // ── Ciclo de vida ───────────────────────────────────────────────────────

    private void Awake()
    {
        _instance = this;
        BloquearCambiarTiempo = true;

        bool activar = siempreActivo || GestorEscenas.ModoTutorial;
        if (!activar)
        {
            if (uiBotonesControles) uiBotonesControles.SetActive(true);
            if (uiHudEvolucion)     uiHudEvolucion.SetActive(true);
            if (overlay)            overlay.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        GestorEscenas.LimpiarModoTutorial();
        _activo = true;
        // Resetear ambos valores de tiempo para evitar que fixedDeltaTime de una sesión anterior rompa la física
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f; // valor por defecto de Unity

        if (uiBotonesControles) uiBotonesControles.SetActive(false);
        if (uiBandejaEspecies)  uiBandejaEspecies.SetActive(false);
        if (uiHudEvolucion)     uiHudEvolucion.SetActive(false);
        if (uiPanelInspector)   uiPanelInspector.SetActive(false);
    }

    private void Start()
    {
        if (!_activo) return;

        _controladorTiempo = FindFirstObjectByType<ControladorTiempo>();
        _controladorInteraccion = FindFirstObjectByType<ControladorInteraccion>();
        CamaraControladorPro.OnIntervencionManual      += OnCamaraMovida;
        ControladorInteraccion.OnCamaraCentrada        += OnDetectarCamaraCentrada;
        ControladorInteraccion.OnBacteriaColocada      += () => _bacteriaColocada = true;
        ControladorInteraccion.OnComidaColocada        += () => _comidaColocada   = true;
        ControladorInteraccion.OnSpawnerColocado       += () => _spawnerColocado  = true;

        Camera cam = Camera.main;
        if (cam != null) cam.orthographicSize = 6f;

        _pasos = new List<Paso>
        {
            // ── 0: Bienvenida ──────────────────────────────────────────────
            new Paso {
                titulo   = "Bienvenido al Simulador de Bacterias",
                mensaje  = "Estas observando una <b>placa de Petri</b> con bacterias vivas.\n\n" +
                           "Veras como se alimentan, reproducen y evolucionan de forma autonoma.\n\n" +
                           "Este tutorial te guiara por todas las mecanicas.",
                botonVisible = true
            },

            // ── 1: Control del tiempo ──────────────────────────────────────
            new Paso {
                titulo   = "Control de la Simulacion",
                mensaje  = "La simulacion esta <b>pausada</b>. Puedes controlar la velocidad con <b>T</b>:\n\n" +
                           "Pausa  <b>0x</b>\nVelocidad normal  <b>1x</b>\nVelocidad rapida  <b>5x</b>\n\n" +
                           "<color=#FFD700>Pulsa <b>T</b> para activar la simulacion.</color>",
                botonVisible = false,
                accion   = AccionRequerida.CambiarTiempo,
                onEntrar = () => BloquearCambiarTiempo = false
            },

            // ── 2: Camara WASD ─────────────────────────────────────────────
            new Paso {
                titulo   = "Moverse por la Placa",
                mensaje  = "Muevete por la placa con <b>W A S D</b> o las flechas.\n\n" +
                           "Tambien puedes arrastrar con el <b>boton central del raton</b>.\n\n" +
                           "<color=#FFD700>Muevete para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.MoverCamara
            },

            // ── 3: Zoom ────────────────────────────────────────────────────
            new Paso {
                titulo   = "Zoom",
                mensaje  = "Usa la <b>rueda del raton</b> para acercarte o alejarte de la placa.\n\n" +
                           "<color=#FFD700>Haz scroll para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.HacerZoom
            },

            // ── 4: Las bacterias ───────────────────────────────────────────
            new Paso {
                titulo   = "Las Bacterias",
                mensaje  = "Cada bacteria tiene <b>genes</b> unicos que definen su comportamiento:\n\n" +
                           "<b>Velocidad</b> - que tan rapido se mueve\n" +
                           "<b>Vision</b> - radio de deteccion de comida y enemigos\n" +
                           "<b>Tamano</b> - una bacteria mas grande devora a una mas pequena\n" +
                           "<b>Consumo</b> - energia gastada por segundo\n" +
                           "<b>Energia maxima</b> - cuanta energia puede almacenar\n" +
                           "<b>Tiempo de vida</b> - edad maxima que puede alcanzar; al superarla, la probabilidad de morir aumenta progresivamente\n" +
                           "<b>Coste de reproduccion</b> - energia necesaria para reproducirse\n" +
                           "<b>Tiempo entre reproducciones</b> - cooldown minimo entre cada reproduccion",
                botonVisible = true,
                onEntrar = () => {
                    if (_instance.uiBotonesControles) _instance.uiBotonesControles.SetActive(true);
                }
            },

            // ── 5: Reproduccion y evolucion ────────────────────────────────
            new Paso {
                titulo   = "Reproduccion y Evolucion",
                mensaje  = "Cuando una bacteria acumula suficiente energia, se <b>reproduce asexualmente</b>.\n\n" +
                           "La cria hereda los genes de la madre con <b>pequenas mutaciones aleatorias</b>.\n\n" +
                           "Con el tiempo, las bacterias mejor adaptadas dominan la placa. Asi emerge la <b>evolucion natural</b>.",
                botonVisible = true
            },

            // ── 6: Inspeccionar bacteria ───────────────────────────────────
            new Paso {
                titulo   = "Inspeccionar una Bacteria",
                mensaje  = "Haz <b>click izquierdo</b> sobre la bacteria para ver su informacion detallada.\n\n" +
                           "Veras su energia actual, estadisticas geneticas y linaje.\n\n" +
                           "<color=#FFD700>Haz click en la bacteria para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.SeleccionarBacteria,
                interactivo = true,
                onEntrar = () => {
                    ControladorInteraccion inter = Object.FindFirstObjectByType<ControladorInteraccion>();
                    if (inter != null) inter.ActivarModoInspeccion();
                }
            },

            // ── 7: Herramienta Menu ────────────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Menu",
                mensaje  = "El primer boton de la barra lateral abre el <b>menu de pausa</b>.\n\n" +
                           "Desde ahi puedes guardar la partida, cargar una anterior o volver al menu principal.\n\n" +
                           "Tambien puedes abrirlo en cualquier momento con <b>Escape</b>.",
                botonVisible = true
            },

            // ── 8: Herramienta Mover ───────────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Mover",
                mensaje  = "El boton del <b>raton</b> activa el modo de movimiento.\n\n" +
                           "En este modo puedes desplazarte libremente por la placa <b>sin riesgo de seleccionar</b> bacterias accidentalmente al hacer click.",
                botonVisible = true
            },

            // ── 9: Herramienta Lupa ────────────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Inspeccionar",
                mensaje  = "El boton de la <b>lupa</b> activa el modo de inspeccion.\n\n" +
                           "En este modo, al hacer click sobre una bacteria se abre su panel de informacion con todos sus datos geneticos en tiempo real.",
                botonVisible = true
            },

            // ── 10: Herramienta Laboratorio ────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Laboratorio de Especies",
                mensaje  = "El boton <b>+</b> abre el laboratorio de especies.\n\n" +
                           "Ahi puedes disenar nuevas bacterias: elige su nombre, color y estadisticas geneticas, y luego sintetizalas directamente en la placa.",
                botonVisible = true
            },

            // ── 10b: Crear una bacteria ────────────────────────────────────
            new Paso {
                titulo   = "Crea tu primera Bacteria",
                mensaje  = "Ahora prueba a crear una especie:\n\n" +
                           "1. Pulsa el boton <b>+</b> para abrir el laboratorio\n" +
                           "2. Dale un nombre y ajusta las estadisticas\n" +
                           "3. Pulsa <b>Sintetizar</b> — aparecera en la bandeja\n" +
                           "4. Seleccionala en la bandeja y haz <b>click en la placa</b> para colocarla\n\n" +
                           "<color=#FFD700>Coloca una bacteria para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.ColocarBacteria,
                interactivo = true,
                onEntrar = () => {
                    // Activar modo Creador para que el jugador pueda colocar
                    ControladorInteraccion inter = Object.FindFirstObjectByType<ControladorInteraccion>();
                    if (inter != null && inter.modoActual != ControladorInteraccion.ModoRaton.Creador)
                        inter.ActivarModoCreador();
                    if (_instance.uiBandejaEspecies) _instance.uiBandejaEspecies.SetActive(true);
                }
            },

            // ── 11: Herramienta Estrella ───────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Comida y Spawners",
                mensaje  = "El boton de la <b>estrella</b> activa las herramientas de entorno.\n\n" +
                           "Con el pincel de comida puedes generar nutrientes directamente en la placa.\n\n" +
                           "Con el pincel de spawner colocas zonas que generan comida de forma automatica y continua.",
                botonVisible = true
            },

            // ── 11b: Colocar comida ────────────────────────────────────────
            new Paso {
                titulo   = "Dar de Comer a las Bacterias",
                mensaje  = "Las bacterias necesitan energia para sobrevivir y reproducirse.\n\n" +
                           "Con el pincel de <b>comida</b> puedes generar nutrientes directamente:\n\n" +
                           "1. Asegurate de tener el modo <b>Estrella</b> activo\n" +
                           "2. Selecciona la carta <b>Anadir Comida</b> en la bandeja\n" +
                           "3. Haz <b>click en la placa</b> para depositar nutrientes\n\n" +
                           "<color=#FFD700>Coloca comida en la placa para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.ColocarComida,
                interactivo = true,
                onEntrar = () => {
                    ControladorInteraccion inter = Object.FindFirstObjectByType<ControladorInteraccion>();
                    if (inter != null) inter.ActivarModoHerramientas();
                }
            },

            // ── 11c: Colocar spawner ───────────────────────────────────────
            new Paso {
                titulo   = "Spawner de Comida Automatico",
                mensaje  = "El <b>spawner</b> genera comida de forma automatica y continua en su area.\n\n" +
                           "Es ideal para mantener un suministro constante sin intervencion manual.\n\n" +
                           "1. Selecciona la carta <b>Plantar Spawner</b> en la bandeja\n" +
                           "2. Haz <b>click en la placa</b> para colocarlo\n\n" +
                           "Puedes modificar su radio, energia minima/maxima e intervalo pulsando el lapiz.\n\n" +
                           "<color=#FFD700>Coloca un spawner para continuar.</color>",
                botonVisible = false,
                accion   = AccionRequerida.ColocarSpawner,
                interactivo = true
            },

            // ── 12: Herramienta Casa ───────────────────────────────────────
            new Paso {
                titulo   = "Herramienta: Centrar Camara",
                mensaje  = "El boton de la <b>casa</b> centra la camara en el origen de la placa de Petri de forma instantanea.\n\n" +
                           "Util cuando te hayas alejado demasiado y quieras volver rapidamente al centro.",
                botonVisible = true
            },

            // ── 13: Graficas de evolucion ──────────────────────────────────
            new Paso {
                titulo   = "Graficas de Evolucion",
                mensaje  = "A la derecha tienes las <b>graficas de evolucion</b> que muestran en tiempo real como cambian las estadisticas medias de la poblacion.\n\n" +
                           "Puedes ver tendencias de velocidad, vision, tamano, consumo y esperanza de vida.",
                botonVisible = true,
                onEntrar = () => {
                    if (_instance.uiHudEvolucion) _instance.uiHudEvolucion.SetActive(true);
                }
            },

            // ── 12b: Graficas movibles ─────────────────────────────────────
            new Paso {
                titulo   = "Graficas de Evolucion Movibles",
                mensaje  = "Los paneles de las <b>graficas de evolucion</b> son completamente movibles.\n\n" +
                           "Puedes arrastrarlos por toda la pantalla y colocarlos donde mas te convenga para tener siempre los datos a la vista mientras observas la simulacion.",
                botonVisible = true
            },



            // ── 14: Guardado ───────────────────────────────────────────────
            new Paso {
                titulo   = "Guardar y Cargar",
                mensaje  = "Puedes guardar tu simulacion en cualquier momento con <b>Escape</b>.\n\n" +
                           "Desde el menu de pausa podras guardar con nombre, cargar una partida anterior o volver al menu principal.",
                botonVisible = true
            },

            // ── 15: Fin ────────────────────────────────────────────────────
            new Paso {
                titulo   = "Ya estas listo!",
                mensaje  = "Ahora conoces todo lo necesario para empezar tu propia simulacion.\n\n" +
                           "Al pulsar <b>Empezar</b> se abrira una simulacion nueva y vacia lista para que experimentes.\n\n" +
                           "Observa, experimenta y descubre que bacterias acaban dominando la placa.",
                botonVisible = true
            }
        };

        SiguientePaso();
    }

    private void OnDestroy()
    {
        CamaraControladorPro.OnIntervencionManual -= OnCamaraMovida;
        ControladorInteraccion.OnCamaraCentrada   -= OnDetectarCamaraCentrada;
        ControladorInteraccion.OnBacteriaColocada -= () => _bacteriaColocada = true;
        ControladorInteraccion.OnComidaColocada   -= () => _comidaColocada   = true;
        ControladorInteraccion.OnSpawnerColocado  -= () => _spawnerColocado  = true;
    }

    // ── Logica de pasos ─────────────────────────────────────────────────────

    public void SiguientePaso()
    {
        _pasoActual++;

        if (_pasoActual >= _pasos.Count)
        {
            FinalizarTutorial();
            return;
        }

        Paso paso = _pasos[_pasoActual];
        paso.onEntrar?.Invoke();

        textoTitulo.text  = paso.titulo;
        textoMensaje.text = paso.mensaje;

        bool esUltimo = (_pasoActual == _pasos.Count - 1);
        botonSiguiente.gameObject.SetActive(paso.botonVisible);
        if (textoBotonSiguiente != null)
            textoBotonSiguiente.text = esUltimo ? "Empezar!" : "Siguiente ->";

        _accionActual    = paso.accion;
        _esperandoAccion = paso.accion != AccionRequerida.Ninguna;
        _timerLectura    = _esperandoAccion ? TIEMPO_MINIMO_LECTURA : 0f;

        // Pasos interactivos: el fondo oscuro no bloquea clicks para que el jugador pueda usar la UI
        if (overlayFondo != null)
            overlayFondo.raycastTarget = !paso.interactivo;
    }

    // ── Deteccion de acciones ───────────────────────────────────────────────

    private void Update()
    {
        if (!_activo || !_esperandoAccion) return;

        if (_timerLectura > 0f)
        {
            _timerLectura -= Time.unscaledDeltaTime;
            return;
        }

        switch (_accionActual)
        {
            case AccionRequerida.CambiarTiempo:
                if (Input.GetKeyDown(KeyCode.T) || Time.timeScale > 0f)
                {
                    BloquearCambiarTiempo = true;
                    _esperandoAccion = false;
                    StartCoroutine(SecuenciaBacteriaMoviendose());
                }
                break;

            case AccionRequerida.HacerZoom:
                if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)
                    OnAccionCompletada();
                break;

            case AccionRequerida.SeleccionarBacteria:
                if (uiPanelInspector != null && uiPanelInspector.activeSelf)
                {
                    _esperandoAccion = false;
                    StartCoroutine(SecuenciaInspeccion());
                }
                break;

            case AccionRequerida.AbrirLaboratorio:
                if (PanellCreacion.LaboratorioAbierto)
                    OnAccionCompletada();
                break;

            case AccionRequerida.ActivarHerramienta:
                if (_controladorInteraccion != null &&
                    _controladorInteraccion.modoActual == ControladorInteraccion.ModoRaton.Herramientas)
                    OnAccionCompletada();
                break;

            case AccionRequerida.CentrarCamara:
                if (_camaraCentrada) { _camaraCentrada = false; OnAccionCompletada(); }
                break;

            case AccionRequerida.ColocarBacteria:
                if (_bacteriaColocada) { _bacteriaColocada = false; OnAccionCompletada(); }
                break;

            case AccionRequerida.ColocarComida:
                if (_comidaColocada) { _comidaColocada = false; OnAccionCompletada(); }
                break;

            case AccionRequerida.ColocarSpawner:
                if (_spawnerColocado) { _spawnerColocado = false; OnAccionCompletada(); }
                break;
        }
    }

    private void OnCamaraMovida()
    {
        if (!_activo || !_esperandoAccion || _accionActual != AccionRequerida.MoverCamara) return;
        if (_timerLectura > 0f) return;
        OnAccionCompletada();
    }

    private void OnDetectarCamaraCentrada() => _camaraCentrada = true;

    private void OnAccionCompletada()
    {
        _esperandoAccion = false;
        StartCoroutine(AvanzarConDelay(0.7f));
    }

    private IEnumerator AvanzarConDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SiguientePaso();
    }

    // ── Secuencias especiales ───────────────────────────────────────────────

    /// <summary>Fuerza una velocidad usando ControladorTiempo para que fixedDeltaTime se actualice correctamente.</summary>
    private void SetVelocidad(float velocidad)
    {
        if (_controladorTiempo != null)
            _controladorTiempo.CambiarVelocidad(velocidad);
        else
            Time.timeScale = velocidad; // fallback
    }

    private IEnumerator SecuenciaBacteriaMoviendose()
    {
        // Normalizar siempre a 1x (aunque el jugador haya puesto 5x) y quitar overlay
        SetVelocidad(1f);
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;

        yield return new WaitForSecondsRealtime(4f);

        SetVelocidad(0f);
        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;

        SiguientePaso();
    }

    private IEnumerator SecuenciaInspeccion()
    {
        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        SetVelocidad(1f);

        yield return new WaitForSecondsRealtime(5f);

        SetVelocidad(0f);
        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;

        SiguientePaso();
    }

    // ── Fin del tutorial ────────────────────────────────────────────────────

    private void FinalizarTutorial()
    {
        _activo = false;
        BloquearCambiarTiempo = false;

        CamaraControladorPro.OnIntervencionManual -= OnCamaraMovida;
        ControladorInteraccion.OnCamaraCentrada   -= OnDetectarCamaraCentrada;

        // Resetear el tiempo antes de cargar la escena
        Time.timeScale = 1f;
        GestorEscenas.LimpiarPartidaACargar();

        // Abrir nueva simulacion limpia
        SceneManager.LoadScene(GestorEscenas.ESCENA_GAMEPLAY);
    }
}
