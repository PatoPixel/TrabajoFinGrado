using System;
using System.Collections.Generic;
using UnityEngine;

public class CrearGraficas : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí tu Prefab del Cajón desde la carpeta")]
    public GameObject prefabCajonGrafica;
    [Tooltip("El objeto que tiene el Vertical Layout Group")]
    public Transform contenedorPadre;
    [Tooltip("Tu Tracker central para conectarle las gráficas al nacer")]
    public EvolutionTracker evolutionTracker;

    // ¡Hemos borrado la lista manual! Ya no te hace falta.

    void Start()
    {
        // Limpiamos la lista del Tracker por si acaso
        evolutionTracker.todasLasGraficas = new List<GraficaIndividual>();

        // MAGIA SENIOR: Recorremos automáticamente TODAS las opciones que existan en tu Enum
        foreach (GraficaIndividual.TipoEstadistica tipo in Enum.GetValues(typeof(GraficaIndividual.TipoEstadistica)))
        {
            // Opcional: Si dejaste el "OtraNuevaQueInventes" de prueba, nos lo saltamos
            if (tipo.ToString() == "OtraNuevaQueInventes") continue;

            // 1. Clonamos el Prefab dentro del contenedor
            GameObject nuevoCajon = Instantiate(prefabCajonGrafica, contenedorPadre);
            nuevoCajon.transform.localScale = Vector3.one;

            // 2. Le asignamos la estadística
            GraficaIndividual scriptGrafica = nuevoCajon.GetComponent<GraficaIndividual>();
            scriptGrafica.estadisticaAsignada = tipo;

            // 3. Le ponemos un nombre bonito en la jerarquía
            nuevoCajon.name = "Panel_" + tipo.ToString();

            // 4. Lo enchufamos automáticamente al Tracker
            evolutionTracker.todasLasGraficas.Add(scriptGrafica);
        }
    }
}
