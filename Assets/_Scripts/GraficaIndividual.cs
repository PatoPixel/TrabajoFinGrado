using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

public class GraficaIndividual : MonoBehaviour, IPointerClickHandler
{
    // --- 1. CONFIGURACIÓN ---
    public enum TipoEstadistica { Velocidad, Vision, Tamaño, Consumo, EnergíaMáxima, EsperanzaDeVida }

    [Header("Configuración General")]
    public TipoEstadistica estadisticaAsignada;

    [Header("Referencias UI Gráfica")]
    [SerializeField] private LineRenderer lineaGrafica;
    [SerializeField] private TextMeshProUGUI textoActual;
    [SerializeField] private TextMeshProUGUI textoMax;
    [SerializeField] private TextMeshProUGUI textoMin;
    private List<EspeciesSnapshot> historialGuardado;
    [Header("Info Extra (Opcional)")]
    // Es para la energia máxima
    [SerializeField] private TextMeshProUGUI textoInfoSecundaria;

    [Header("Referencias Acordeón")]
    [SerializeField] private GameObject cuerpoGrafica;
    [SerializeField] private Button botonCabecera;
    [SerializeField] private TextMeshProUGUI textoTituloBoton;

    [Header("Ajustes Visuales")]
    [SerializeField] private float separacionX = 0.1f;
    [SerializeField] private int maxPuntosVisible = 60;

    private bool estaExpandido = false;

    [SerializeField] private Camera camaraPrincipal;

    void Start()
    {
        if (botonCabecera != null)
        {
            botonCabecera.onClick.AddListener(AlternarDespliegue);
        }

        if (textoTituloBoton != null)
        {
            // AQUÍ LLAMAMOS A LA NUEVA FUNCIÓN DE FORMATEO
            textoTituloBoton.text = FormatearNombre(estadisticaAsignada.ToString());
        }

        if (cuerpoGrafica != null)
            cuerpoGrafica.SetActive(false);

        ActualizarLayoutPadre();
    }

    private void Update()
    {
        if (estaExpandido)
        {
            if (VisorGraficaGrande.Instance != null && VisorGraficaGrande.Instance.camaraPrincipal != null)
            {
                // 2. Leemos el zoom directamente de la referencia que YA tenemos en el Singleton
                float zoomActual = VisorGraficaGrande.Instance.camaraPrincipal.orthographicSize;

                // 3. Aplicamos la proporción (ajusta el 0.05f según lo que te guste)
                // Multiplicamos para que a más zoom (cámara lejos), la línea sea más gruesa.
                lineaGrafica.widthMultiplier = zoomActual * 0.05f;
            }
        }



    }

    // --- FUNCIÓN NUEVA: CONVIERTE "EsperanzaDeVida" en "Esperanza De Vida" ---
    private string FormatearNombre(string nombreOriginal)
    {
        // 1. Usamos Regex para buscar cualquier mayúscula ([A-Z]) que NO esté al principio (\B)
        // y le ponemos un espacio delante (" $1").
        string conEspacios = Regex.Replace(nombreOriginal, "(\\B[A-Z])", " $1");

        //Para que empiecen por mayusculas cometnar estas lienas
         conEspacios = conEspacios.ToLower(); 
         conEspacios = char.ToUpper(conEspacios[0]) + conEspacios.Substring(1);

        return conEspacios;
    }

    // --- 2. LÓGICA DEL ACORDEÓN ---
    public void AlternarDespliegue()
    {
        estaExpandido = !estaExpandido;
        if (cuerpoGrafica != null) cuerpoGrafica.SetActive(estaExpandido);
        ActualizarLayoutPadre();
    }

    private void ActualizarLayoutPadre()
    {
        StartCoroutine(ForzarReconstruccion());
    }

    private IEnumerator ForzarReconstruccion()
    {
        yield return new WaitForEndOfFrame();
        if (transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        }
    }

    // --- 3. LÓGICA DE LA GRÁFICA ---
    public void ActualizarGrafica(List<EspeciesSnapshot> historial)
    {
        this.historialGuardado = historial;
        if (VisorGraficaGrande.Instance != null && VisorGraficaGrande.Instance.estaAbierto)
        {
            if (VisorGraficaGrande.Instance.estadisticaActiva == this.estadisticaAsignada)
            {
                // Le mandamos los datos frescos para que se redibuje sola
                VisorGraficaGrande.Instance.AbrirVisor(historial, this.estadisticaAsignada);
            }
        }
        if (historial == null || historial.Count == 0) return;

        int puntosADibujar = Mathf.Min(historial.Count, maxPuntosVisible);
        lineaGrafica.positionCount = puntosADibujar;
        int indiceInicio = historial.Count - puntosADibujar;

        // --- PASO 1: ENCONTRAR EL RANGO REAL DE LOS DATOS VISIBLES ---
        float minVisible = float.MaxValue;
        float maxVisible = float.MinValue;

        // Recorremos los datos PRIMERO solo para encontrar los límites
        for (int i = 0; i < puntosADibujar; i++)
        {
            float valor = ObtenerValorDeSnapshot(historial[indiceInicio + i]);
            if (valor > maxVisible) maxVisible = valor;
            if (valor < minVisible) minVisible = valor;
        }

        // Protección: Si todos los valores son iguales (línea plana), inventamos un rango pequeño
        if (maxVisible == minVisible)
        {
            maxVisible += 1f;
            minVisible -= 1f;
        }

        // --- PASO 2: ACTUALIZAR TEXTOS CON LA REALIDAD ---
        float valorActual = ObtenerValorDeSnapshot(historial[historial.Count - 1]);

        if (textoActual != null) textoActual.text = valorActual.ToString("F2");
        if (textoMax != null) textoMax.text = maxVisible.ToString("F2"); // Ahora muestra el pico real
        if (textoMin != null) textoMin.text = minVisible.ToString("F2"); // Ahora muestra el suelo real

        // --- PASO 3: OBTENER LA ALTURA DE TU CAJA NEGRA ---
        float alturaCaja = 100f; // Valor por defecto por si falla el Rect
        if (cuerpoGrafica != null)
        {
            alturaCaja = cuerpoGrafica.GetComponent<RectTransform>().rect.height;
        }

        // Dejamos un pequeño margen (padding) para que la línea no toque los bordes exactos (90% del espacio)
        float alturaUtil = alturaCaja * 0.9f;

        // --- PASO 4: DIBUJAR NORMALIZANDO LOS DATOS ---
        for (int i = 0; i < puntosADibujar; i++)
        {
            float xPos = i * separacionX;
            float valorHistorico = ObtenerValorDeSnapshot(historial[indiceInicio + i]);

            // LA FORMULA MAGICA DE NORMALIZACIÓN (Inverse Lerp)
            // Convierte el valor en un porcentaje entre 0 y 1 basándose en el min y max encontrados
            float porcentaje = (valorHistorico - minVisible) / (maxVisible - minVisible);

            // Convertimos ese porcentaje a posición en píxeles
            // Restamos (alturaCaja / 2) porque el punto (0,0) está en el centro del RectTransform
            float yPos = (porcentaje * alturaUtil) - (alturaUtil / 2f);

            lineaGrafica.SetPosition(i, new Vector3(xPos, yPos, 0));
            if (textoInfoSecundaria != null)
            {
                if (estadisticaAsignada == TipoEstadistica.Tamaño) // OJO: Asegúrate de escribirlo igual que en tu Enum
                {
                    // Calculamos la energía basada en el tamaño actual
                    float energiaCalculada = valorActual * 100f;
                    textoInfoSecundaria.text = $"(E. Máx: {energiaCalculada:F0})"; // F0 para quitar decimales
                    textoInfoSecundaria.gameObject.SetActive(true);
                }
                else
                {
                    // Si no es Tamaño, ocultamos este texto para no molestar
                    textoInfoSecundaria.gameObject.SetActive(false);
                }
            }
        }
    }

    private float ObtenerValorDeSnapshot(EspeciesSnapshot snap)
    {
        switch (estadisticaAsignada)
        {
            case TipoEstadistica.Velocidad: return snap.avgVel;
            case TipoEstadistica.Vision: return snap.avgVision;
            case TipoEstadistica.Tamaño: return snap.avgTamano;
            case TipoEstadistica.Consumo: return snap.avgConsumo;
            case TipoEstadistica.EnergíaMáxima: return snap.avgEnergia;
            case TipoEstadistica.EsperanzaDeVida: return snap.avgVidaUtil;
            default: return 0f;
        }
    }

    public void Limpiar()
    {
        if (lineaGrafica != null) lineaGrafica.positionCount = 0;
        if (textoActual != null) textoActual.text = "-";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Si ha hecho 2 clics muy rápidos
        if (eventData.clickCount == 2)
        {
            Debug.Log("Doble clic detectado. Abriendo visor grande...");
            // --- DEBUG DE ERRORES ---
            if (VisorGraficaGrande.Instance == null)
                Debug.LogError(" ERROR CRÍTICO: VisorGraficaGrande.Instance es NULL. ¿El objeto Visor existe en la escena? ¿Está ACTIVADO?");

            if (historialGuardado == null)
                Debug.LogError(" ERROR CRÍTICO: historialGuardado es NULL. No se están guardando los datos en ActualizarGrafica.");
            //
            if (VisorGraficaGrande.Instance != null && historialGuardado != null)
            {
                VisorGraficaGrande.Instance.AbrirVisor(historialGuardado, estadisticaAsignada);
            }
        }
    }
}