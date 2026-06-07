using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
- Panel que permite al jugador crear una nueva especie sintética personalizada.
- El jugador puede ajustar el tamaño, velocidad, visión y color de la bacteria usando sliders e inputs manuales.
- El panel muestra en tiempo real las consecuencias de las elecciones del jugador, como el consumo energético, vida útil, tiempo de mitosis y coste de reproducción.
- El jugador debe asignar un nombre a la nueva especie antes de sintetizarla.
- Al sintetizar, el panel envía los datos al GestorLinajes para crear la nueva especie y luego se cierra automáticamente.
- El panel también tiene una posición especial para el tutorial, donde se resalta cada elemento a medida que se explica su función.
*/

public class PanellCreacion : MonoBehaviour
{
    public static bool LaboratorioAbierto = false;
    public static event System.Action OnEspecieSintetizada;
    public Button BtnSintetizar => btnSintetizar;

    [Header("Tutorial")]
    [SerializeField] private Vector2 posicionTutorial = new Vector2(750f, 0f);
    [SerializeField] private RectTransform selectorColor;
    [SerializeField] private RectTransform panelStats;
    [SerializeField] private RectTransform filaNombre;
    [SerializeField] private RectTransform filaVelocidad;
    [SerializeField] private RectTransform filaVision;
    [SerializeField] private RectTransform filaTamano;
    private Vector2 _posicionOriginal;
    private bool    _posicionCapturada = false;

    public RectTransform RtPanel          => GetComponent<RectTransform>();
    public RectTransform RtInputNombre    => filaNombre    != null ? filaNombre    : inputNombre?.GetComponent<RectTransform>();
    public RectTransform RtSelectorColor  => selectorColor;
    public RectTransform RtSliderVelocidad => filaVelocidad != null ? filaVelocidad : sliderVelocidad?.GetComponent<RectTransform>();
    public RectTransform RtSliderVision   => filaVision    != null ? filaVision    : sliderVision?.GetComponent<RectTransform>();
    public RectTransform RtSliderTamano   => filaTamano    != null ? filaTamano    : sliderTamano?.GetComponent<RectTransform>();
    public RectTransform RtPanelStats     => panelStats != null ? panelStats : txtConsumo?.transform.parent?.GetComponent<RectTransform>();
    public string TextoNombre             => inputNombre?.text ?? "";

    [Header("Identificaci�n")]
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Image previewBacteria;

    [SerializeField] private TextMeshProUGUI txtAlertaNombre;

    [Header("Sliders (L�mites visuales)")]
    [SerializeField] private Slider sliderTamano;
    [SerializeField] private Slider sliderVelocidad;
    [SerializeField] private Slider sliderVision;

    [Header("Inputs Manuales (Sin l�mites)")]
    [SerializeField] private TMP_InputField inputTamano;
    [SerializeField] private TMP_InputField inputVelocidad;
    [SerializeField] private TMP_InputField inputVision;

    [Header("Panel de Consecuencias")]
    [SerializeField] private TextMeshProUGUI txtConsumo;
    [SerializeField] private TextMeshProUGUI txtEnergia;
    [SerializeField] private TextMeshProUGUI txtVida;
    [SerializeField] private TextMeshProUGUI txtMitosis;
    [SerializeField] private TextMeshProUGUI txtCoste;

    [Header("Botones de Acci�n")]
    [SerializeField] private Button btnSintetizar;
    [SerializeField] private Button btnCancelar;

    // Valores actuales reales (pueden ser mayores que los sliders)
    private float _tamanoActual = 1f;
    private float _velocidadActual = 2f;
    private float _visionActual = 2f;

    private Color _colorSeleccionado = Color.white;
    private DatosGeneticos _statsEnProgreso;
    private const float FACTOR_RITMO = 5f;

    private void Start()
    {

        // 1. Conectamos los Sliders (Cuando arrastras la barra)
        if (sliderTamano != null) sliderTamano.onValueChanged.AddListener(AlMoverSliderTamano);
        if (sliderVelocidad != null) sliderVelocidad.onValueChanged.AddListener(AlMoverSliderVelocidad);
        if (sliderVision != null) sliderVision.onValueChanged.AddListener(AlMoverSliderVision);

        // 2. Conectamos los Inputs (Cuando terminas de escribir y pulsas Enter)
        if (inputTamano != null) inputTamano.onEndEdit.AddListener(AlEscribirInputTamano);
        if (inputVelocidad != null) inputVelocidad.onEndEdit.AddListener(AlEscribirInputVelocidad);
        if (inputVision != null) inputVision.onEndEdit.AddListener(AlEscribirInputVision);

        if (btnCancelar != null) btnCancelar.onClick.AddListener(CerrarPanel);
        if (btnSintetizar != null) btnSintetizar.onClick.AddListener(SintetizarNuevaEspecie);

        if (txtAlertaNombre != null) txtAlertaNombre.text = "";

        // Forzamos la primera actualizaci�n
        SincronizarUI();
    }

    // --- L�GICA DE LOS SLIDERS ---
    private void AlMoverSliderTamano(float valor) { _tamanoActual = valor; SincronizarUI(); }
    private void AlMoverSliderVelocidad(float valor) { _velocidadActual = valor; SincronizarUI(); }
    private void AlMoverSliderVision(float valor) { _visionActual = valor; SincronizarUI(); }

    // --- L�GICA DE LOS INPUTS MANUALES (CON VALIDACI�N) ---
    private void AlEscribirInputTamano(string texto) { _tamanoActual = ValidarNumero(texto, _tamanoActual); SincronizarUI(); }
    private void AlEscribirInputVelocidad(string texto) { _velocidadActual = ValidarNumero(texto, _velocidadActual); SincronizarUI(); }
    private void AlEscribirInputVision(string texto) { _visionActual = ValidarNumero(texto, _visionActual); SincronizarUI(); }

    // Comprueba que lo escrito sea un n�mero y que sea mayor estricto que 0
    private float ValidarNumero(string texto, float valorAnterior)
    {
        if (float.TryParse(texto, out float resultado))
        {
            if (resultado <= 0f) return 0.01f; // M�nimo absoluto biol�gico para no dividir por cero
            return resultado;
        }
        return valorAnterior; // Si escribe letras, restauramos el n�mero anterior
    }

    // --- SINCRONIZACI�N Y C�LCULO ---
    private void SincronizarUI()
    {
        // 1. Actualizamos los Inputs silenciosamente (SetTextWithoutNotify evita bucles)
        if (inputTamano != null) inputTamano.SetTextWithoutNotify(_tamanoActual.ToString("F2"));
        if (inputVelocidad != null) inputVelocidad.SetTextWithoutNotify(_velocidadActual.ToString("F2"));
        if (inputVision != null) inputVision.SetTextWithoutNotify(_visionActual.ToString("F2"));

        // 2. Actualizamos los Sliders silenciosamente (Clamp para que no se rompan si el valor manual es alt�simo)
        if (sliderTamano != null) sliderTamano.SetValueWithoutNotify(Mathf.Clamp(_tamanoActual, sliderTamano.minValue, sliderTamano.maxValue));
        if (sliderVelocidad != null) sliderVelocidad.SetValueWithoutNotify(Mathf.Clamp(_velocidadActual, sliderVelocidad.minValue, sliderVelocidad.maxValue));
        if (sliderVision != null) sliderVision.SetValueWithoutNotify(Mathf.Clamp(_visionActual, sliderVision.minValue, sliderVision.maxValue));

        RecalcularEstadisticas();
    }

    private void RecalcularEstadisticas()
    {
        // Usamos nuestras variables reales (que pueden ser gigantescas)
        float t = _tamanoActual;
        float v = _velocidadActual;
        float r = _visionActual;

        float consumoCalc = DatosGeneticos.CalcularGasto(t, v, r);
        float energiaMaxCalc = t * 100f;
        float vidaUtilCalc = (t * t * 100f) / consumoCalc;
        float mitosisCalc = (energiaMaxCalc / vidaUtilCalc) * FACTOR_RITMO;
        float costeCalc = DatosGeneticos.CalcularCosteReproduccion(consumoCalc, mitosisCalc);

        _statsEnProgreso = new DatosGeneticos
        {
            tamano = t,
            velocidad = v,
            radioVision = r,
            consumo = consumoCalc,
            energiaMax = energiaMaxCalc,
            vidaUtil = vidaUtilCalc,
            tiempreEntreReproduccion = mitosisCalc,
            rangoMutacion = 0.10f,
            colorLinaje = _colorSeleccionado
        };

        if (txtConsumo != null) txtConsumo.text = $"Consumo: {consumoCalc:F2}/s";
        if (txtEnergia != null) txtEnergia.text = $"Energia Max: {energiaMaxCalc:F0}";
        if (txtVida != null) txtVida.text = $"Vida Util: {vidaUtilCalc:F1}s";
        if (txtMitosis != null) txtMitosis.text = $"Mitosis: {mitosisCalc:F1}s";
        if (txtCoste != null) txtCoste.text = $"Coste: {costeCalc:F0} E";
    }

    // --- M�TODOS P�BLICOS ---
    public void SeleccionarColorDinamico(Color nuevoColor)
    {
        _colorSeleccionado = nuevoColor;
        if (previewBacteria != null) previewBacteria.color = _colorSeleccionado;
        RecalcularEstadisticas();
    }

    public void AbrirPanel()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            if (!_posicionCapturada)
            {
                _posicionOriginal  = rt.anchoredPosition;
                _posicionCapturada = true;
            }
            rt.anchoredPosition = TutorialManager.TutorialActivo ? posicionTutorial : _posicionOriginal;
        }
        gameObject.SetActive(true);
        LaboratorioAbierto = true;
        if (txtAlertaNombre != null) txtAlertaNombre.text = "";
        SincronizarUI();
    }

    public void CerrarPanel()
    {
        LaboratorioAbierto = false;
        gameObject.SetActive(false);
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = _posicionOriginal;
    }

    private void SintetizarNuevaEspecie()
    {
        if (inputNombre == null || string.IsNullOrWhiteSpace(inputNombre.text))
        {
            if (txtAlertaNombre != null)
            {
                txtAlertaNombre.color = Color.red;
                txtAlertaNombre.text = "�Error: Debes bautizar la especie!";
            }
            return;
        }

        string nombreEspecie = inputNombre.text;

        //ENVIAMOS LOS DATOS AL GESTOR Y OBTENEMOS EL ID
        if (GestorLinajes.Instance != null)
        {
            int nuevoID = GestorLinajes.Instance.RegistrarLinajeManual(nombreEspecie, _statsEnProgreso);
            Debug.Log($"[Laboratorio] Guardada especie: {nombreEspecie} con ID #{nuevoID}");
            OnEspecieSintetizada?.Invoke();

            // AVISAMOS A LA BANDEJA QUE SE ACTUALICE
            ControladorInteraccion controlador = FindFirstObjectByType<ControladorInteraccion>();
            if (controlador != null && controlador.panelSelector != null)
            {
                BandejaEspeciesUI bandeja = controlador.panelSelector.GetComponent<BandejaEspeciesUI>();
                if (bandeja != null) bandeja.RedibujarBandeja();

                // Autom�ticamente seleccionamos este pincel para que ya lo tengas en el rat�n
                controlador.SeleccionarPincelEspecie(nuevoID);
            }
        }

        CerrarPanel();
    }
}