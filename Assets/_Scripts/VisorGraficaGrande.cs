using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class VisorGraficaGrande : MonoBehaviour
{
    // SINGLETON: Para poder llamarlo desde cualquier lado sin arrastrar referencias
    public static VisorGraficaGrande Instance;

    // Añade estas dos variables nuevas
    [HideInInspector] public bool estaAbierto = false;
    [HideInInspector] public GraficaIndividual.TipoEstadistica estadisticaActiva;

    [Header("Referencias")]
    public GameObject panelCompleto;     // El propio objeto (para apagarlo/encenderlo)
    public LineRenderer lineaGrande;
    public RectTransform areaGrafica;    // El Contenedor_Lineas
    public TextMeshProUGUI tituloGrafica;

    [Header("Ejes")]
    public Transform contenedorEjeY;     // Donde nacen los números verticales
    public Transform contenedorEjeX;     // Donde nacen los números horizontales
    public GameObject prefabTextoEje;    // El prefab del numerito

    [SerializeField] public Camera camaraPrincipal; 
    private void Awake()
    {
        Instance = this;
        panelCompleto.SetActive(false); // Empezamos ocultos
    }

    private void Update()
    {
        if (estaAbierto)
        {
            lineaGrande.widthMultiplier = 0.005f * camaraPrincipal.orthographicSize; // Ajusta el grosor según el zoom

        }
    }

    // --- FUNCIÓN PRINCIPAL QUE LLAMARÁS AL DOBLE CLICK ---
    public void AbrirVisor(List<EspeciesSnapshot> historialCompleto, GraficaIndividual.TipoEstadistica tipo)
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

        LimpiarEjes(); // Limpiamos cualquier número viejo antes de dibujar

        // 2. Calcular Máximos y Mínimos Reales
        float minValor = float.MaxValue;
        float maxValor = float.MinValue;

        foreach (var snap in historialCompleto)
        {
            float val = ObtenerValor(snap, tipo);
            if (val > maxValor) maxValor = val;
            if (val < minValor) minValor = val;
        }

        // Evitar línea plana
        if (Mathf.Approximately(maxValor, minValor)) { maxValor += 1; minValor -= 1; }

        // 3. DIBUJAR EJE Y (Vertical - Valores)
        DibujarEjeY(minValor, maxValor);

        // 4. DIBUJAR EJE X (Horizontal - Generaciones)
        DibujarEjeX(historialCompleto.Count);

        // 5. DIBUJAR LA LÍNEA
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
        // Vamos a poner 5 marcas fijas: Min, 25%, 50%, 75%, Max
        int pasos = 5;
        float alto = areaGrafica.rect.height;

        for (int i = 0; i < pasos; i++)
        {
            float porcentaje = (float)i / (pasos - 1);
            float valorNum = Mathf.Lerp(min, max, porcentaje);
            float posY = (porcentaje * alto) - (alto / 2f); // Posición local Y
            ObtenerTextoDelPool(contenedorEjeY, new Vector2(-20, posY)).GetComponent<TextMeshProUGUI>().text = valorNum.ToString("F2"); // Un poco a la izquierda
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

            // Convertimos el índice 'i' a segundos totales
            int segundosTotales = i * segundosPorPunto;

            // Llamamos a la magia del formateo
            string textoEje = FormatearTiempo(segundosTotales);
            ObtenerTextoDelPool(contenedorEjeX, new Vector2(posX, -20)).GetComponent<TextMeshProUGUI>().text = textoEje; // Un poco abajo

        }


    }

    // --- FUNCIÓN NUEVA: ESCALADO DINÁMICO DE TIEMPO ---
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
            // Entre 1 hora y 1 día: Mostramos horas con 1 decimal (Ej: "2.4h")
            float horas = segundos / 3600f;
            return horas.ToString("F1") + "h";
        }
        else
        {
            // Más de 1 día: Mostramos días (Ej: "Día 1.2")
            float dias = segundos / 86400f;
            return "Día " + dias.ToString("F1");
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
            // Cogemos al primero de la fila y lo mandamos al almacén.
            DevolverTextoAlPool(contenedorEjeX.GetChild(0).gameObject);
        }
    }

    // Tu traductor de siempre
    private float ObtenerValor(EspeciesSnapshot snap, GraficaIndividual.TipoEstadistica tipo)
    {
        switch (tipo)
        {
            case GraficaIndividual.TipoEstadistica.Velocidad: return snap.avgVel;
            case GraficaIndividual.TipoEstadistica.Vision: return snap.avgVision;
            case GraficaIndividual.TipoEstadistica.Tamaño: return snap.avgTamano;
            case GraficaIndividual.TipoEstadistica.Consumo: return snap.avgConsumo;
            case GraficaIndividual.TipoEstadistica.EnergíaMáxima: return snap.avgEnergia;
            case GraficaIndividual.TipoEstadistica.EsperanzaDeVida: return snap.avgVidaUtil;
            default: return 0;
        }
    }

    // 1. EL ALMACÉN (Añade esto arriba con tus variables)
    private Stack<GameObject> _poolTextos = new Stack<GameObject>();


    // 2. EL NUEVO "INSTANTIATE" (Úsalo en DibujarEjeX y DibujarEjeY)
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

        // Finalmente, le aplicamos la posición (esto sirve para ambos casos)
        textoObj.transform.localPosition = posLocal;
        return textoObj;
    }


    // 3. EL NUEVO "DESTROY" (Úsalo en tu método LimpiarEjes)
    private void DevolverTextoAlPool(GameObject textoObj)
    {
 
        if (textoObj != null)
        {
            textoObj.SetActive(false);
            _poolTextos.Push(textoObj);
            textoObj.transform.SetParent(this.transform, false);
        }
    }
}