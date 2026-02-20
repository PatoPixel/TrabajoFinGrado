using UnityEngine;
using System.Collections.Generic;

public struct SpeciesSnapshot
{
    public float avgSpeed;
    public float avgVision;
    public float avgSize;
}
public struct Accumulator
{
    public float sumSpeed;
    public float sumVision;
    public float sumSize;
    public int count;
    public void Reset() { sumSpeed = sumVision = sumSize = 0; count = 0; }
}
public class EvolutionTracker : MonoBehaviour
{
    public int familiaSeleccionada = 1; // Por defecto vemos la 1
    private Dictionary<int, List<SpeciesSnapshot>> historialEspecies = new Dictionary<int, List<SpeciesSnapshot>>();

    [SerializeField] private GraficaEvolucion graficaUI;
    [SerializeField] private float intervaloMuestreo = 2f;
    private float timer;

    // Array fijo de acumuladores para evitar crear memoria nueva (Senior style)
    private Accumulator[] batchAccumulators = new Accumulator[100];

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= intervaloMuestreo)
        {
            PerformSnapshot();
            timer = 0;
        }
    }

    void PerformSnapshot()
    {
        for (int i = 0; i < batchAccumulators.Length; i++) batchAccumulators[i].Reset();

        // Accedemos al RegistroVida que ya tienes en GestorLinajes
        foreach (var par in GestorLinajes.RegistroVida)
        {
            // Usamos el id de linaje para saber en qué cajón sumar
            int id = par.Value.misStats.idLinaje % batchAccumulators.Length;

            batchAccumulators[id].sumSpeed += par.Value.misStats.velocidad;
            batchAccumulators[id].sumVision += par.Value.misStats.radioVision;
            batchAccumulators[id].sumSize += par.Value.misStats.tamano;
            batchAccumulators[id].count++;
        }

        for (int i = 0; i < batchAccumulators.Length; i++)
        {
            if (batchAccumulators[i].count > 0)
            {
                GuardarEnHistorial(i, batchAccumulators[i]);
            }
        }
    }

    void GuardarEnHistorial(int speciesId, Accumulator acc)
    {
        if (!historialEspecies.ContainsKey(speciesId))
            historialEspecies.Add(speciesId, new List<SpeciesSnapshot>());

        SpeciesSnapshot nuevoSnapshot = new SpeciesSnapshot
        {
            avgSpeed = acc.sumSpeed / acc.count,
            avgVision = acc.sumVision / acc.count,
            avgSize = acc.sumSize / acc.count
        };

        historialEspecies[speciesId].Add(nuevoSnapshot);

        if (speciesId == familiaSeleccionada && graficaUI != null)
        {
            graficaUI.ActualizarGrafica(historialEspecies[speciesId]);
        }
    }
    public void CambiarFamiliaSeleccionada(int nuevoId)
    {
        familiaSeleccionada = nuevoId;

        // Si ya tenemos datos de esa familia en el historial, actualizamos la gráfica al instante
        if (historialEspecies.ContainsKey(nuevoId) && graficaUI != null)
        {
            graficaUI.ActualizarGrafica(historialEspecies[nuevoId]);
        }
    }
}