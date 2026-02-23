using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GraficaIndividual : MonoBehaviour
{
    // 1. CREAMOS UN DESPLEGABLE PARA ELEGIR QUÉ LEE ESTA GRÁFICA
    public enum TipoEstadistica { Velocidad, Vision, Tamaño, OtraNuevaQueInventes }

    [Header("¿Qué estadística muestra este panel?")]
    public TipoEstadistica estadisticaAsignada;



    [Header("Referencias de este Panel")]
    public TextMeshProUGUI textoTituloBoton;
    [SerializeField] private LineRenderer lineaGrafica;
    [SerializeField] private TextMeshProUGUI textoActual;
    [SerializeField] private TextMeshProUGUI textoMax;
    [SerializeField] private TextMeshProUGUI textoMin;

    [Header("Ajustes Visuales")]
    [SerializeField] private float separacionX = 0.01f;
    [SerializeField] private int maxPuntosVisible = 50;
    [SerializeField] private float multiplicadorY = 1f;
    [SerializeField] private float offsetY = 2f;

    void Start()
    {
        // Al nacer, el cajón mira qué estadística le ha tocado y se lo escribe al botón
        if (textoTituloBoton != null)
        {
            textoTituloBoton.text = estadisticaAsignada.ToString(); // Quedará como "VELOCIDAD"
        }
    }

    public void ActualizarGrafica(List<SpeciesSnapshot> historial)
    {
        if (historial == null || historial.Count == 0) return;

        int puntosADibujar = Mathf.Min(historial.Count, maxPuntosVisible);
        lineaGrafica.positionCount = puntosADibujar;
        int indiceInicio = historial.Count - puntosADibujar;

        float valorActual = ObtenerValorDeSnapshot(historial[historial.Count - 1]);

        float rango = Mathf.Max(valorActual * 0.5f, 0.5f);

        if (textoActual != null) textoActual.text = valorActual.ToString("F2");
        if (textoMax != null) textoMax.text = (valorActual + rango).ToString("F2");
        if (textoMin != null) textoMin.text = (valorActual - rango).ToString("F2");

        for (int i = 0; i < puntosADibujar; i++)
        {
            float xPos = i * separacionX;
            float valorHistorico = ObtenerValorDeSnapshot(historial[indiceInicio + i]);

            float yPos = ((valorHistorico - valorActual) * multiplicadorY) + offsetY;
            lineaGrafica.SetPosition(i, new Vector3(xPos, yPos, 0));
        }
    }

    // 2. EL "TRADUCTOR": Convierte la opción del menú en el dato real
    private float ObtenerValorDeSnapshot(SpeciesSnapshot snap)
    {
        switch (estadisticaAsignada)
        {
            case TipoEstadistica.Velocidad: return snap.avgSpeed;
            case TipoEstadistica.Vision: return snap.avgVision;
            case TipoEstadistica.Tamaño: return snap.avgSize;
            // Si mañana añades fuerza, solo pones aquí: case TipoEstadistica.Fuerza: return snap.avgFuerza;
            default: return 0f;
        }
    }
    public void Limpiar()
    {
        if (lineaGrafica != null)
        {
            lineaGrafica.positionCount = 0;
        }

        // Opcional: Poner los textos a "0" o "-"
        if (textoActual != null) textoActual.text = "-";
        if (textoMax != null) textoMax.text = "-";
        if (textoMin != null) textoMin.text = "-";
    }

}