using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/*
- Este script se encarga de mostrar una grafica grande con el historial completo de una estadistica
cuando el jugador hace doble click sobre una grafica pequeña de una bacteria.
- Es un visor detallado que muestra toda la evolucion de esa estadistica a lo largo del
tiempo, con ejes escalados dinamicamente segun el rango de valores.
- Utiliza un sistema de pool para los textos de los ejes, evitando instanciaciones y destrucciones constantes.
- Se comunica con el EvolutionTracker para obtener los datos históricos de la especie seleccionada.
- Se activa y desactiva el panel completo del visor, y ajusta el grosor de la línea según el zoom de la cámara.
*/


public class VisorGraficaGrande : MonoBehaviour
{
    // SINGLETON: Para poder llamarlo desde cualquier lado sin arrastrar referencias
    public static VisorGraficaGrande Instance;

    [HideInInspector] public bool estaAbierto = false;
    [HideInInspector] public GraficaIndividual.TipoEstadistica estadisticaActiva;

    [Header("Referencias")]
    public GameObject panelCompleto;     // El propio objeto (para apagarlo/encenderlo)
    public LineRenderer lineaGrande;
    public RectTransform areaGrafica;    // El Contenedor_Lineas
    public TextMeshProUGUI tituloGrafica;

    [Header("Ejes")]
    public Transform contenedorEjeY;     // Donde nacen los numeros verticales
    public Transform contenedorEjeX;     // Donde nacen los numeros horizontales
    public GameObject prefabTextoEje;    // El prefab del numerito

    [SerializeField] private Camera camaraPrincipal;

    public Camera CamaraPrincipal { get { return camaraPrincipal; } }
    private Stack<GameObject> _poolTextos = new Stack<GameObject>();


    private void Awake()
    {
        Instance = this;
        panelCompleto.SetActive(false); // Empezamos ocultos
    }

    private void Update()
    {
        if (estaAbierto)
        {
            lineaGrande.widthMultiplier = 0.005f * camaraPrincipal.orthographicSize; // Ajusta el grosor segun el zoom

        }
    }

    public void AbrirVisor(List<EspeciesSnapshot> historialCompleto, GraficaIndividual.TipoEstadistica tipo, RangoEstadisticoEspecie rango)
    {
        if (historialCompleto.Count <= 1)
        {
            Debug.LogWarning("No hay datos para mostrar en el visor grande de " + tipo.ToString());
            return;
        }
        Debug.Log("Abriendo visor grande para " + tipo.ToString() + " con " + historialCompleto.Count + " puntos.");
        estaAbierto = true;
        estadisticaActiva = tipo;
        panelCompleto.SetActive(true);
        tituloGrafica.text = "Historial Completo: " + tipo.ToString();

        LimpiarEjes(); // Limpiamos cualquier numero viejo antes de dibujar

        // Obtener rango limites (Min y Max) para escalar la grafica
        Vector2 limites = ObtenerLimites(rango, tipo);

        float minValor = limites.x;
        float maxValor = limites.y;

        // Evitar linea plana
        if (Mathf.Approximately(maxValor, minValor)) { maxValor += 1; minValor -= 1; }

        // Dibujar Eje Y (Vertical - Valores)
        DibujarEjeY(minValor, maxValor);

        // Dibujar Eje X (Horizontal - Generaciones)
        DibujarEjeX(historialCompleto.Count);

        // Dibujar la Linea con el historial completo
        DibujarLinea(historialCompleto, tipo, minValor, maxValor);
    }

    private void DibujarLinea(List<EspeciesSnapshot> historia, GraficaIndividual.TipoEstadistica tipo, float min, float max)
    {
        int puntos = historia.Count;
        lineaGrande.positionCount = puntos;
        float ancho = areaGrafica.rect.width;
        float alto = areaGrafica.rect.height;

        for (int i = 0; i < puntos; i++)
        {
            float valor = ObtenerValor(historia[i], tipo);

            // Normalizamos (0 a 1)
            float normX = (float)i / (float)(puntos - 1);
            float normY = Mathf.InverseLerp(min, max, valor);

            // Convertimos a coordenadas locales del rect
            // Restamos ancho/2 y alto/2 porque el (0,0) del RectTransform suele ser el centro
            float posX = (normX * ancho) - (ancho / 2f);
            float posY = (normY * alto) - (alto / 2f);

            lineaGrande.SetPosition(i, new Vector3(posX, posY, 0));
        }
    }

    private void DibujarEjeY(float min, float max)
    {
        // Hay 5 marcas fijas: Min, 25%, 50%, 75%, Max
        int pasos = 5;
        float alto = areaGrafica.rect.height;

        for (int i = 0; i < pasos; i++)
        {
            float porcentaje = (float)i / (pasos - 1);
            float valorNum = Mathf.Lerp(min, max, porcentaje);
            float posY = (porcentaje * alto) - (alto / 2f);
            ObtenerTextoDelPool(contenedorEjeY, new Vector2(0, posY)).GetComponent<TextMeshProUGUI>().text = valorNum.ToString("F2"); // Un poco a la izquierda
        }
    }
    private void DibujarEjeX(int totalPuntosDeTiempo)
    {


        int salto = Mathf.Max(1, totalPuntosDeTiempo / 10);
        float ancho = areaGrafica.rect.width;

        int segundosPorPunto = 1;

        for (int i = 0; i < totalPuntosDeTiempo; i += salto)
        {
            float porcentaje = (float)i / (totalPuntosDeTiempo - 1);
            float posX = (porcentaje * ancho) - (ancho / 2f);

            // Convertimos el inidice 'i' a segundos totales
            int segundosTotales = i * segundosPorPunto;

            // Llamamos a la magia del formateo
            string textoEje = FormatearTiempo(segundosTotales);
            ObtenerTextoDelPool(contenedorEjeX, new Vector2(posX, -20)).GetComponent<TextMeshProUGUI>().text = textoEje; // Un poco abajo

        }


    }

    private string FormatearTiempo(int segundos)
    {
        if (segundos < 60)
        {
            // Menos de 1 minuto: Mostramos segundos enteros (Ej: "45s")
            return segundos + "s";
        }
        else if (segundos < 3600)
        {
            // Entre 1 minuto y 1 hora: Mostramos minutos con 1 decimal si hace falta (Ej: "1.5m")
            float minutos = segundos / 60f;
            return minutos.ToString("F1") + "m";
        }
        else if (segundos < 86400)
        {
            // Entre 1 hora y 1 dia: Mostramos horas con 1 decimal (Ej: "2.4h")
            float horas = segundos / 3600f;
            return horas.ToString("F1") + "h";
        }
        else
        {
            // Mas de 1 dia: Mostramos dias (Ej: "Dia 1.2")
            float dias = segundos / 86400f;
            return "D�a " + dias.ToString("F1");
        }
    }

    public void Cerrar()
    {
        panelCompleto.SetActive(false);
        estaAbierto = false;
        panelCompleto.SetActive(false);
        // Devolvemos los textos al pool en lugar de destruirlos
        LimpiarEjes();
    }
    public void LimpiarEjes()
    {
        // Devolvemos los textos al pool en lugar de destruirlos
        while (contenedorEjeY.childCount > 0)
        {
            DevolverTextoAlPool(contenedorEjeY.GetChild(0).gameObject);
        }
        while (contenedorEjeX.childCount > 0)
        {
            // Cogemos al primero de la fila y lo mandamos al almacen.
            DevolverTextoAlPool(contenedorEjeX.GetChild(0).gameObject);
        }
    }


    private float ObtenerValor(EspeciesSnapshot snap, GraficaIndividual.TipoEstadistica tipo)
    {
        switch (tipo)
        {
            case GraficaIndividual.TipoEstadistica.Velocidad: return snap.avgVel;
            case GraficaIndividual.TipoEstadistica.Vision: return snap.avgVision;
            case GraficaIndividual.TipoEstadistica.Tama�o: return snap.avgTamano;
            case GraficaIndividual.TipoEstadistica.Consumo: return snap.avgConsumo;
            case GraficaIndividual.TipoEstadistica.Energ�aM�xima: return snap.avgEnergia;
            case GraficaIndividual.TipoEstadistica.EsperanzaDeVida: return snap.avgVidaUtil;
            default: return 0;
        }
    }

    private GameObject ObtenerTextoDelPool(Transform padre, Vector2 posLocal)
    {
        GameObject textoObj;

        if (_poolTextos.Count > 0)
        {
            textoObj = _poolTextos.Pop();
            textoObj.SetActive(true);
            textoObj.transform.SetParent(padre, false);
        }
        else
        {
            textoObj = Instantiate(prefabTextoEje, padre);
            textoObj.transform.localScale = Vector3.one; // Aseguramos escala correcta
        }

        // Finalmente, le aplicamos la posicion (esto sirve para ambos casos)
        textoObj.transform.localPosition = posLocal;
        return textoObj;
    }


    // 3.Detroy en vez de destruir, lo que hacemos es devolverlo al pool para reutilizarlo luego
    private void DevolverTextoAlPool(GameObject textoObj)
    {
 
        if (textoObj != null)
        {
            textoObj.SetActive(false);
            _poolTextos.Push(textoObj);
            textoObj.transform.SetParent(this.transform, false);
        }
    }

    private Vector2 ObtenerLimites(RangoEstadisticoEspecie rango, GraficaIndividual.TipoEstadistica tipo)
    {
        switch (tipo)
        {
            case GraficaIndividual.TipoEstadistica.Velocidad: return new Vector2(rango.minVel, rango.maxVel);
            case GraficaIndividual.TipoEstadistica.Vision: return new Vector2(rango.minVision, rango.maxVision);
            case GraficaIndividual.TipoEstadistica.Tama�o: return new Vector2(rango.minTamano, rango.maxTamano);
            case GraficaIndividual.TipoEstadistica.Consumo: return new Vector2(rango.minConsumo, rango.maxConsumo);
            case GraficaIndividual.TipoEstadistica.Energ�aM�xima: return new Vector2(rango.minEnergia, rango.maxEnergia);
            case GraficaIndividual.TipoEstadistica.EsperanzaDeVida: return new Vector2(rango.minVidaUtil, rango.maxVidaUtil);
            default: return Vector2.zero;
        }
    }
}