using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class GestorGuardado : MonoBehaviour
{
    [Header("PANELES DE DIÁLOGO (NUEVO)")]
    public GameObject ventanaConfirmacion;
    public TMP_Text textoConfirmacion; // Texto que cambiaremos dinámicamente
    public GameObject ventanaNuevoGuardado;
    public TMP_InputField inputNombrePartida; // El cuadro de texto para el nombre
    public GameObject ventanaExito;

    // Variables internas para "recordar" qué archivo seleccionó el jugador en la ficha
    private string archivoPendienteAccion = "";
    private enum TipoAccion { Cargar, SobrescribirFicha }
    private TipoAccion accionActual;
    public string nombrePartidaActual = "";
    EvolutionTracker EvolutionTrackerInstance => FindFirstObjectByType<EvolutionTracker>();
    [Header("Referencias Extra para Foto")]
    public Canvas canvasPrincipal;
    public GameObject[] objetosAOcultarEnFoto;
    [Header("Referencias UI")]
    public GameObject menuSeccionPartidas;
    public MenuSeleccionPartidas menuSeleccionPartidas;

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
    void Awake()
    {
        // Apagamos todas las ventanas por código al arrancar el juego
        // por si se nos olvidó apagarlas en el editor.
        if (ventanaConfirmacion != null) ventanaConfirmacion.SetActive(false);
        if (ventanaNuevoGuardado != null) ventanaNuevoGuardado.SetActive(false);
        if (ventanaExito != null) ventanaExito.SetActive(false);
    }
    void Update()
    {
        if (ControladorMenuPausa.juegoPausado) return;
        // Usamos 'unscaledDeltaTime' para que cuente segundos reales.
        // Si usáramos 'deltaTime' normal, al poner el juego a x5, el tiempo sumaría el quíntuple.
        tiempoJugadoTotal += Time.unscaledDeltaTime;
    }
    public void GuardarPartida(string nombreArchivo)
    {
        // 1. Pausa temporal
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        // 2. Crear la maleta
        SaveData data = new SaveData();

        // 3. Llenar GestorLinajes
        data.proximoIdLinaje = GestorLinajes.Instance.SiguienteIdDisponible;

        // Guardar las plantillas del laboratorio (Diccionario -> Lista JSON)
        if (GestorLinajes.Instance != null)
        {
            foreach (var kvp in GestorLinajes.Instance.plantillasLinajes)
            {
                int id = kvp.Key;
                DatosGeneticos stats = kvp.Value;
                // Buscamos su nombre personalizado asignado, si no tiene usará el genérico
                string nombreCustom = GestorLinajes.Instance.nombresLinajes.ContainsKey(id)
                    ? GestorLinajes.Instance.nombresLinajes[id]
                    : GestorLinajes.Instance.GetNombrePorId(id);

                DatosPlantillaEspecie plantillaSave = new DatosPlantillaEspecie(id, stats, nombreCustom);
                data.plantillasLaboratorio.Add(plantillaSave);
            }
        }

        // 4. Llenar Bacterias Vivas (RegistroVida)
        foreach (var kvp in GestorLinajes.RegistroVida)
        {
            if (kvp.Value != null)
            {
                DatosEntidad entidad = new DatosEntidad(kvp.Value, kvp.Value.transform.position, kvp.Value.transform.eulerAngles.z);
                data.bacteriasVivas.Add(entidad);
            }
        }

        // 5. Llenar Historial del Tracker
        EvolutionTracker evolutionTracker = EvolutionTrackerInstance;
        if (evolutionTracker != null)
        {
            foreach (var par in evolutionTracker.HistorialEspecies)
            {
                int id = par.Key;
                List<EspeciesSnapshot> historial = par.Value;
                RangoEstadisticoEspecie rango = evolutionTracker.RangosEspecies.ContainsKey(id) ? evolutionTracker.RangosEspecies[id] : new RangoEstadisticoEspecie();
                DatosContenedorEspecie contenedor = new DatosContenedorEspecie(id, historial, rango);
                data.historialEspecies.Add(contenedor);
            }
        }

        // 6. Llenar Comida
        Comida[] comidasEnMapa = FindObjectsByType<Comida>(FindObjectsSortMode.None);
        foreach (Comida comida in comidasEnMapa)
        {
            DatosComidas datosComida = new DatosComidas(comida.transform.position.x, comida.transform.position.y, comida.transform.localScale.z);
            data.datosComidas.Add(datosComida);
        }

        string json = JsonUtility.ToJson(data, true);
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");
        File.WriteAllText(ruta, json);

        Debug.Log("Partida Guardada con éxito en: " + ruta);

        Time.timeScale = velocidadJuego;

        // RUTAS METADATOS Y FOTO
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombreArchivo + ".png");

        MetadatosPartida meta = new MetadatosPartida();
        meta.nombrePartida = nombrePartidaActual;
        meta.totalBacterias = GestorLinajes.RegistroVida.Count;

        // Contamos biodiversidad basándonos en los linajes vivos actuales
        meta.totalLinajesRestantes = GestorLinajes.RegistroVida.Values
            .Where(b => b != null)
            .Select(b => b.misStats.idLinaje)
            .Distinct()
            .Count();

        meta.horasJugadas = tiempoJugadoTotal;
        meta.fechaCreacion = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        File.WriteAllText(rutaMeta, JsonUtility.ToJson(meta));
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

        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        string json = File.ReadAllText(ruta);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 3. Purgar escena para evitar duplicados
        GestorLinajes.Instance?.Purga();
        PoolComida.Instance?.Purga();

        // 4. LinajeSiguiente
        GestorLinajes.Instance.SiguienteIdDisponible = data.proximoIdLinaje;

        // Reconstruir las plantillas del laboratorio (Lista JSON -> Diccionario)
        if (GestorLinajes.Instance != null)
        {
            GestorLinajes.Instance.plantillasLinajes.Clear();
            GestorLinajes.Instance.nombresLinajes.Clear();

            foreach (var plantillaData in data.plantillasLaboratorio)
            {
                GestorLinajes.Instance.plantillasLinajes[plantillaData.idLinaje] = plantillaData.stats;
                GestorLinajes.Instance.nombresLinajes[plantillaData.idLinaje] = plantillaData.nombre;
            }
        }

        // 5. Historiales del Tracker
        EvolutionTracker tracker = EvolutionTrackerInstance;
        if (tracker != null)
        {
            tracker.HistorialEspecies.Clear();
            tracker.RangosEspecies.Clear();

            foreach (var contenedor in data.historialEspecies)
            {
                tracker.HistorialEspecies[contenedor.idLinaje] = contenedor.historial;
                tracker.RangosEspecies[contenedor.idLinaje] = contenedor.rango;
            }
        }

        // 6. Regenerar Comida
        foreach (var comida in data.datosComidas)
        {
            Vector2 posicion = new Vector2(comida.posX, comida.posY);
            PoolComida.Instance.GetComida(posicion, comida.tamano);
        }

        // 7. Regenerar Bacterias
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

        // Forzar el redibujado de la bandeja inferior para que reaparezcan las cartas cargadas
        ControladorInteraccion controlador = FindFirstObjectByType<ControladorInteraccion>();
        if (controlador != null && controlador.panelSelectorEspecies != null)
        {
            BandejaEspeciesUI bandeja = controlador.panelSelectorEspecies.GetComponent<BandejaEspeciesUI>();
            if (bandeja != null)
            {
                bandeja.RedibujarBandeja();
            }
        }

        // 8. Tiempo de juego
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        if (File.Exists(rutaMeta))
        {
            string jsonMeta = File.ReadAllText(rutaMeta);
            MetadatosPartida meta = JsonUtility.FromJson<MetadatosPartida>(jsonMeta);
            tiempoJugadoTotal = meta.horasJugadas;
            Debug.Log("Tiempo restaurado: " + tiempoJugadoTotal + " segundos.");
        }
        else
        {
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
    private System.Collections.IEnumerator TomarFotoSinUI(string ruta)
    {
        canvasPrincipal.enabled = false;

        bool[] estadosPrevios = null;

        if (objetosAOcultarEnFoto != null)
        {
            // Creamos la libreta con tantas páginas como objetos haya
            estadosPrevios = new bool[objetosAOcultarEnFoto.Length];

            for (int i = 0; i < objetosAOcultarEnFoto.Length; i++)
            {
                if (objetosAOcultarEnFoto[i] != null)
                {
                    // Apuntamos si estaba encendido (true) o apagado (false)
                    estadosPrevios[i] = objetosAOcultarEnFoto[i].activeSelf;

                    // Ahora sí, lo apagamos por la fuerza para la captura
                    objetosAOcultarEnFoto[i].SetActive(false);
                }
            }
        }

        yield return new WaitForEndOfFrame();

        Texture2D textura = ScreenCapture.CaptureScreenshotAsTexture();
        canvasPrincipal.enabled = true;

        if (objetosAOcultarEnFoto != null && estadosPrevios != null)
        {
            for (int i = 0; i < objetosAOcultarEnFoto.Length; i++)
            {
                if (objetosAOcultarEnFoto[i] != null)
                {
                    // En lugar de poner 'true' a lo bruto, le pasamos lo que apuntamos en la libreta
                    objetosAOcultarEnFoto[i].SetActive(estadosPrevios[i]);
                }
            }
        }

        byte[] bytes = textura.EncodeToPNG();
        File.WriteAllBytes(ruta, bytes);
        Destroy(textura);

        //Le decimos al menú que refresque las fichas
        if (menuSeleccionPartidas != null) menuSeleccionPartidas.RefrescarMenu();

        Debug.Log("Foto guardada y menú ordenado.");
    }

    public void BorrarPartida(string nombreArchivo)
    {
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombreArchivo + ".png");

        if (File.Exists(ruta)) File.Delete(ruta);
        if (File.Exists(rutaMeta)) File.Delete(rutaMeta);
        if (File.Exists(rutaImagen)) File.Delete(rutaImagen);

        Debug.Log("Partida eliminada: " + nombreArchivo);

        if (menuSeleccionPartidas != null) menuSeleccionPartidas.RefrescarMenu();
    }

    public void abrirGestor()
    {
        menuSeccionPartidas.SetActive(true);
    }

    public void SolicitarCargarPartida(string nombreArchivo)
    {
        archivoPendienteAccion = nombreArchivo;
        accionActual = TipoAccion.Cargar;

        textoConfirmacion.text = $"¿Estás seguro de que quieres cargar '{nombreArchivo}'?\nTodo el progreso actual no guardado se perderá.";
        ventanaConfirmacion.SetActive(true);
    }

    // Esto lo llamará el botón GUARDAR de la ficha de partida
    public void SolicitarSobrescribirFicha(string nombreArchivo)
    {
        archivoPendienteAccion = nombreArchivo;
        accionActual = TipoAccion.SobrescribirFicha;

        textoConfirmacion.text = $"¿Estás seguro de que quieres sobrescribir la partida '{nombreArchivo}'?\nLos datos viejos se borrarán.";
        ventanaConfirmacion.SetActive(true);
    }

    // Este método va en el botón "SÍ" de la ventana de confirmación
    public void ConfirmarAccionVentana()
    {
        ventanaConfirmacion.SetActive(false);

        if (accionActual == TipoAccion.Cargar)
        {
            CargarPartida(archivoPendienteAccion); // Tu método viejo de cargar
        }
        else if (accionActual == TipoAccion.SobrescribirFicha)
        {
            GuardarPartida(archivoPendienteAccion); // Tu método viejo de guardar
            MostrarVentanaExito();
        }
    }


    // ==========================================
    // FLUJO 3: BOTÓN GUARDAR DEL MENÚ DE PAUSA GENERAL
    // ==========================================

    // Esto lo llamará el botón "Guardar" grande del menú de pausa
    public void AbrirVentanaNuevoGuardado()
    {
        // Sugerimos el nombre actual por si ya estábamos jugando en una partida
        inputNombrePartida.text = nombrePartidaActual;
        ventanaNuevoGuardado.SetActive(true);
    }

    // Este método va en el botón "Confirmar" de la ventana donde tecleas el nombre
    public void ProcesarNuevoGuardado()
    {
        string nombreIntroducido = inputNombrePartida.text.Trim();

        // Evitamos que guarden con un nombre vacío
        if (string.IsNullOrEmpty(nombreIntroducido)) return;

        ventanaNuevoGuardado.SetActive(false);

        // Si el nombre tecleado es exactamente el mismo de la partida en curso...
        if (nombreIntroducido == nombrePartidaActual)
        {
            // Guardamos directamente sin preguntar ni molestar al jugador
            GuardarPartida(nombreIntroducido);
            MostrarVentanaExito();
            return; // Cortamos la función aquí para que no siga leyendo hacia abajo
        }

        // COMPROBACIÓN CLAVE: Si es un nombre diferente... ¿Existe ya ese archivo?
        string ruta = Path.Combine(Application.persistentDataPath, nombreIntroducido + ".json");

        if (File.Exists(ruta))
        {
            // Si el archivo existe (y sabemos que NO es nuestra partida actual), pedimos confirmación
            SolicitarSobrescribirFicha(nombreIntroducido);
        }
        else
        {
            // Si no existe, es un ecosistema 100% nuevo
            nombrePartidaActual = nombreIntroducido;
            GuardarPartida(nombreIntroducido);
            MostrarVentanaExito();
        }

        nombrePartidaActual = nombreIntroducido;
    }


    // ==========================================
    // VENTANA DE ÉXITO
    // ==========================================
    private void MostrarVentanaExito()
    {
        ventanaExito.SetActive(true);
    }
}
