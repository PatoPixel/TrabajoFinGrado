using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Linq;

public class GestorGuardado : MonoBehaviour
{
    public string nombrePartidaActual = "";
    EvolutionTracker EvolutionTrackerInstance => FindFirstObjectByType<EvolutionTracker>();
    [Header("Referencias UI")]
    public Canvas canvasPrincipal;
    public GameObject menuSeccionPartidas;

    [System.Serializable]
    public class MetadatosPartida
    {
        public string nombrePartida;
        public int totalBacterias;
        public int totalLinajesRestantes;
        public float horasJugadas;
        public string fechaCreacion;
    }

    public float tiempoJugadoTotal = 0f;

    void Update()
    {
        if (ControladorMenuPausa.juegoPausado) return;
        // Usamos 'unscaledDeltaTime' para que cuente segundos reales.
        // Si usáramos 'deltaTime' normal, al poner el juego a x5, el tiempo sumaría el quíntuple.
        tiempoJugadoTotal += Time.unscaledDeltaTime;
    }
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

        string json = JsonUtility.ToJson(data, true);
        string ruta = Path.Combine(Application.persistentDataPath, nombrePartidaActual + ".json");
        File.WriteAllText(ruta, json);

        Debug.Log("Partida Guardada con éxito en: " + ruta);

        // Despausar (o dejarlo pausado para que el usuario elija)
        Time.timeScale = velocidadJuego;

        // 1. RUTAS NUEVAS
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombrePartidaActual + "_meta.json");
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombrePartidaActual + ".png");
        MetadatosPartida meta = new MetadatosPartida();
        meta.nombrePartida = nombrePartidaActual;
        meta.totalBacterias = GestorLinajes.RegistroVida.Count;
        meta.totalLinajesRestantes = GestorLinajes.RegistroVida.Values.Select(b => b.misStats.idLinaje).Distinct().Count();
        meta.horasJugadas = tiempoJugadoTotal;

        // Si la partida es nueva, le ponemos la fecha de hoy. Si la estamos sobrescribiendo, 
        // deberíamos mantener la que tenía, pero para simplificar ahora, pondremos la de hoy.
        meta.fechaCreacion = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Guardamos el JSON pequeñito
        File.WriteAllText(rutaMeta, JsonUtility.ToJson(meta));

        // 3. CAPTURA DE PANTALLA (Magia de Unity de 1 sola línea)
        StartCoroutine(TomarFotoSinUI(rutaImagen));

        Debug.Log("Guardado completo: Datos + Metadatos + Foto");
    }
    public void CargarPartida(string nombreArchivo)
    {

        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");

        if (!File.Exists(ruta))
        {
            Debug.LogWarning("No hay partida guardada en: " + ruta);
            return;
        }

        // 1. Pausa
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        // 2. Leer json
        string json = File.ReadAllText(ruta);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 3. Purgar lo que haya en escena para evitar duplicados o conflictos
        GestorLinajes.Instance?.Purga();
        PoolComida.Instance?.Purga(); // O la clase/gestor desde donde llames a esto

        // 4. LinajeSiguiente
        GestorLinajes.Instance.SiguienteIdDisponible = data.proximoIdLinaje;

        // 5. Historiales
        EvolutionTracker tracker = EvolutionTrackerInstance;
        tracker.HistorialEspecies.Clear();
        tracker.RangosEspecies.Clear();

        foreach (var contenedor in data.historialEspecies)
        {
            tracker.HistorialEspecies[contenedor.idLinaje] = contenedor.historial;
            tracker.RangosEspecies[contenedor.idLinaje] = contenedor.rango;
        }


        // 6. Comida

        foreach (var comida in data.datosComidas)
        {
            Vector2 posicion = new Vector2(comida.posX, comida.posY);
            PoolComida.Instance.GetComida(posicion, comida.tamano);
        }


        // 7. Bacterias

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

        // 8. Tiempo de juego
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        if (File.Exists(rutaMeta))
        {
            string jsonMeta = File.ReadAllText(rutaMeta);
            MetadatosPartida meta = JsonUtility.FromJson<MetadatosPartida>(jsonMeta);

            // Le decimos al cronómetro que empiece desde donde lo dejamos
            tiempoJugadoTotal = meta.horasJugadas;
            Debug.Log("Tiempo restaurado: " + tiempoJugadoTotal + " segundos.");
        }
        else
        {
            // Si por algún motivo cargamos una partida vieja que no tenía metadatos, empezamos de cero
            tiempoJugadoTotal = 0f;
        }
        ObtenerPartidasGuardadas();
        nombrePartidaActual = nombreArchivo;
        Debug.Log("Partida Cargada con éxito.");
        Time.timeScale = velocidadJuego;
    }

    public List<string> ObtenerPartidasGuardadas()
    {
        List<string> nombresPartidas = new List<string>();
        string rutaCarpeta = Application.persistentDataPath;

        // Aquí está la magia: Directory.GetFiles busca en la carpeta todos los archivos con la extensión que le digas
        string[] archivos = Directory.GetFiles(rutaCarpeta, "*.json");

        foreach (string archivo in archivos)
        {
            string nombreLimpio = Path.GetFileNameWithoutExtension(archivo);
            DateTime fechaGuardado = File.GetLastWriteTime(archivo);

            // Formateamos la fecha para que se vea bonita en la consola (y en tu UI de mañana)
            string fechaString = fechaGuardado.ToString("dd/MM/yyyy HH:mm");

            // Añadimos la fecha al Debug para comprobarlo hoy
            Debug.Log($"Partida: {nombreLimpio} | Fecha: {fechaString}");

            nombresPartidas.Add(nombreLimpio);
        }

        return nombresPartidas;
    }
    public void BorrarPartida(string nombreArchivo)
    {
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");

        // Siempre comprobamos si existe antes de intentar borrar, o Unity dará un error
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
            Debug.Log("Partida eliminada: " + nombreArchivo);
        }
        else
        {
            Debug.LogWarning("No se encontró el archivo para borrar: " + nombreArchivo);
        }
    }

    private System.Collections.IEnumerator TomarFotoSinUI(string ruta)
    {
        // 1. Apagamos la UI
        canvasPrincipal.enabled = false;

        // 2. Esperamos a que termine el frame actual para que Unity dibuje la pantalla limpia
        yield return new WaitForEndOfFrame();

        // 3. Tomamos la foto como una textura en memoria RAM
        Texture2D textura = ScreenCapture.CaptureScreenshotAsTexture();

        // 4. Volvemos a encender la UI instantáneamente (el jugador no verá parpadeos)
        canvasPrincipal.enabled = true;

        // 5. Convertimos la textura a archivo PNG y la guardamos en el disco duro
        byte[] bytes = textura.EncodeToPNG();
        File.WriteAllBytes(ruta, bytes);

        // Quitamos la textura de la memoria para no llenarla con fotos antiguas
        Destroy(textura);

        Debug.Log("Foto limpia guardada en: " + ruta);
    }

    public void abrirGestor()
    {
        menuSeccionPartidas.SetActive(true);
    }
}
