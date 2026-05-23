using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/*
- Controla el boton de reset en la UI de graficas, devolviendo cada grafica a su posicion original dentro del contenedor
- Para esto, busca todas las graficas activas en la escena, las ordena por
el valor de la estadistica que representan (que coincide con el orden original) y las recoloca en el contenedor en ese orden
*/

public class ControladorResetUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto que tiene el Vertical Layout Group (Area_De_Graficas)")]
    public Transform contenedorOriginal;

    // Esta funcion se llama desde el boton en Unity
    public void ResetearPosiciones()
    {
        // 1. Buscamos TODAS las graficas activas en la escena, esten donde esten (incluso las que el usuario haya arrastrado fuera del contenedor)
        GraficaIndividual[] todasLasGraficas = FindObjectsByType<GraficaIndividual>(FindObjectsSortMode.None);

        // 2. Las convertimos a una lista para poder ordenarlas
        List<GraficaIndividual> listaOrdenada = new List<GraficaIndividual>(todasLasGraficas);

        // 3. La ordenamos por el valor de la estadistica que representan, para que vuelvan a su orden original (que es el orden de las estadisticas en el enum)
        listaOrdenada.Sort((a, b) => a.estadisticaAsignada.CompareTo(b.estadisticaAsignada));

        // 4. Las recolocamos en su sitio
        foreach (GraficaIndividual grafica in listaOrdenada)
        {
            // A. Devolver al padre original
            grafica.transform.SetParent(contenedorOriginal);

            // B. Resetear escalas por si acaso
            grafica.transform.localScale = Vector3.one;

            // C. Asegurar que se ven por si acaso
            grafica.gameObject.SetActive(true);
            
            // D. Asegurar que estan al frente del contenedor
            grafica.transform.SetAsLastSibling();
        }

        // 5. Forzamos al Layout Group a que recalcule las posiciones inmediatamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorOriginal as RectTransform);
        
        Debug.Log(" Interfaz Reseteada al orden original");
    }
}