using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Unit Inspection Panel.
/// Displays static gene data, a live energy bar, a live lifespan counter,
/// a Picture-in-Picture camera and camera control buttons.
/// </summary>
public class PanelInspectorBacteria : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------
    private const float ZOffsetCamara = -10f;
    private const float VelocidadSeguimiento = 8f;

    // -------------------------------------------------------------------------
    // Inspector — Text labels
    // -------------------------------------------------------------------------
    [Header("Textos de estadísticas")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoLinaje;
    [SerializeField] private TextMeshProUGUI textoGeneracion;
    [SerializeField] private TextMeshProUGUI textoVelocidad;
    [SerializeField] private TextMeshProUGUI textoVision;
    [SerializeField] private TextMeshProUGUI textoTamano;
    [SerializeField] private TextMeshProUGUI textoConsumo;
    [SerializeField] private TextMeshProUGUI textoVidaUtil; // Ahora lo usaremos para el tiempo en vivo
    [SerializeField] private TextMeshProUGUI textoEnergia;

    // -------------------------------------------------------------------------
    // Inspector — Energy bar
    // -------------------------------------------------------------------------
    [Header("Barra de energía")]
    [Tooltip("Image hija de FondoBarra. Se configurará como Filled en Awake.")]
    [SerializeField] private Image barraEnergia;
    [SerializeField] private Gradient gradienteEnergia;

    // -------------------------------------------------------------------------
    // Inspector — Picture-in-Picture
    // -------------------------------------------------------------------------
    [Header("Picture-in-Picture")]
    [SerializeField] private RawImage visorPiP;
    [SerializeField] private Camera camaraPiP;

    // -------------------------------------------------------------------------
    // Inspector — Scene references
    // -------------------------------------------------------------------------
    [Header("Escena")]
    [SerializeField] private Camera camaraPrincipal;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private SistemaVida _bacteriaActual;
    private bool _siguiendo;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (barraEnergia != null)
        {
            barraEnergia.type = Image.Type.Filled;
            barraEnergia.fillMethod = Image.FillMethod.Horizontal;
            barraEnergia.fillAmount = 1f;
        }

        gameObject.SetActive(false);
    }
    private void OnEnable() 
    { 
        CamaraControladorPro.OnIntervencionManual += CancelarSeguimiento;
    }

    private void OnDisable()
    {
        CamaraControladorPro.OnIntervencionManual -= CancelarSeguimiento;
    }

    private void Update()
    {
        // 1. Si la bacteria ha sido destruida por completo (Destroy)
        if (_bacteriaActual == null)
        {
            Cerrar();
            return;
        }

        // 2. Si la bacteria sigue existiendo pero se ha apagado (Object Pooling / Muerte visual)
        if (!_bacteriaActual.gameObject.activeInHierarchy)
        {
            Cerrar();
            return;
        }

        // --- Si llegamos aquí, la bacteria está viva y coleando ---
        ActualizarBarraEnergia();
        ActualizarVidaUtil();
        ActualizarCamaraPiP();

        if (_siguiendo)
            SeguirConCamaraPrincipal();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------
    public void Abrir(SistemaVida bacteria)
    {
        _bacteriaActual = bacteria;
        _siguiendo = false;


        if (camaraPiP != null)
        {
            camaraPiP.gameObject.SetActive(true);

            // La movemos instantáneamente a la nueva bacteria para que no 
            // se vea un "salto" desde la posición de la bacteria muerta
            Vector3 target = _bacteriaActual.transform.position;
            camaraPiP.transform.position = new Vector3(target.x, target.y, ZOffsetCamara);
        }
        // -----------------------------------------------------------

        MostrarDatosEstaticos();
        gameObject.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Button callbacks
    // -------------------------------------------------------------------------
    public void OnBotonIrA()
    {
        if (_bacteriaActual == null || camaraPrincipal == null) return;

        _siguiendo = false;
        Vector3 pos = _bacteriaActual.transform.position;
        camaraPrincipal.transform.position = new Vector3(pos.x, pos.y, ZOffsetCamara);
    }

    public void OnBotonSeguir()
    {
        if (_bacteriaActual == null) return;
        _siguiendo = !_siguiendo;
    }

    public void Cerrar()
    {
        _bacteriaActual = null;
        _siguiendo = false;

        if (camaraPiP != null)
            camaraPiP.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Internal — static data display
    // -------------------------------------------------------------------------
    private void MostrarDatosEstaticos()
    {
        if (_bacteriaActual == null) return;

        DatosGeneticos g = _bacteriaActual.misStats;

        if (textoNombre != null) textoNombre.text = _bacteriaActual.gameObject.name;
        if (textoLinaje != null) textoLinaje.text = $"Linaje: {g.idLinaje}";
        if (textoGeneracion != null) textoGeneracion.text = $"Generación: {g.generaciones}";
        if (textoVelocidad != null) textoVelocidad.text = $"Velocidad: {g.velocidad:F2}";
        if (textoVision != null) textoVision.text = $"Visión: {g.radioVision:F2}";
        if (textoTamano != null) textoTamano.text = $"Tamaño: {g.tamano:F2}";
        if (textoConsumo != null) textoConsumo.text = $"Consumo: {g.consumo:F3}/s";

        // ELIMINADO: textoVidaUtil ya no se actualiza aquí porque ahora es dinámico.
    }

    // -------------------------------------------------------------------------
    // Internal — per-frame updates
    // -------------------------------------------------------------------------
    private void ActualizarBarraEnergia()
    {
        float ratio = Mathf.Clamp01(_bacteriaActual.EnergiaActual / _bacteriaActual.misStats.energiaMax);

        if (barraEnergia != null)
        {
            barraEnergia.fillAmount = ratio;
            barraEnergia.color = gradienteEnergia.Evaluate(ratio);
        }

        if (textoEnergia != null)
            textoEnergia.text = $"{_bacteriaActual.EnergiaActual:F0} / {_bacteriaActual.misStats.energiaMax:F0}";
    }

    private void ActualizarVidaUtil()
    {
        if (textoVidaUtil != null)
        {

            float tiempoVivido = _bacteriaActual.EdadActual;
            float vidaMaxima = _bacteriaActual.misStats.vidaUtil;

            // Mostrará algo como: "Edad: 12.5s / 40.0s"
            textoVidaUtil.text = $"Edad: {tiempoVivido:F1}s / {vidaMaxima:F1}s";
        }
    }

    private void ActualizarCamaraPiP()
    {
        if (camaraPiP == null) return;

        Vector3 target = _bacteriaActual.transform.position;
        camaraPiP.transform.position = new Vector3(target.x, target.y, ZOffsetCamara);
    }

    private void SeguirConCamaraPrincipal()
    {
        if (camaraPrincipal == null) return;

        Vector3 target = _bacteriaActual.transform.position;
        Vector3 destino = new Vector3(target.x, target.y, ZOffsetCamara);
        camaraPrincipal.transform.position = Vector3.Lerp(
            camaraPrincipal.transform.position,
            destino,
            VelocidadSeguimiento * Time.deltaTime);
    }

    private void CancelarSeguimiento()
    {
        _siguiendo = false;
    }
}