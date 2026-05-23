using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanellCreacion : MonoBehaviour
{
    public static bool LaboratorioAbierto = false;

    [Header("Identificación")]
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private Image previewBacteria;

    [SerializeField] private TextMeshProUGUI txtAlertaNombre;

    [Header("Sliders (Límites visuales)")]
    [SerializeField] private Slider sliderTamano;
    [SerializeField] private Slider sliderVelocidad;
    [SerializeField] private Slider sliderVision;

    [Header("Inputs Manuales (Sin límites)")]
    [SerializeField] private TMP_InputField inputTamano;
    [SerializeField] private TMP_InputField inputVelocidad;
    [SerializeField] private TMP_InputField inputVision;

    [Header("Panel de Consecuencias")]
    [SerializeField] private TextMeshProUGUI txtConsumo;
    [SerializeField] private TextMeshProUGUI txtEnergia;
    [SerializeField] private TextMeshProUGUI txtVida;
    [SerializeField] private TextMeshProUGUI txtMitosis;
    [SerializeField] private TextMeshProUGUI txtCoste;

    [Header("Botones de Acción")]
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

        // Forzamos la primera actualización
        SincronizarUI();
    }

    // --- LÓGICA DE LOS SLIDERS ---
    private void AlMoverSliderTamano(float valor) { _tamanoActual = valor; SincronizarUI(); }
    private void AlMoverSliderVelocidad(float valor) { _velocidadActual = valor; SincronizarUI(); }
    private void AlMoverSliderVision(float valor) { _visionActual = valor; SincronizarUI(); }

    // --- LÓGICA DE LOS INPUTS MANUALES (CON VALIDACIÓN) ---
    private void AlEscribirInputTamano(string texto) { _tamanoActual = ValidarNumero(texto, _tamanoActual); SincronizarUI(); }
    private void AlEscribirInputVelocidad(string texto) { _velocidadActual = ValidarNumero(texto, _velocidadActual); SincronizarUI(); }
    private void AlEscribirInputVision(string texto) { _visionActual = ValidarNumero(texto, _visionActual); SincronizarUI(); }

    // Comprueba que lo escrito sea un número y que sea mayor estricto que 0
    private float ValidarNumero(string texto, float valorAnterior)
    {
        if (float.TryParse(texto, out float resultado))
        {
            if (resultado <= 0f) return 0.01f; // Mínimo absoluto biológico para no dividir por cero
            return resultado;
        }
        return valorAnterior; // Si escribe letras, restauramos el número anterior
    }

    // --- SINCRONIZACIÓN Y CÁLCULO ---
    private void SincronizarUI()
    {
        // 1. Actualizamos los Inputs silenciosamente (SetTextWithoutNotify evita bucles)
        if (inputTamano != null) inputTamano.SetTextWithoutNotify(_tamanoActual.ToString("F2"));
        if (inputVelocidad != null) inputVelocidad.SetTextWithoutNotify(_velocidadActual.ToString("F2"));
        if (inputVision != null) inputVision.SetTextWithoutNotify(_visionActual.ToString("F2"));

        // 2. Actualizamos los Sliders silenciosamente (Clamp para que no se rompan si el valor manual es altísimo)
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

    // --- MÉTODOS PÚBLICOS ---
    public void SeleccionarColorDinamico(Color nuevoColor)
    {
        _colorSeleccionado = nuevoColor;
        if (previewBacteria != null) previewBacteria.color = _colorSeleccionado;
        RecalcularEstadisticas();
    }

    public void AbrirPanel()
    {
        gameObject.SetActive(true);
        LaboratorioAbierto = true;
        if (txtAlertaNombre != null) txtAlertaNombre.text = "";
        SincronizarUI();
    }

    public void CerrarPanel()
    {
        LaboratorioAbierto = false;
        gameObject.SetActive(false);
    }

    private void SintetizarNuevaEspecie()
    {
        if (inputNombre == null || string.IsNullOrWhiteSpace(inputNombre.text))
        {
            if (txtAlertaNombre != null)
            {
                txtAlertaNombre.color = Color.red;
                txtAlertaNombre.text = "¡Error: Debes bautizar la especie!";
            }
            return;
        }

        string nombreEspecie = inputNombre.text;

        //ENVIAMOS LOS DATOS AL GESTOR Y OBTENEMOS EL ID
        if (GestorLinajes.Instance != null)
        {
            int nuevoID = GestorLinajes.Instance.RegistrarLinajeManual(nombreEspecie, _statsEnProgreso);
            Debug.Log($"[Laboratorio] Guardada especie: {nombreEspecie} con ID #{nuevoID}");

            // AVISAMOS A LA BANDEJA QUE SE ACTUALICE
            ControladorInteraccion controlador = FindFirstObjectByType<ControladorInteraccion>();
            if (controlador != null && controlador.panelSelectorEspecies != null)
            {
                BandejaEspeciesUI bandeja = controlador.panelSelectorEspecies.GetComponent<BandejaEspeciesUI>();
                if (bandeja != null) bandeja.RedibujarBandeja();

                // Automáticamente seleccionamos este pincel para que ya lo tengas en el ratón
                controlador.SeleccionarPincelEspecie(nuevoID);
            }
        }

        CerrarPanel();
    }
}