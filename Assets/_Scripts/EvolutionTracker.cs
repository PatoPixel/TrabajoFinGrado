using UnityEngine;
using System.Collections.Generic;

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
public class EvolutionTracker : MonoBehaviour
{
    public int familiaSeleccionada = 1; // Por defecto vemos la 1
    private Dictionary<int, List<EspeciesSnapshot>> historialEspecies = new Dictionary<int, List<EspeciesSnapshot>>();

    public List<GraficaIndividual> todasLasGraficas = new List<GraficaIndividual>();
    [SerializeField] private float intervaloMuestreo = 2f;
    private float timer;

    private Acumulador[] acumulador = new Acumulador[100];

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

        if (speciesId == familiaSeleccionada && todasLasGraficas != null)
        {
            // Avisamos a todos los paneles a la vez
            foreach (GraficaIndividual panel in todasLasGraficas)
            {
                panel.ActualizarGrafica(historialEspecies[speciesId]);
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
                    panel.ActualizarGrafica(historialEspecies[nuevoId]);
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