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
    public static bool BloquearCambiarTiempo { get; private set; } = false;

    private static TutorialManager _instance;

    [Header("Configuracion")]
    public bool siempreActivo = false;
    [Tooltip("Solo para pruebas: indice del paso desde el que empieza el tutorial (0 = normal)")]
    [SerializeField] private int _pasoInicial = 0;

    [Header("Panel Tutorial UI")]
    public CanvasGroup overlay;
    public Image overlayFondo;
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
    private bool _camaraCentrada      = false;
    private bool _bacteriaColocada    = false;
    private bool _comidaColocada      = false;
    private bool _spawnerColocado     = false;
    private bool _especieSintetizada  = false;
    private int  _bacteriasColocadasCount = 0;
    private const int BACTERIAS_REQUERIDAS = 3;
    private Canvas           _canvasHighlightTemp    = null;
    private GraphicRaycaster _raycasterHighlightTemp = null;

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
        ActivarModoCreador,
        EspecieSintetizada,
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
        public bool interactivo = false;
        public System.Action onEntrar;
        public System.Func<bool> validacion = null;
        /// <summary>Si no es null, se oculta overlayFondo para que el elemento sea visible.</summary>
        public System.Func<RectTransform> obtenerTarget = null;
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
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f;

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
        ControladorInteraccion.OnBacteriaColocada      += OnBacteriaColocadaTutorial;
        ControladorInteraccion.OnComidaColocada        += () => _comidaColocada     = true;
        ControladorInteraccion.OnSpawnerColocado       += () => _spawnerColocado    = true;
        PanellCreacion.OnEspecieSintetizada            += () => _especieSintetizada = true;

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
                mensaje  = "El boton <b>+</b> de la barra lateral abre el laboratorio de especies.\n\n" +
                           "Ahi podras disenar nuevas bacterias: nombre, color y estadisticas geneticas.\n\n" +
                           "Vamos a probarlo.",
                botonVisible = true
            },

            // ── 10a: Pulsa el boton + ──────────────────────────────────────
            new Paso {
                titulo   = "Abre el Modo Creador",
                mensaje  = "Pulsa el boton <b>+</b> de la barra lateral.\n\n" +
                           "Aparecera la bandeja con la opcion de crear una nueva especie.\n\n" +
                           "<color=#FFD700>Pulsa el boton +.</color>",
                botonVisible = false,
                accion      = AccionRequerida.ActivarModoCreador,
                interactivo = true,
                onEntrar    = () => {
                    if (_instance.uiBandejaEspecies) _instance.uiBandejaEspecies.SetActive(true);
                }
            },

            // ── 10b: Haz click en la carta ────────────────────────────────
            new Paso {
                titulo        = "Crear una Nueva Especie",
                mensaje       = "Ahora haz click en la carta <b>+ CREAR NUEVA</b> de la bandeja para abrir el laboratorio.\n\n" +
                                "<color=#FFD700>Haz click en la carta.</color>",
                botonVisible  = false,
                accion        = AccionRequerida.AbrirLaboratorio,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<CartaEspecieUI>()?.GetComponent<RectTransform>()
            },

            // ── 10c-10m: Lab (obtenerTarget oculta el overlay para ver el panel) ──
            // ── 10c: Nombre ───────────────────────────────────────────────
            new Paso {
                titulo      = "El Nombre de la Especie",
                mensaje     = "Escribe un <b>nombre</b> para tu especie en el campo de texto.\n\n" +
                              "Este nombre identifica el linaje y aparecera en la bandeja y en las graficas.\n\n" +
                              "<color=#FFD700>Escribe algo para continuar.</color>",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtInputNombre,
                validacion    = () => !string.IsNullOrWhiteSpace(Object.FindFirstObjectByType<PanellCreacion>()?.TextoNombre)
            },

            // ── 10d: Color ────────────────────────────────────────────────
            new Paso {
                titulo        = "El Color del Linaje",
                mensaje       = "Haz click en el selector de colores para elegir el <b>color del linaje</b>.\n\n" +
                                "Todas las bacterias de esta especie y sus descendientes tendran este color en la placa.",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtSelectorColor
            },

            // ── 10e: Velocidad ────────────────────────────────────────────
            new Paso {
                titulo        = "Gen: Velocidad",
                mensaje       = "El slider de <b>Velocidad</b> controla que tan rapido se mueve la bacteria.\n\n" +
                                "Mayor velocidad = mas alcance para buscar comida, pero tambien <b>mayor consumo de energia</b>.",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtSliderVelocidad
            },

            // ── 10f: Vision ───────────────────────────────────────────────
            new Paso {
                titulo        = "Gen: Vision",
                mensaje       = "El slider de <b>Vision</b> define el radio de deteccion de comida y enemigos.\n\n" +
                                "Mayor vision = detecta objetivos lejanos, pero tambien <b>sube el consumo</b>.",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtSliderVision
            },

            // ── 10g: Tamano ───────────────────────────────────────────────
            new Paso {
                titulo        = "Gen: Tamano",
                mensaje       = "El slider de <b>Tamano</b> determina el volumen fisico de la bacteria.\n\n" +
                                "Una bacteria grande puede <b>devorar a otras mas pequenas</b>, pero necesita mucha mas energia para sobrevivir.",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtSliderTamano
            },

            // ── 10h: Estadisticas calculadas ──────────────────────────────
            new Paso {
                titulo        = "Estadisticas Calculadas",
                mensaje       = "Estos valores se calculan <b>automaticamente</b> segun tus genes:\n\n" +
                                "<b>Consumo</b> = (Tamano³ + Velocidad² + Vision) / 4\n" +
                                "<b>Energia Max</b> = Tamano × 100\n" +
                                "<b>Vida Util</b> = (Tamano² × 100) / Consumo\n" +
                                "<b>Mitosis</b> = (Energia Max / Vida Util) × 5\n" +
                                "<b>Coste Reprod.</b> = Consumo × Mitosis × 2",
                botonVisible  = true,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.RtPanelStats
            },

            // ── 10m: Sintetizar ───────────────────────────────────────────
            new Paso {
                titulo        = "Sintetiza tu Especie",
                mensaje       = "Cuando estes satisfecho con los parametros, pulsa <b>Sintetizar</b>.\n\n" +
                                "La especie quedara registrada y aparecera como una carta en la bandeja.\n\n" +
                                "<color=#FFD700>Pulsa Sintetizar para continuar.</color>",
                botonVisible  = false,
                accion        = AccionRequerida.EspecieSintetizada,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<PanellCreacion>()?.BtnSintetizar?.GetComponent<RectTransform>()
            },

            // ── 10n: Colocar 3 bacterias ──────────────────────────────────
            new Paso {
                titulo        = "Coloca tus Bacterias en la Placa",
                mensaje       = "Tu especie ya aparece en la bandeja.\n\n" +
                                "Seleccionala haciendo click en su carta y luego haz <b>click en la placa</b> para colocarla.\n\n" +
                                $"<color=#FFD700>Coloca <b>3 bacterias</b> en la placa (0/{BACTERIAS_REQUERIDAS}).</color>",
                botonVisible  = false,
                accion        = AccionRequerida.ColocarBacteria,
                interactivo   = true,
                obtenerTarget = () => {
                    foreach (var c in Object.FindObjectsByType<CartaEspecieUI>(FindObjectsSortMode.None))
                        if (!c.EsCartaCrearNueva) return c.GetComponent<RectTransform>();
                    return null;
                },
                onEntrar = () => {
                    _instance._bacteriasColocadasCount = 0;
                    _instance._bacteriaColocada = false;
                    GameObject bacteriaTutorial = GameObject.Find("BacteriaTutorial");
                    if (bacteriaTutorial != null)
                    {
                        SistemaVida sv = bacteriaTutorial.GetComponent<SistemaVida>();
                        if (sv != null) sv.Purga();
                        else Object.Destroy(bacteriaTutorial);
                    }
                    _instance.overlay.blocksRaycasts = false;
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
                titulo        = "Dar de Comer a las Bacterias",
                mensaje       = "Las bacterias necesitan energia para sobrevivir y reproducirse.\n\n" +
                                "Selecciona la carta <b>Anadir Comida</b> y haz <b>click en la placa</b> para depositar nutrientes.\n\n" +
                                "Con el boton <b>Modificar</b> de la carta puedes cambiar cuanta energia aporta cada deposito.\n\n" +
                                "<color=#FFD700>Coloca comida en la placa para continuar.</color>",
                botonVisible  = false,
                accion        = AccionRequerida.ColocarComida,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<CartaComidaUI>()?.GetComponent<RectTransform>(),
                onEntrar      = () => {
                    ControladorInteraccion inter = Object.FindFirstObjectByType<ControladorInteraccion>();
                    if (inter != null) inter.ActivarModoHerramientas();
                    _instance.overlay.blocksRaycasts = false;
                }
            },

            // ── 11c: Colocar spawner ───────────────────────────────────────
            new Paso {
                titulo        = "Spawner de Comida Automatico",
                mensaje       = "El <b>spawner</b> genera comida de forma automatica y continua en su area.\n\n" +
                                "Selecciona la carta <b>Plantar Spawner</b> y haz <b>click en la placa</b> para colocarlo.\n\n" +
                                "Con el boton <b>Modificar</b> puedes ajustar el radio, la energia minima/maxima y el intervalo de generacion.\n\n" +
                                "<color=#FFD700>Coloca un spawner para continuar.</color>",
                botonVisible  = false,
                accion        = AccionRequerida.ColocarSpawner,
                interactivo   = true,
                obtenerTarget = () => Object.FindFirstObjectByType<CartaSpawnerUI>()?.GetComponent<RectTransform>(),
                onEntrar      = () => {
                    _instance.overlay.blocksRaycasts = false;
                }
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

            // ── 13b: Graficas movibles ─────────────────────────────────────
            new Paso {
                titulo   = "Graficas de Evolucion Movibles",
                mensaje  = "Los paneles de las <b>graficas de evolucion</b> son completamente movibles.\n\n" +
                           "Puedes arrastrarlos por toda la pantalla y colocarlos donde mas te convenga.",
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

        int inicio = Mathf.Clamp(_pasoInicial, 0, _pasos.Count);
        for (int i = 0; i < inicio; i++)
            _pasos[i].onEntrar?.Invoke();
        _pasoActual = inicio - 1;
        SiguientePaso();
    }

    private void OnDestroy()
    {
        CamaraControladorPro.OnIntervencionManual -= OnCamaraMovida;
        ControladorInteraccion.OnCamaraCentrada   -= OnDetectarCamaraCentrada;
        ControladorInteraccion.OnBacteriaColocada -= OnBacteriaColocadaTutorial;
        ControladorInteraccion.OnComidaColocada   -= () => _comidaColocada     = true;
        ControladorInteraccion.OnSpawnerColocado  -= () => _spawnerColocado    = true;
        PanellCreacion.OnEspecieSintetizada       -= () => _especieSintetizada = true;
    }

    // ── Logica de pasos ─────────────────────────────────────────────────────

    public void SiguientePaso()
    {
        if (_pasos != null && _pasoActual >= 0 && _pasoActual < _pasos.Count)
        {
            Paso pasoActual = _pasos[_pasoActual];
            if (pasoActual.validacion != null && !pasoActual.validacion()) return;
        }

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

        if (overlayFondo != null)
            overlayFondo.raycastTarget = !paso.interactivo;

        ActualizarHighlight(paso);
    }

    private void ActualizarHighlight(Paso paso)
    {
        LimpiarHighlight();
        if (paso.obtenerTarget == null) return;
        RectTransform target = paso.obtenerTarget();
        if (target == null) return;

        if (!target.TryGetComponent(out Canvas _))
        {
            _canvasHighlightTemp = target.gameObject.AddComponent<Canvas>();
            _canvasHighlightTemp.overrideSorting = true;
            _canvasHighlightTemp.sortingOrder = 999;
            _raycasterHighlightTemp = target.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void LimpiarHighlight()
    {
        if (_raycasterHighlightTemp != null) { Destroy(_raycasterHighlightTemp); _raycasterHighlightTemp = null; }
        if (_canvasHighlightTemp    != null) { Destroy(_canvasHighlightTemp);    _canvasHighlightTemp    = null; }
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

            case AccionRequerida.ActivarModoCreador:
                if (_controladorInteraccion != null &&
                    _controladorInteraccion.modoActual == ControladorInteraccion.ModoRaton.Creador)
                    OnAccionCompletada();
                break;

            case AccionRequerida.EspecieSintetizada:
                if (_especieSintetizada) { _especieSintetizada = false; OnAccionCompletada(); }
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
                if (_bacteriaColocada)
                {
                    _bacteriaColocada = false;
                    overlay.blocksRaycasts = true;
                    OnAccionCompletada();
                }
                break;

            case AccionRequerida.ColocarComida:
                if (_comidaColocada)
                {
                    _comidaColocada = false;
                    overlay.blocksRaycasts = true;
                    OnAccionCompletada();
                }
                break;

            case AccionRequerida.ColocarSpawner:
                if (_spawnerColocado)
                {
                    _spawnerColocado = false;
                    _esperandoAccion = false;
                    StartCoroutine(SecuenciaPostSpawner());
                }
                break;
        }
    }

    private void OnBacteriaColocadaTutorial()
    {
        if (!_esperandoAccion || _accionActual != AccionRequerida.ColocarBacteria) return;

        _bacteriasColocadasCount++;
        textoMensaje.text =
            "Tu especie ya aparece en la bandeja.\n\n" +
            "Seleccionala haciendo click en su carta y luego haz <b>click en la placa</b> para colocarla.\n\n" +
            $"<color=#FFD700>Coloca <b>3 bacterias</b> en la placa ({_bacteriasColocadasCount}/{BACTERIAS_REQUERIDAS}).</color>";

        if (_bacteriasColocadasCount >= BACTERIAS_REQUERIDAS)
            _bacteriaColocada = true;
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

    private void SetVelocidad(float velocidad)
    {
        if (_controladorTiempo != null)
            _controladorTiempo.CambiarVelocidad(velocidad);
        else
            Time.timeScale = velocidad;
    }

    private IEnumerator SecuenciaBacteriaMoviendose()
    {
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

    private IEnumerator SecuenciaPostSpawner()
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
        LimpiarHighlight();

        CamaraControladorPro.OnIntervencionManual -= OnCamaraMovida;
        ControladorInteraccion.OnCamaraCentrada   -= OnDetectarCamaraCentrada;

        Time.timeScale = 1f;
        GestorEscenas.LimpiarPartidaACargar();
        SceneManager.LoadScene(GestorEscenas.ESCENA_GAMEPLAY);
    }
}
