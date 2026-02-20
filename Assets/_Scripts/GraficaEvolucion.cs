using UnityEngine;
using System.Collections.Generic;

public class GraficaEvolucion : MonoBehaviour
{
    [Header("Configuración de Líneas")]
    [SerializeField] private LineRenderer lineVelocidad;
    [SerializeField] private LineRenderer lineVision;
    [SerializeField] private LineRenderer lineTamano;

    [Header("Ajustes Visuales")]
    [SerializeField] private float separacionX = 0.01f; // Cuánto espacio hay entre puntos
    [SerializeField] private int maxPuntosVisible = 6;

    // Este método lo llamarás cuando el usuario haga CLICK en una bacteria
    // o cada vez que el EvolutionTracker genere un nuevo Snapshot
    public void ActualizarGrafica(List<SpeciesSnapshot> historial)
    {
        if (historial == null || historial.Count == 0) return;

        // 1. Ajustamos la cantidad de puntos de todas las líneas
        int puntosADibujar = Mathf.Min(historial.Count, maxPuntosVisible);
        lineVelocidad.positionCount = puntosADibujar;
        lineVision.positionCount = puntosADibujar;
        lineTamano.positionCount = puntosADibujar;
        int indiceInicio = historial.Count - puntosADibujar;

        // Pon esto para ver la magia en la consola:
        Debug.Log($"<color=orange>[Gráfica] Leyendo historial desde el índice {indiceInicio} hasta {historial.Count - 1}</color>");
        // 2. Llenamos los puntos recorriendo el historial
        for (int i = 0; i < puntosADibujar; i++)
        {
            float xPos = i * separacionX;
            SpeciesSnapshot snap = historial[indiceInicio + i];
            // Dibujamos cada stat (puedes sumar un offset en Y para separarlas)
            lineVelocidad.SetPosition(i, new Vector3(xPos, snap.avgSpeed, 0));
            lineVision.SetPosition(i, new Vector3(xPos, snap.avgVision, 0));
            lineTamano.SetPosition(i, new Vector3(xPos, snap.avgSize+1, 0));
            
            }
    }

    // Método para limpiar la gráfica si la especie se extingue o cambias de selección
    public void Limpiar()
    {
        lineVelocidad.positionCount = 0;
        lineVision.positionCount = 0;
        lineTamano.positionCount = 0;
    }
}