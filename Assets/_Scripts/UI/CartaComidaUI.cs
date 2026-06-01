using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaComidaUI : MonoBehaviour
{
    [Header("UI Pincel Comida")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TMP_InputField  inputEnergia;

    [Header("Botones")]
    [SerializeField] private Button botonSeleccionCarta;
    [SerializeField] private Button botonModificar;

    private ControladorInteraccion _controladorInteraccion;
    private Image _fondo;

    private static readonly Color COLOR_SELECCIONADO = new Color(1f, 0.85f, 0.1f, 1f);
    private static readonly Color COLOR_NORMAL       = Color.white;

    private void Awake() => _fondo = GetComponent<Image>();

    private void OnEnable()  => BandejaEspeciesUI.OnCartaSeleccionada += OnSeleccionCambio;
    private void OnDisable() => BandejaEspeciesUI.OnCartaSeleccionada -= OnSeleccionCambio;

    private void OnSeleccionCambio(MonoBehaviour seleccionado)
    {
        if (_fondo) _fondo.color = (seleccionado == this) ? COLOR_SELECCIONADO : COLOR_NORMAL;
    }

    public void Inicializar(string titulo, string valorDefecto, ControladorInteraccion ctrl)
    {
        _controladorInteraccion = ctrl;

        if (textoNombre  != null) textoNombre.text = titulo;
        if (inputEnergia != null) { inputEnergia.text = valorDefecto; inputEnergia.interactable = false; }

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

        int energiaElegida = 50;
        if (inputEnergia != null && !string.IsNullOrEmpty(inputEnergia.text))
            int.TryParse(inputEnergia.text, out energiaElegida);

        energiaElegida = Mathf.Max(1, energiaElegida);
        if (inputEnergia != null) { inputEnergia.text = energiaElegida.ToString(); inputEnergia.interactable = false; }

        _controladorInteraccion.ConfigurarHerramientaComida(energiaElegida);

        BandejaEspeciesUI.NotificarSeleccion(this);
    }

    private void AlPulsarModificar()
    {
        if (inputEnergia != null) { inputEnergia.interactable = true; inputEnergia.Select(); }
    }
}
