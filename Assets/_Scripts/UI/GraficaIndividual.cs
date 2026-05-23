using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

/*
- Cada grafica individual representa una estadistica concreta (velocidad, tamanno, etc)
- Se encarga de dibujar la linea de esa estadistica a lo largo del tiempo, y actualizar los textos de valor actual, maximo y minimo
- Tambien se encarga de abrir el visor grande con su estadistica concreta al hacer doble
clic sobre ella
- Para dibujar la linea, normaliza los valores historicos al rango visible en ese momento
- El rango visible se puede configurar para que sea el rango global de esa especie (minimo y maximo registrado en toda la simulacion) 
o el rango local de los datos visibles (minimo y maximo entre los ultimos X puntos)
- Tiene un sistema de acordeon para mostrar u ocultar la grafica y sus textos asociados
- Ajusta el grosor de la linea segun el zoom actual del visor grande, para que se vea bien tanto de cerca como de lejos
*/

public class GraficaIndividual : MonoBehaviour, IPointerClickHandler
{
    // --- CONFIGURACON ---
    public enum TipoEstadistica { Velocidad, Vision, Tamanno, Consumo, EnergiaMaxima, EsperanzaDeVida }

    [Header("Configuraci�n General")]
    public TipoEstadistica estadisticaAsignada;
    private RangoEstadisticoEspecie rangoEspecieAsignada;

    [Header("Referencias UI Gr�fica")]
    [SerializeField] private LineRenderer lineaGrafica;
    [SerializeField] private TextMeshProUGUI textoActual;
    [SerializeField] private TextMeshProUGUI textoMax;
    [SerializeField] private TextMeshProUGUI textoMin;
    private RectTransform cuerpoRect;
    private List<EspeciesSnapshot> historialGuardado;
    [Header("Info Extra (Opcional)")]
    // Es para la energia maxima
    [SerializeField] private TextMeshProUGUI textoInfoSecundaria;

    [Header("Referencias Acorde�n")]
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
            textoTituloBoton.text = FormatearNombre(estadisticaAsignada.ToString());
        }

        if (cuerpoGrafica != null) 
        {
            cuerpoRect = cuerpoGrafica.GetComponent<RectTransform>();
            cuerpoGrafica.SetActive(false);

            }

        ActualizarLayoutPadre();
    }

    private void Update()
    {
        if (estaExpandido)
        {
            if (VisorGraficaGrande.Instance != null && VisorGraficaGrande.Instance.CamaraPrincipal != null)
            {
                // Leemos el zoom directamente de la referencia que YA tenemos en el Singleton
                float zoomActual = VisorGraficaGrande.Instance.CamaraPrincipal.orthographicSize;

                // Aplicamos la proporcion
                // Multiplicamos para que a mas zoom (camara lejos), la linea sea mas gruesa.
                lineaGrafica.widthMultiplier = zoomActual * 0.05f;
            }
        }



    }

    // Convierte "EsperanzaDeVida" en "Esperanza De Vida"
    private string FormatearNombre(string nombreOriginal)
    {
        // Usamos Regex para buscar cualquier mayuscula ([A-Z]) que NO ested al principio (\B)
        // y le ponemos un espacio delante (" $1").
        string conEspacios = Regex.Replace(nombreOriginal, "(\\B[A-Z])", " $1");

        //Para que empiecen por mayusculas cometnar estas lienas
         conEspacios = conEspacios.ToLower(); 
         conEspacios = char.ToUpper(conEspacios[0]) + conEspacios.Substring(1);

        return conEspacios;
    }

    // --- 2. LOGICA DEL ACORDEON ---
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

    // --- 3. LOGICA DE LA GRAFICA ---
    public void ActualizarGrafica(List<EspeciesSnapshot> historial, RangoEstadisticoEspecie rango)
    {
        this.historialGuardado = historial;
        this.rangoEspecieAsignada = rango;
        if (VisorGraficaGrande.Instance != null && VisorGraficaGrande.Instance.estaAbierto)
        {
            if (VisorGraficaGrande.Instance.estadisticaActiva == this.estadisticaAsignada)
            {
                // Le mandamos los datos frescos para que se redibuje sola
                VisorGraficaGrande.Instance.AbrirVisor(historial, this.estadisticaAsignada, rango);
            }
        }
        if (historial == null || historial.Count == 0) return;

        int puntosADibujar = Mathf.Min(historial.Count, maxPuntosVisible);
        lineaGrafica.positionCount = puntosADibujar;
        int indiceInicio = historial.Count - puntosADibujar;

        //DEPRECATED NO TENGO NI IDEA DE QUE HACIA ESTO
        /*
        // --- PASO 1: ENCONTRAR EL RANGO REAL DE LOS DATOS VISIBLES ---
        float minVisible = float.MaxValue;
        float maxVisible = float.MinValue;

        // Recorremos los datos PRIMERO solo para encontrar los l�mites
        for (int i = 0; i < puntosADibujar; i++)
        {
            float valor = ObtenerValorDeSnapshot(historial[indiceInicio + i]);
            if (valor > maxVisible) maxVisible = valor;
            if (valor < minVisible) minVisible = valor;
        }

        // Protecci�n: Si todos los valores son iguales (l�nea plana), inventamos un rango peque�o
        if (maxVisible == minVisible)
        {
            maxVisible += 1f;
            minVisible -= 1f;
        }
        */

        Vector2 limitesGlobales = ObtenerLimitesGlobales();
        float minVisible = limitesGlobales.x;
        float maxVisible = limitesGlobales.y;

        if (Mathf.Approximately(minVisible, maxVisible))
        {
            minVisible -= 1f;
            maxVisible += 1f;
        }
        // --- ACTUALIZAR TEXTOS CON LA REALIDAD ---
        float valorActual = ObtenerValorDeSnapshot(historial[historial.Count - 1]);

        if (textoActual != null) textoActual.text = valorActual.ToString("F2");
        if (textoMax != null) textoMax.text = maxVisible.ToString("F2");
        if (textoMin != null) textoMin.text = minVisible.ToString("F2"); 

        // --- OBTENER LA ALTURA DE TU CAJA NEGRA ---
        float alturaCaja = 100f; // Valor por defecto por si falla el Rect
        if (cuerpoGrafica != null)
        {
            alturaCaja = cuerpoRect.rect.height;
        }

        // Dejamos un pequenno margen (padding) para que la linea no toque los bordes exactos (90% del espacio)
        float alturaUtil = alturaCaja * 0.9f;

        // --- DIBUJAR NORMALIZANDO LOS DATOS ---
        for (int i = 0; i < puntosADibujar; i++)
        {
            float xPos = i * separacionX;
            float valorHistorico = ObtenerValorDeSnapshot(historial[indiceInicio + i]);

            // Convierte el valor en un porcentaje entre 0 y 1 basandose en el min y max encontrados
            float porcentaje = (valorHistorico - minVisible) / (maxVisible - minVisible);

            // Convertimos ese porcentaje a posicion en pixeles
            // Restamos (alturaCaja / 2) porque el punto (0,0) esta en el centro del RectTransform
            float yPos = (porcentaje * alturaUtil) - (alturaUtil / 2f);

            lineaGrafica.SetPosition(i, new Vector3(xPos, yPos, 0));
            if (textoInfoSecundaria != null)
            {
                if (estadisticaAsignada == TipoEstadistica.Tamanno)
                {
                    // Calculamos la energia basada en el tamanno actual
                    float energiaCalculada = valorActual * 100f;
                    textoInfoSecundaria.text = $"(E. Max: {energiaCalculada:F0})"; // F0 para quitar decimales
                    textoInfoSecundaria.gameObject.SetActive(true);
                }
                else
                {
                    // Si no es Tamanno, ocultamos este texto para no molestar
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
            case TipoEstadistica.Tamanno: return snap.avgTamano;
            case TipoEstadistica.Consumo: return snap.avgConsumo;
            case TipoEstadistica.EnergiaMaxima: return snap.avgEnergia;
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
        // Si ha hecho 2 clics muy rapidos
        if (eventData.clickCount == 2)
        {
            Debug.Log("Doble clic detectado. Abriendo visor grande...");
            // --- DEBUG DE ERRORES ---
            if (VisorGraficaGrande.Instance == null)
                Debug.LogError(" ERROR CR�TICO: VisorGraficaGrande.Instance es NULL. �El objeto Visor existe en la escena? �Est� ACTIVADO?");

            if (historialGuardado == null)
                Debug.LogError(" ERROR CR�TICO: historialGuardado es NULL. No se est�n guardando los datos en ActualizarGrafica.");
            //
            if (VisorGraficaGrande.Instance != null && historialGuardado != null)
            {
                VisorGraficaGrande.Instance.AbrirVisor(historialGuardado, estadisticaAsignada, rangoEspecieAsignada);
            }
        }
    }
    private Vector2 ObtenerLimitesGlobales()
    {
        switch (estadisticaAsignada)
        {
            case TipoEstadistica.Velocidad: return new Vector2(rangoEspecieAsignada.minVel, rangoEspecieAsignada.maxVel);
            case TipoEstadistica.Vision: return new Vector2(rangoEspecieAsignada.minVision, rangoEspecieAsignada.maxVision);
            case TipoEstadistica.Tamanno: return new Vector2(rangoEspecieAsignada.minTamano, rangoEspecieAsignada.maxTamano);
            case TipoEstadistica.Consumo: return new Vector2(rangoEspecieAsignada.minConsumo, rangoEspecieAsignada.maxConsumo);
            case TipoEstadistica.EnergiaMaxima: return new Vector2(rangoEspecieAsignada.minEnergia, rangoEspecieAsignada.maxEnergia);
            case TipoEstadistica.EsperanzaDeVida: return new Vector2(rangoEspecieAsignada.minVidaUtil, rangoEspecieAsignada.maxVidaUtil);
            default: return new Vector2(0, 1);
        }
    }
}