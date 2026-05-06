using UnityEngine;
using System.IO;
using System.Collections.Generic; 

public class GestorGuardado : MonoBehaviour
{
    // Paso previo: obtener la instancia de EvolutionTracker
    EvolutionTracker EvolutionTrackerInstance => FindFirstObjectByType<EvolutionTracker>();

    public void GuardarPartida()
    {
        // 1. Pausa temporal
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        // 2. Crear la maleta
        SaveData data = new SaveData();

        // 3. Llenar GestorLinajes (Tu lógica aquí)
        data.proximoIdLinaje = GestorLinajes.Instance.SiguienteIdDisponible;

        // 4. Llenar Bacterias Vivas (Tu lógica aquí con RegistroVida)
        foreach (var kvp in GestorLinajes.RegistroVida)
        {
            DatosEntidad entidad = new DatosEntidad(kvp.Value, kvp.Value.transform.position, kvp.Value.transform.eulerAngles.z);
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
            DatosComidas datosComida = new DatosComidas(comida.transform.position.x, comida.transform.position.y, comida.transform.localScale.z);
            data.datosComidas.Add(datosComida);
        }

        // --- MAGIA DEL GUARDADO ---
        // Convertimos a texto y guardamos (¡De esto me encargo yo, tú haz lo de arriba!)
        string json = JsonUtility.ToJson(data, true);
        string ruta = Path.Combine(Application.persistentDataPath, "PartidaTFG.json");
        File.WriteAllText(ruta, json);

        Debug.Log("Partida Guardada con éxito en: " + ruta);

        // Despausar (o dejarlo pausado para que el usuario elija)
        Time.timeScale = velocidadJuego;
    }
    public void CargarPartida()
    {
        string ruta = Path.Combine(Application.persistentDataPath, "PartidaTFG.json");

        if (!File.Exists(ruta))
        {
            Debug.LogWarning("No hay partida guardada en: " + ruta);
            return;
        }

        // 1. Pausa
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        // 2. Leer maleta
        string json = File.ReadAllText(ruta);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 3. LA PURGA (Limpiamos la escena actual)
        GestorLinajes.Instance?.Purga();
        PoolComida.Instance?.Purga(); // O la clase/gestor desde donde llames a esto

        // 4. RESTAURAR LEYES (Gestor de Linajes)
        GestorLinajes.Instance.SiguienteIdDisponible = data.proximoIdLinaje;

        // 5. RESTAURAR HISTORIAL (Evolution Tracker)
        EvolutionTracker tracker = EvolutionTrackerInstance;
        tracker.HistorialEspecies.Clear();
        tracker.RangosEspecies.Clear();
        // TODO: Haz un foreach sobre data.historialEspecies.
        // Saca el idLinaje, el historial y el rango de cada 'DatosContenedorEspecie' 
        // y mételos en los diccionarios del tracker.
        foreach (var contenedor in data.historialEspecies)
        {
            tracker.HistorialEspecies[contenedor.idLinaje] = contenedor.historial;
            tracker.RangosEspecies[contenedor.idLinaje] = contenedor.rango;
        }


        // 6. RESTAURAR HABITANTES (Bacterias y Comida)

        // 6A. Comida: 
        // TODO: Haz un foreach sobre data.posicionesComida. Pide comida al Pool en esa posición.
        foreach (var comida in data.datosComidas)
        {
            Vector2 posicion = new Vector2(comida.posX, comida.posY);
            PoolComida.Instance.GetComida(posicion, comida.tamano);
        }
        // 6B. Bacterias:
        // TODO: Haz un foreach sobre data.bacteriasVivas.
        // 1. Pide una bacteria al pool usando su posX y posY.
        // 2. Consigue su componente SistemaVida.
        // 3. Llámale a AsignarStatsLoad(...) pasándole los datos.

        foreach (var entidad in data.bacteriasVivas)
        {
            Vector3 posicion = new Vector3(entidad.posX, entidad.posY, 0);
            GameObject bacteriaObj = BacteriasMuertas.Instance.GetBacteria(posicion);
            SistemaVida sv = bacteriaObj.GetComponent<SistemaVida>();
            if (sv != null)
            {
                sv.AsignarStatsLoad(entidad);
            }
            else
            {
                Debug.LogError("La bacteria obtenida del pool no tiene SistemaVida.");
            }
        }

        Debug.Log("Partida Cargada con éxito.");
        Time.timeScale = velocidadJuego;
    }
}