using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public struct EspeciesSnapshot
{
    public float avgVel;
    public float avgVision;
    public float avgTamano;
    public float avgEnergia;
    public float avgConsumo;
    public float avgVidaUtil;

}
public struct Acumulador
{
    public float sumVel;
    public float sumVision;
    public float sumTamano;
    public float sumEnergia;
    public float sumConsumo;
    public float sumVidaUtil;
    public int count;
    public void Reset() { sumVel = sumVision = sumTamano =  sumEnergia = sumConsumo = sumVidaUtil = 0; count = 0; }
}
[System.Serializable]
public struct RangoEstadisticoEspecie
    {
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


public class EvolutionTracker : MonoBehaviour
{
    public int familiaSeleccionada = 1; // Por defecto vemos la 1

    private Dictionary<int, List<EspeciesSnapshot>> historialEspecies = new Dictionary<int, List<EspeciesSnapshot>>();
    private Dictionary<int, RangoEstadisticoEspecie> rangosEspecies = new Dictionary<int, RangoEstadisticoEspecie>();   
    public List<GraficaIndividual> todasLasGraficas = new List<GraficaIndividual>();
    [SerializeField] private float intervaloMuestreo = 2f;
    private float timer;

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

    void PerformSnapshot()
    {
        for (int i = 0; i < acumulador.Length; i++) acumulador[i].Reset();

        // Accedemos al RegistroVida que ya tienes en GestorLinajes
        foreach (var par in GestorLinajes.RegistroVida)
        {
            // Usamos el id de linaje para saber en qué cajón sumar
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

        // Nos aseguramos de que la lista de gráficas esté inicializada
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