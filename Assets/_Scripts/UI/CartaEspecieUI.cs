using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
- Controla la carta de especie dentro de la bandeja de especies.
- Muestra el nombre, linaje y estadísticas de la especie representada.
- Permite seleccionar la carta para usarla como pincel o abrir el laboratorio si es la carta de crear nueva.
- Cambia su apariencia cuando es seleccionada para indicar que está activa.
- Notifica al ControladorInteraccion cuando se selecciona, para que pueda actualizar su estado y comportamiento.
*/

public class CartaEspecieUI : MonoBehaviour
{
    [Header("Textos Principales")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoLinaje;

    [Header("Textos de Estadisticas")]
    [SerializeField] private TextMeshProUGUI textoVidaUtil;
    [SerializeField] private TextMeshProUGUI textoMitosis;
    [SerializeField] private TextMeshProUGUI textoCosteMitosis;
    [SerializeField] private TextMeshProUGUI textoEnergia;
    [SerializeField] private TextMeshProUGUI textoVelocidad;
    [SerializeField] private TextMeshProUGUI textoVision;
    [SerializeField] private TextMeshProUGUI textoTamano;
    [SerializeField] private TextMeshProUGUI textoConsumo;

    [Header("Visuales")]
    [SerializeField] private Image  iconoBacteria;
    [SerializeField] private Button botonSeleccion;
    [SerializeField] private Image  fondoSeleccion;

    private int                    _idLinajeAsignado;
    public bool EsCartaCrearNueva => _idLinajeAsignado == -1;
    private ControladorInteraccion _controladorInteraccion;
    private Image                  _fondo;

    private static readonly Color COLOR_SELECCIONADO = new Color(1f, 0.85f, 0.1f, 1f);
    private static readonly Color COLOR_NORMAL       = Color.white;

    private void Awake()
    {
        if (fondoSeleccion != null)
        {
            _fondo = fondoSeleccion;
        }
        else
        {
            Transform hijo = transform.Find("Fondo");
            _fondo = hijo != null ? hijo.GetComponent<Image>() : GetComponent<Image>();
        }
    }

    private void OnEnable()  => BandejaEspeciesUI.OnCartaSeleccionada += OnSeleccionCambio;
    private void OnDisable() => BandejaEspeciesUI.OnCartaSeleccionada -= OnSeleccionCambio;

    private void OnSeleccionCambio(MonoBehaviour seleccionado)
    {
        if (_fondo) _fondo.color = (seleccionado == this) ? COLOR_SELECCIONADO : COLOR_NORMAL;
    }

    public void Inicializar(int id, string nombre, DatosGeneticos stats, ControladorInteraccion ctrl)
    {
        _idLinajeAsignado       = id;
        _controladorInteraccion = ctrl;

        if (id == -1)
        {
            if (textoNombre    != null) textoNombre.text = "+ CREAR NUEVA";
            if (textoLinaje    != null) textoLinaje.text = "";
            if (iconoBacteria  != null) iconoBacteria.color = Color.white;
            OcultarTextosEstadisticas();
        }
        else
        {
            if (textoNombre != null) textoNombre.text = nombre;
            if (textoLinaje != null) textoLinaje.text = $"L: #{id:D2}";
            if (iconoBacteria != null) iconoBacteria.color = stats.colorLinaje;

            if (textoVidaUtil     != null) textoVidaUtil.text     = $"{stats.vidaUtil:F1}";
            if (textoMitosis      != null) textoMitosis.text      = $"{stats.tiempreEntreReproduccion:F1}";
            if (textoEnergia      != null) textoEnergia.text      = $"{stats.energiaMax:F0}";
            if (textoVelocidad    != null) textoVelocidad.text    = $"{stats.velocidad:F2}";
            if (textoVision       != null) textoVision.text       = $"{stats.radioVision:F2}";
            if (textoTamano       != null) textoTamano.text       = $"{stats.tamano:F2}";
            if (textoConsumo      != null) textoConsumo.text      = $"{stats.consumo:F2}";

            if (textoCosteMitosis != null)
            {
                float costo = DatosGeneticos.CalcularCosteReproduccion(stats.consumo, stats.tiempreEntreReproduccion);
                textoCosteMitosis.text = $"{costo:F0}";
            }
        }

        if (botonSeleccion != null)
        {
            botonSeleccion.onClick.RemoveAllListeners();
            botonSeleccion.onClick.AddListener(AlPulsarCarta);
        }
    }

    private void OcultarTextosEstadisticas()
    {
        if (textoVidaUtil     != null) textoVidaUtil.gameObject.SetActive(false);
        if (textoMitosis      != null) textoMitosis.gameObject.SetActive(false);
        if (textoCosteMitosis != null) textoCosteMitosis.gameObject.SetActive(false);
        if (textoEnergia      != null) textoEnergia.gameObject.SetActive(false);
        if (textoVelocidad    != null) textoVelocidad.gameObject.SetActive(false);
        if (textoVision       != null) textoVision.gameObject.SetActive(false);
        if (textoTamano       != null) textoTamano.gameObject.SetActive(false);
        if (textoConsumo      != null) textoConsumo.gameObject.SetActive(false);
    }

    private void AlPulsarCarta()
    {
        if (_controladorInteraccion == null) return;

        if (_idLinajeAsignado == -1)
            _controladorInteraccion.AbrirLaboratorio();
        else
        {
            _controladorInteraccion.SeleccionarPincelEspecie(_idLinajeAsignado);
            BandejaEspeciesUI.NotificarSeleccion(this);
        }
    }
}
