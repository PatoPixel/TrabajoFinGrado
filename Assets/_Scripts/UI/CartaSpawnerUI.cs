using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
- Controla la carta de spawner dentro de la bandeja de especies.
- Permite configurar el radio de efecto, energía mínima, máxima y intervalo de generación del spawner.
- Cambia su apariencia cuando es seleccionada para indicar que está activa.
- Notifica al ControladorInteraccion cuando se selecciona o modifica, para que pueda actualizar
    su estado y comportamiento.
*/

public class CartaSpawnerUI : MonoBehaviour
{
    [Header("UI Pincel Spawner")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TMP_InputField inputRadio;
    [SerializeField] private TMP_InputField inputMinE;
    [SerializeField] private TMP_InputField inputMaxE;
    [SerializeField] private TMP_InputField inputIntervalo;

    [Header("Botones")]
    [SerializeField] private Button botonSeleccionCarta;
    [SerializeField] private Button botonModificar;

    private ControladorInteraccion _controladorInteraccion;
    private Image _fondo;

    private static readonly Color COLOR_SELECCIONADO = new Color(1f, 0.85f, 0.1f, 1f);
    private static readonly Color COLOR_NORMAL       = Color.white;

    private void Awake()
    {
        Transform hijo = transform.Find("Fondo");
        _fondo = hijo != null ? hijo.GetComponent<Image>() : GetComponent<Image>();
    }

    private void OnEnable()  => BandejaEspeciesUI.OnCartaSeleccionada += OnSeleccionCambio;
    private void OnDisable() => BandejaEspeciesUI.OnCartaSeleccionada -= OnSeleccionCambio;

    private void OnSeleccionCambio(MonoBehaviour seleccionado)
    {
        if (_fondo) _fondo.color = (seleccionado == this) ? COLOR_SELECCIONADO : COLOR_NORMAL;
    }

    public void Inicializar(string titulo, string radioDefecto, string minE, string maxE,
                            string intervalo, ControladorInteraccion ctrl)
    {
        _controladorInteraccion = ctrl;

        if (textoNombre    != null) textoNombre.text = titulo;
        if (inputRadio     != null) { inputRadio.text     = radioDefecto; inputRadio.interactable     = false; }
        if (inputMinE      != null) { inputMinE.text      = minE;         inputMinE.interactable      = false; }
        if (inputMaxE      != null) { inputMaxE.text      = maxE;         inputMaxE.interactable      = false; }
        if (inputIntervalo != null) { inputIntervalo.text = intervalo;    inputIntervalo.interactable = false; }

        if (botonSeleccionCarta != null)
        {
            botonSeleccionCarta.onClick.RemoveAllListeners();
            botonSeleccionCarta.onClick.AddListener(AlPulsarCarta);
        }
        if (botonModificar != null)
        {
            botonModificar.onClick.RemoveAllListeners();
            botonModificar.onClick.AddListener(AlPulsarModificar);
        }
    }

    private void AlPulsarCarta()
    {
        if (_controladorInteraccion == null) return;

        // Valores por defecto
        float radio = 1.5f, minE = 30f, maxE = 70f, inter = 0.2f;

        // Parsear con InvariantCulture para evitar problemas de locale (coma vs punto)
        if (inputRadio     != null && !string.IsNullOrEmpty(inputRadio.text))
            float.TryParse(inputRadio.text,     NumberStyles.Float, CultureInfo.InvariantCulture, out radio);
        if (inputMinE      != null && !string.IsNullOrEmpty(inputMinE.text))
            float.TryParse(inputMinE.text,      NumberStyles.Float, CultureInfo.InvariantCulture, out minE);
        if (inputMaxE      != null && !string.IsNullOrEmpty(inputMaxE.text))
            float.TryParse(inputMaxE.text,      NumberStyles.Float, CultureInfo.InvariantCulture, out maxE);
        if (inputIntervalo != null && !string.IsNullOrEmpty(inputIntervalo.text))
            float.TryParse(inputIntervalo.text, NumberStyles.Float, CultureInfo.InvariantCulture, out inter);

        // Clampear valores
        radio = Mathf.Max(0.2f, radio);
        minE  = Mathf.Max(1f, minE);
        maxE  = Mathf.Max(minE + 1f, maxE);
        inter = Mathf.Max(0.05f, inter);

        // Actualizar display con InvariantCulture
        if (inputRadio     != null) inputRadio.text     = radio.ToString("F1", CultureInfo.InvariantCulture);
        if (inputMinE      != null) inputMinE.text      = minE.ToString("F0",  CultureInfo.InvariantCulture);
        if (inputMaxE      != null) inputMaxE.text      = maxE.ToString("F0",  CultureInfo.InvariantCulture);
        if (inputIntervalo != null) inputIntervalo.text = inter.ToString("F2", CultureInfo.InvariantCulture);

        _controladorInteraccion.ConfigurarHerramientaSpawner(minE, maxE, inter, radio);
        BloquearInputs();

        // Notificar selección visual
        BandejaEspeciesUI.NotificarSeleccion(this);
    }

    private void AlPulsarModificar()
    {
        if (inputRadio     != null) inputRadio.interactable     = true;
        if (inputMinE      != null) inputMinE.interactable      = true;
        if (inputMaxE      != null) inputMaxE.interactable      = true;
        if (inputIntervalo != null) inputIntervalo.interactable = true;
        if (inputRadio     != null) inputRadio.Select();
    }

    private void BloquearInputs()
    {
        if (inputRadio     != null) inputRadio.interactable     = false;
        if (inputMinE      != null) inputMinE.interactable      = false;
        if (inputMaxE      != null) inputMaxE.interactable      = false;
        if (inputIntervalo != null) inputIntervalo.interactable = false;
    }
}
