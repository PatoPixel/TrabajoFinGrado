using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VisorGraficaGrande : MonoBehaviour
{
    // SINGLETON: Para poder llamarlo desde cualquier lado sin arrastrar referencias
    public static VisorGraficaGrande Instance;

    [Header("Referencias")]
    public GameObject panelCompleto;     // El propio objeto (para apagarlo/encenderlo)
    public LineRenderer lineaGrande;
    public RectTransform areaGrafica;    // El Contenedor_Lineas
    public TextMeshProUGUI tituloGrafica;

    [Header("Ejes")]
    public Transform contenedorEjeY;     // Donde nacen los números verticales
    public Transform contenedorEjeX;     // Donde nacen los números horizontales
    public GameObject prefabTextoEje;    // El prefab del numerito

    private void Awake()
    {
        Instance = this;
        panelCompleto.SetActive(false); // Empezamos ocultos
    }

    // --- FUNCIÓN PRINCIPAL QUE LLAMARÁS AL DOBLE CLICK ---
    public void AbrirVisor(List<EspeciesSnapshot> historialCompleto, GraficaIndividual.TipoEstadistica tipo)
    {
        Debug.Log("Abriendo visor grande para " + tipo.ToString() + " con " + historialCompleto.Count + " puntos.");
        panelCompleto.SetActive(true);
        tituloGrafica.text = "Historial Completo: " + tipo.ToString();

        // 1. Limpiar basura anterior
        LimpiarEjes();

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

            CrearTextoEje(contenedorEjeY, valorNum.ToString("F1"), new Vector2(-20, posY)); // Un poco a la izquierda
        }
    }

    private void DibujarEjeX(int totalGeneraciones)
    {
        // Ponemos una marca cada 10 o 20 generaciones para no saturar
        // Calculamos un salto inteligente (si hay 1000 gen, saltamos de 100 en 100)
        int salto = Mathf.Max(1, totalGeneraciones / 10);
        float ancho = areaGrafica.rect.width;

        for (int i = 0; i < totalGeneraciones; i += salto)
        {
            float porcentaje = (float)i / (totalGeneraciones - 1);
            float posX = (porcentaje * ancho) - (ancho / 2f);

            CrearTextoEje(contenedorEjeX, "Gen " + i, new Vector2(posX, -20)); // Un poco abajo
        }
    }

    private void CrearTextoEje(Transform padre, string texto, Vector2 posLocal)
    {
        GameObject obj = Instantiate(prefabTextoEje, padre);
        // OJO: Importante resetear la escala al instanciar en UI
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = posLocal;
        obj.GetComponent<TextMeshProUGUI>().text = texto;
    }

    private void LimpiarEjes()
    {
        foreach (Transform child in contenedorEjeY) Destroy(child.gameObject);
        foreach (Transform child in contenedorEjeX) Destroy(child.gameObject);
    }

    public void Cerrar()
    {
        panelCompleto.SetActive(false);
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
}