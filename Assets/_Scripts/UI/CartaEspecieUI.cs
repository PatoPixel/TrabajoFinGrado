using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartaEspecieUI : MonoBehaviour
{
    [Header("Textos Principales")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoLinaje;

    [Header("Textos de Estadísticas")]
    [SerializeField] private TextMeshProUGUI textoVidaUtil;
    [SerializeField] private TextMeshProUGUI textoMitosis; // Asumo que es el tiempo entre reproducción
    [SerializeField] private TextMeshProUGUI textoCosteMitosis;
    [SerializeField] private TextMeshProUGUI textoEnergia; // Asumo que es energía máxima
    [SerializeField] private TextMeshProUGUI textoVelocidad;
    [SerializeField] private TextMeshProUGUI textoVision;
    [SerializeField] private TextMeshProUGUI textoTamano;
    [SerializeField] private TextMeshProUGUI textoConsumo; // Para el texto que tenías duplicado

    [Header("Visuales")]
    [SerializeField] private Image iconoBacteria;
    [SerializeField] private Button botonSeleccion;

    private int _idLinajeAsignado;
    private ControladorInteraccion _controladorInteraccion;

    // Ahora recibimos el struct DatosGeneticos entero
    public void Inicializar(int id, string nombre, DatosGeneticos stats, ControladorInteraccion ctrl)
    {
        _idLinajeAsignado = id;
        _controladorInteraccion = ctrl;

        if (id == -1)
        {
            // --- MODO: BOTÓN DE CREAR NUEVO (LABORATORIO) ---
            if (textoNombre != null) textoNombre.text = "+ CREAR NUEVA";
            if (textoLinaje != null) textoLinaje.text = "";
            if (iconoBacteria != null) iconoBacteria.color = Color.white; // Color neutral

            OcultarTextosEstadisticas();
        }
        else
        {
            // --- MODO: CARTA DE ESPECIE EXISTENTE ---
            if (textoNombre != null) textoNombre.text = nombre;
            if (textoLinaje != null) textoLinaje.text = $"L: #{id:D2}";

            // Tintamos tu imagen de la bacteria con el color de su linaje
            if (iconoBacteria != null) iconoBacteria.color = stats.colorLinaje;

            // Rellenamos tus campos matemáticos
            if (textoVidaUtil != null) textoVidaUtil.text = $"{stats.vidaUtil:F1}";
            if (textoMitosis != null) textoMitosis.text = $"{stats.tiempreEntreReproduccion:F1}";
            if (textoEnergia != null) textoEnergia.text = $"{stats.energiaMax:F0}";
            if (textoVelocidad != null) textoVelocidad.text = $"{stats.velocidad:F2}";
            if (textoVision != null) textoVision.text = $"{stats.radioVision:F2}";
            if (textoTamano != null) textoTamano.text = $"{stats.tamano:F2}";
            if (textoConsumo != null) textoConsumo.text = $"{stats.consumo:F2}";

            // Calculamos el coste exacto de mitosis llamando a tu método
            if (textoCosteMitosis != null)
            {
                float costo = DatosGeneticos.CalcularCosteReproduccion(stats.consumo, stats.tiempreEntreReproduccion);
                textoCosteMitosis.text = $"{costo:F0}";
            }
        }

        // Conectamos el clic
        if (botonSeleccion != null)
        {
            botonSeleccion.onClick.RemoveAllListeners();
            botonSeleccion.onClick.AddListener(AlPulsarCarta);
        }
    }

    private void OcultarTextosEstadisticas()
    {
        if (textoVidaUtil != null) textoVidaUtil.gameObject.SetActive(false);
        if (textoMitosis != null) textoMitosis.gameObject.SetActive(false);
        if (textoCosteMitosis != null) textoCosteMitosis.gameObject.SetActive(false);
        if (textoEnergia != null) textoEnergia.gameObject.SetActive(false);
        if (textoVelocidad != null) textoVelocidad.gameObject.SetActive(false);
        if (textoVision != null) textoVision.gameObject.SetActive(false);
        if (textoTamano != null) textoTamano.gameObject.SetActive(false);
        if (textoConsumo != null) textoConsumo.gameObject.SetActive(false);
    }

    private void AlPulsarCarta()
    {
        if (_controladorInteraccion == null) return;

        if (_idLinajeAsignado == -1)
        {
         
            _controladorInteraccion.AbrirLaboratorio();
            
        }
        else
        {
            _controladorInteraccion.SeleccionarPincelEspecie(_idLinajeAsignado);
        }
    }
}