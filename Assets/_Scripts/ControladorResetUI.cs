using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // ¡IMPORTANTE! Necesario para ordenar listas (Sort)

public class ControladorResetUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto que tiene el Vertical Layout Group (Area_De_Graficas)")]
    public Transform contenedorOriginal;

    // Esta función la llamarás desde el Botón en Unity
    public void ResetearPosiciones()
    {
        // 1. Buscamos TODAS las gráficas activas en la escena, estén donde estén
        GraficaIndividual[] todasLasGraficas = FindObjectsByType<GraficaIndividual>(FindObjectsSortMode.None);

        // 2. Las convertimos a una lista para poder ordenarlas
        List<GraficaIndividual> listaOrdenada = new List<GraficaIndividual>(todasLasGraficas);

        // 3. MAGIA: Las ordenamos por su número de Enum (Velocidad=0, Vision=1...)
        // Esto asegura que siempre vuelvan en el mismo orden perfecto.
        listaOrdenada.Sort((a, b) => a.estadisticaAsignada.CompareTo(b.estadisticaAsignada));

        // 4. Las recolocamos en su sitio
        foreach (GraficaIndividual grafica in listaOrdenada)
        {
            // A. Devolver al padre original (la cárcel del Layout Group)
            grafica.transform.SetParent(contenedorOriginal);

            // B. Resetear escalas raras (por si al arrastrar se deformó algo)
            grafica.transform.localScale = Vector3.one;

            // C. Asegurar que se ve (por si alguna estaba oculta)
            grafica.gameObject.SetActive(true);
            
            // D. Asegurar que está al frente del contenedor
            grafica.transform.SetAsLastSibling();
        }

        // 5. Forzamos al Layout Group a que recalcule las posiciones inmediatamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorOriginal as RectTransform);
        
        Debug.Log(" Interfaz Reseteada al orden original");
    }
}