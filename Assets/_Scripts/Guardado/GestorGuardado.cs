using UnityEngine;
using System.IO;
using System.Collections.Generic; 

public class GestorGuardado : MonoBehaviour
{
    // Paso previo: obtener la instancia de EvolutionTracker
    EvolutionTracker EvolutionTrackerInstance => FindObjectOfType<EvolutionTracker>();

    public void GuardarPartida()
    {
        // 1. Pausa temporal
        Time.timeScale = 0f;

        // 2. Crear la maleta
        SaveData data = new SaveData();

        // 3. Llenar GestorLinajes (Tu lógica aquí)
        data.proximoIdLinaje = GestorLinajes.Instance.SiguienteIdDisponible;

        // 4. Llenar Bacterias Vivas (Tu lógica aquí con RegistroVida)
        foreach (var kvp in GestorLinajes.RegistroVida)
        {
            DatosEntidad entidad = new DatosEntidad(kvp.Value, kvp.Value.transform.position);
            data.bacteriasVivas.Add(entidad);
        }
        ;
        // 5. Llenar Historial del Tracker (Acuérdate de instanciar DatosContenedorEspecie)
        EvolutionTracker evolutionTracker = EvolutionTrackerInstance; 
        foreach (var par in evolutionTracker.HistorialEspecies)
        {
            int id = par.Key;
            List<EspeciesSnapshot> historial = par.Value;
            RangoEstadisticoEspecie rango = evolutionTracker.RangosEspecies.ContainsKey(id) ? evolutionTracker.RangosEspecies[id] : new RangoEstadisticoEspecie();
            DatosContenedorEspecie contenedor = new DatosContenedorEspecie(id, historial, rango);
            data.historialEspecies.Add(contenedor);
        }



        // 6. Llenar Comida (Usando FindObjectsByType)
        Comida[] comidasEnMapa = FindObjectsByType<Comida>(FindObjectsSortMode.None);
        foreach (Comida comida in comidasEnMapa)
        {
            data.posicionesComida.Add(comida.transform.position);
        }
        // --- MAGIA DEL GUARDADO ---
        // Convertimos a texto y guardamos (¡De esto me encargo yo, tú haz lo de arriba!)
        string json = JsonUtility.ToJson(data, true);
        string ruta = Path.Combine(Application.persistentDataPath, "PartidaTFG.json");
        File.WriteAllText(ruta, json);

        Debug.Log("Partida Guardada con éxito en: " + ruta);

        // Despausar (o dejarlo pausado para que el usuario elija)
        Time.timeScale = 1f;
    }
}