using System;
using System.Collections.Generic;
using UnityEngine;


/*
- Crea una grafica individual por cada estadistica en el enum, al inicio de la simulacion
- Las instancia a partir de un prefab que ya tiene el script de GraficaIndividual, y
le asigna la estadistica correspondiente a cada una
- Las mete a todas en el EvolutionTracker para que este pueda actualizar sus valores al nacer cada nueva bacteria
*/

public class CrearGraficas : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab del Cajon desde la carpeta")]
    public GameObject prefabCajonGrafica;
    [Tooltip("El objeto que tiene el Vertical Layout Group")]
    public Transform contenedorPadre;
    [Tooltip("Tracker central para conectarle las graficas al nacer")]
    public EvolutionTracker evolutionTracker;

    void Start()
    {
        // Limpiamos la lista del Tracker por si acaso
        evolutionTracker.todasLasGraficas = new List<GraficaIndividual>();

        // Por cada estadistica en el enum, creamos una grafica
        foreach (GraficaIndividual.TipoEstadistica tipo in Enum.GetValues(typeof(GraficaIndividual.TipoEstadistica)))
        {
            // 1. Clonamos el Prefab dentro del contenedor
            GameObject nuevoCajon = Instantiate(prefabCajonGrafica, contenedorPadre);
            nuevoCajon.transform.localScale = Vector3.one;

            // 2. Le asignamos la estadistica
            GraficaIndividual scriptGrafica = nuevoCajon.GetComponent<GraficaIndividual>();
            scriptGrafica.estadisticaAsignada = tipo;

            // 3. Le ponemos un nombre bonito en la jerarqu�a
            nuevoCajon.name = "Panel_" + tipo.ToString();

            // 4. Lo metemos al Tracker
            evolutionTracker.todasLasGraficas.Add(scriptGrafica);
        }
    }
}
