using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaHerramientaUI : MonoBehaviour
{
    [Header("Textos e Inputs")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TMP_InputField inputValorNumerico;

    [Header("Visuales")]
    [SerializeField] private Image iconoPrincipal;

    [Header("Botones")]
    [SerializeField] private Button botonSeleccionCarta;
    [SerializeField] private Button botonModificar;

    private int _idHerramienta;
    private ControladorInteraccion _controladorInteraccion;

    public void Inicializar(int id, string titulo, string valorPorDefecto, Color colorIcono, ControladorInteraccion ctrl)
    {
        _idHerramienta = id;
        _controladorInteraccion = ctrl;

        if (textoNombre != null) textoNombre.text = titulo;

        if (inputValorNumerico != null)
        {
            inputValorNumerico.text = valorPorDefecto;
            inputValorNumerico.interactable = false;
        }

        if (iconoPrincipal != null) iconoPrincipal.color = colorIcono;

        if (botonSeleccionCarta != null)
        {
            botonSeleccionCarta.onClick.RemoveAllListeners();
            botonSeleccionCarta.onClick.AddListener(AlPulsarCartaEntera);
        }

        if (botonModificar != null)
        {
            botonModificar.onClick.RemoveAllListeners();
            botonModificar.onClick.AddListener(AlPulsarModificar);
        }
    }

    private void AlPulsarCartaEntera()
    {
        if (_controladorInteraccion == null) return;

        if (_idHerramienta == -1)
        {
            _controladorInteraccion.AbrirLaboratorio();
        }
        else if (_idHerramienta == -2)
        {
            int energiaElegida = 50; // Valor por defecto por si está vacío
            if (inputValorNumerico != null && !string.IsNullOrEmpty(inputValorNumerico.text))
            {
                int.TryParse(inputValorNumerico.text, out energiaElegida);
            }
            energiaElegida = Mathf.Max(1, energiaElegida);

            // Corregimos el texto en la UI por si el usuario escribió un número negativo o 0
            if (inputValorNumerico != null) inputValorNumerico.text = energiaElegida.ToString();

            Debug.Log($"Pincel de Comida equipado. Energía configurada: {energiaElegida}");

            _controladorInteraccion.energiaPincelComida = energiaElegida;

            _controladorInteraccion.SeleccionarPincelEspecie(-2);

            // Bloqueamos el input al terminar de seleccionar para que quede estético
            if (inputValorNumerico != null) inputValorNumerico.interactable = false;
        }
    }

    private void AlPulsarModificar()
    {
        if (inputValorNumerico != null)
        {
            inputValorNumerico.interactable = true;
            inputValorNumerico.Select();
        }
    }
}