using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct EspeciesSnapshot
{
    // Promedios de cada estadística para una especie en un momento de muestreo
    public float avgVel;
    public float avgVision;
    public float avgTamano;
    public float avgEnergia;
    public float avgConsumo;
    public float avgVidaUtil;

}
public struct Acumulador
{
    // Suma de valores y cuenta de elementos antes de calcular promedios
    public float sumVel;
    public float sumVision;
    public float sumTamano;
    public float sumEnergia;
    public float sumConsumo;
    public float sumVidaUtil;
    public int count;
    public void Reset() { sumVel = sumVision = sumTamano = sumEnergia = sumConsumo = sumVidaUtil = 0; count = 0; }
}

[System.Serializable]
public struct RangoEstadisticoEspecie
{
    // Valores mínimos y máximos registrados para una especie
    public float minVel;
    public float maxVel;
    public float minVision;
    public float maxVision;
    public float minTamano;
    public float maxTamano;
    public float minEnergia;
    public float maxEnergia;
    public float minConsumo;
    public float maxConsumo;
    public float minVidaUtil;
    public float maxVidaUtil;
}

/* 
- Se encarga de recolectar datos de las bacterias a lo largo del tiempo y organizarlos por especie.
- Cada cierto intervalo de tiempo, recorre el registro de vida de las bacterias, acum
ula sus estadísticas en un buffer temporal, y luego calcula promedios para cada especie.
- Guarda estos promedios en un historial por especie, y también actualiza los rangos
estadísticos (mínimos y máximos) para cada especie.
- Cuando se selecciona una especie para mostrar, notifica a todas las gráficas individuales para
que actualicen su visualización con los datos de esa especie.
*/

public class EvolutionTracker : MonoBehaviour
{
    public int familiaSeleccionada = 1; // Especie seleccionada para mostrar en las gráficas

    private Dictionary<int, List<EspeciesSnapshot>> historialEspecies = new Dictionary<int, List<EspeciesSnapshot>>();
    private Dictionary<int, RangoEstadisticoEspecie> rangosEspecies = new Dictionary<int, RangoEstadisticoEspecie>();
    public List<GraficaIndividual> todasLasGraficas = new List<GraficaIndividual>();
    [SerializeField] private float intervaloMuestreo = 2f;
    private float timer;

    // Buffer fijo para agrupar datos antes de guardarlos en el historial
    private Acumulador[] acumulador = new Acumulador[100];

    public Dictionary<int, List<EspeciesSnapshot>> HistorialEspecies { get => historialEspecies; }
    public Dictionary<int, RangoEstadisticoEspecie> RangosEspecies { get => rangosEspecies; }


    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= intervaloMuestreo)
        {
            PerformSnapshot();
            timer = 0;
        }
    }

    // Recolecta datos actuales de las especies y los convierte en snapshots
    void PerformSnapshot()
    {
        for (int i = 0; i < acumulador.Length; i++) acumulador[i].Reset();

        // Recorremos cada registro de vida y sumamos estadísticas en un acumulador
        foreach (var par in GestorLinajes.RegistroVida)
        {
            // Convertimos el id de linaje a un índice válido dentro del arreglo
            int id = par.Value.misStats.idLinaje % acumulador.Length;

            acumulador[id].sumVel += par.Value.misStats.velocidad;
            acumulador[id].sumVision += par.Value.misStats.radioVision;
            acumulador[id].sumTamano += par.Value.misStats.tamano;
            acumulador[id].sumEnergia += par.Value.misStats.energiaMax;
            acumulador[id].sumConsumo += par.Value.misStats.consumo;
            acumulador[id].sumVidaUtil += par.Value.misStats.vidaUtil;
            acumulador[id].count++;
        }

        for (int i = 0; i < acumulador.Length; i++)
        {
            if (acumulador[i].count > 0)
            {
                GuardarEnHistorial(i, acumulador[i]);
            }
        }
    }

    // Calcula el snapshot final y actualiza los datos históricos y los rangos
    void GuardarEnHistorial(int speciesId, Acumulador acc)
    {
        if (!historialEspecies.ContainsKey(speciesId))
            historialEspecies.Add(speciesId, new List<EspeciesSnapshot>());

        if (!rangosEspecies.ContainsKey(speciesId))
            rangosEspecies.Add(speciesId, new RangoEstadisticoEspecie
            {
                minVel = float.MaxValue, maxVel = float.MinValue,
                minVision = float.MaxValue, maxVision = float.MinValue,
                minTamano = float.MaxValue, maxTamano = float.MinValue,
                minEnergia = float.MaxValue, maxEnergia = float.MinValue,
                minConsumo = float.MaxValue, maxConsumo = float.MinValue,
                minVidaUtil = float.MaxValue, maxVidaUtil = float.MinValue
            });

        EspeciesSnapshot nuevoSnapshot = new EspeciesSnapshot
        {
            avgVel = acc.sumVel / acc.count,
            avgVision = acc.sumVision / acc.count,
            avgTamano = acc.sumTamano / acc.count,
            avgEnergia = acc.sumEnergia / acc.count,
            avgConsumo = acc.sumConsumo / acc.count,
            avgVidaUtil = acc.sumVidaUtil / acc.count

        };

        historialEspecies[speciesId].Add(nuevoSnapshot);
        RangoEstadisticoEspecie rangoActual = rangosEspecies[speciesId];
        RangoEstadisticoEspecie nuevoRango = new RangoEstadisticoEspecie
        {
            minVel = Mathf.Min(rangoActual.minVel, nuevoSnapshot.avgVel),
            maxVel = Mathf.Max(rangoActual.maxVel, nuevoSnapshot.avgVel),
            minVision = Mathf.Min(rangoActual.minVision, nuevoSnapshot.avgVision),
            maxVision = Mathf.Max(rangoActual.maxVision, nuevoSnapshot.avgVision),
            minTamano = Mathf.Min(rangoActual.minTamano, nuevoSnapshot.avgTamano),
            maxTamano = Mathf.Max(rangoActual.maxTamano, nuevoSnapshot.avgTamano),
            minEnergia = Mathf.Min(rangoActual.minEnergia, nuevoSnapshot.avgEnergia),
            maxEnergia = Mathf.Max(rangoActual.maxEnergia, nuevoSnapshot.avgEnergia),
            minConsumo = Mathf.Min(rangoActual.minConsumo, nuevoSnapshot.avgConsumo),
            maxConsumo = Mathf.Max(rangoActual.maxConsumo, nuevoSnapshot.avgConsumo),
            minVidaUtil = Mathf.Min(rangoActual.minVidaUtil, nuevoSnapshot.avgVidaUtil),
            maxVidaUtil = Mathf.Max(rangoActual.maxVidaUtil, nuevoSnapshot.avgVidaUtil)
        };

        rangosEspecies[speciesId] = nuevoRango;

        if (speciesId == familiaSeleccionada && todasLasGraficas != null)
        {
            // Avisamos a todos los paneles a la vez
            foreach (GraficaIndividual panel in todasLasGraficas)
            {
                panel.ActualizarGrafica(historialEspecies[speciesId], rangosEspecies[speciesId]);
            }
        }
    }
    public void CambiarFamiliaSeleccionada(int nuevoId)
    {
        familiaSeleccionada = nuevoId;

        // Nos aseguramos de que la lista de graficas este inicializada
        if (todasLasGraficas != null)
        {
            // Si ya tenemos datos de esa familia, actualizamos todas las gráficas al instante
            if (historialEspecies.ContainsKey(nuevoId))
            {
                foreach (GraficaIndividual panel in todasLasGraficas)
                {
                    panel.ActualizarGrafica(historialEspecies[nuevoId], rangosEspecies[nuevoId]);
                }
            }
            else
            {
                // Si es una familia nueva y aún no hay datos, limpiamos las gráficas por si había otra seleccionada antes
                foreach (GraficaIndividual panel in todasLasGraficas)
                {
                    panel.Limpiar();
                }
            }
        }
    }

}