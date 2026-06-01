using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class GestorGuardado : MonoBehaviour
{
    [Header("PANELES DE DI�LOGO")]
    public GameObject ventanaConfirmacion;
    public TMP_Text textoConfirmacion;
    public GameObject ventanaNuevoGuardado;
    public TMP_InputField inputNombrePartida;
    public GameObject ventanaExito;

    private string archivoPendienteAccion = "";

    // CAMBIO: A�adido 'NuevaSimulacion' al enum para controlar este nuevo flujo
    private enum TipoAccion { Cargar, SobrescribirFicha, NuevaSimulacion, VolverAlMenu }
    private TipoAccion accionActual;
    public string nombrePartidaActual = "";
    EvolutionTracker EvolutionTrackerInstance => FindFirstObjectByType<EvolutionTracker>();

    [Header("Referencias Extra para Foto")]
    public Canvas canvasPrincipal;
    public GameObject[] objetosAOcultarEnFoto;

    [Header("Referencias UI")]
    public GameObject menuSeccionPartidas;
    public MenuSeleccionPartidas menuSeleccionPartidas;

    [Header("Modo Menú Principal")]
    [Tooltip("Actívalo en la escena MenuPrincipal. Las partidas navegan a GamePlay en vez de cargarse en caliente.")]
    public bool esMenuPrincipal = false;

    [Header("Referencias del Entorno y Spawners")]
    public GameObject prefabSpawnerComida;
    public float radioPlacaActual = 25f;

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
        if (ventanaConfirmacion != null) ventanaConfirmacion.SetActive(false);
        if (ventanaNuevoGuardado != null) ventanaNuevoGuardado.SetActive(false);
        if (ventanaExito != null) ventanaExito.SetActive(false);
    }

    void Start()
    {
        // En GamePlay: si venimos del menú con una partida pendiente, la cargamos
        if (!esMenuPrincipal && !string.IsNullOrEmpty(GestorEscenas.PartidaACargar))
        {
            string nombre = GestorEscenas.PartidaACargar;
            GestorEscenas.LimpiarPartidaACargar();
            CargarPartida(nombre);
        }
    }

    void Update()
    {
        if (ControladorMenuPausa.juegoPausado) return;
        tiempoJugadoTotal += Time.unscaledDeltaTime;
    }

    public void GuardarPartida(string nombreArchivo)
    {
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        SaveData data = new SaveData();
        data.proximoIdLinaje = GestorLinajes.Instance.SiguienteIdDisponible;

        if (GestorEntorno.Instance != null)
        {
            radioPlacaActual = GestorEntorno.Instance.radioPlaca;
        }
        data.radioPlacaPetri = radioPlacaActual;

        if (GestorLinajes.Instance != null)
        {
            foreach (var kvp in GestorLinajes.Instance.plantillasLinajes)
            {
                int id = kvp.Key;
                DatosGeneticos stats = kvp.Value;
                string nombreCustom = GestorLinajes.Instance.nombresLinajes.ContainsKey(id)
                    ? GestorLinajes.Instance.nombresLinajes[id]
                    : GestorLinajes.Instance.GetNombrePorId(id);

                DatosPlantillaEspecie plantillaSave = new DatosPlantillaEspecie(id, stats, nombreCustom);
                data.plantillasLaboratorio.Add(plantillaSave);
            }
        }

        foreach (var kvp in GestorLinajes.RegistroVida)
        {
            if (kvp.Value != null)
            {
                DatosEntidad entidad = new DatosEntidad(kvp.Value, kvp.Value.transform.position, kvp.Value.transform.eulerAngles.z);
                data.bacteriasVivas.Add(entidad);
            }
        }

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

        Comida[] comidasEnMapa = FindObjectsByType<Comida>(FindObjectsSortMode.None);
        foreach (Comida comida in comidasEnMapa)
        {
            DatosComidas datosComida = new DatosComidas(comida.transform.position.x, comida.transform.position.y, comida.transform.localScale.z);
            data.datosComidas.Add(datosComida);
        }

        SpawnerComida[] spawnersEnMapa = FindObjectsByType<SpawnerComida>(FindObjectsSortMode.None);
        foreach (SpawnerComida spawner in spawnersEnMapa)
        {
            DatosSpawner datosSp = new DatosSpawner(
                spawner.transform.position.x,
                spawner.transform.position.y,
                spawner.MinEnergia,
                spawner.MaxEnergia,
                spawner.Intervalo,
                spawner.RadioSpawn
            );
            data.datosSpawners.Add(datosSp);
        }

        string json = JsonUtility.ToJson(data, true);
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");
        File.WriteAllText(ruta, json);

        Time.timeScale = velocidadJuego;

        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombreArchivo + ".png");

        MetadatosPartida meta = new MetadatosPartida();
        meta.nombrePartida = nombrePartidaActual;
        meta.totalBacterias = GestorLinajes.RegistroVida.Count;

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

        GestorLinajes.Instance?.Purga();
        PoolComida.Instance?.Purga();

        GestorLinajes.Instance.SiguienteIdDisponible = data.proximoIdLinaje;

        radioPlacaActual = data.radioPlacaPetri;
        if (GestorEntorno.Instance != null)
        {
            GestorEntorno.Instance.CambiarRadioPlaca(radioPlacaActual);
        }

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

        foreach (var comida in data.datosComidas)
        {
            Vector2 posicion = new Vector2(comida.posX, comida.posY);
            PoolComida.Instance.GetComida(posicion, comida.tamano);
        }

        SpawnerComida[] spawnersViejos = FindObjectsByType<SpawnerComida>(FindObjectsSortMode.None);
        foreach (SpawnerComida sp in spawnersViejos) Destroy(sp.gameObject);

        if (prefabSpawnerComida != null)
        {
            foreach (var spData in data.datosSpawners)
            {
                Vector3 pos = new Vector3(spData.posX, spData.posY, 0);
                GameObject objSp = Instantiate(prefabSpawnerComida, pos, Quaternion.identity);
                if (objSp.TryGetComponent(out SpawnerComida scriptSp))
                {
                    scriptSp.Inicializar(spData.minEnergia, spData.maxEnergia, spData.intervalo, spData.radioSpawn);
                }
            }
        }

        foreach (var entidad in data.bacteriasVivas)
        {
            Vector3 posicion = new Vector3(entidad.posX, entidad.posY, 0);
            GameObject bacteriaObj = BacteriasMuertas.Instance.GetBacteria(posicion);
            SistemaVida sv = bacteriaObj.GetComponent<SistemaVida>();
            if (sv != null)
            {
                sv.AsignarStatsLoad(entidad);
            }
        }

        ControladorInteraccion controlador = FindFirstObjectByType<ControladorInteraccion>();
        if (controlador != null && controlador.panelSelector != null)
        {
            BandejaEspeciesUI bandeja = controlador.panelSelector.GetComponent<BandejaEspeciesUI>();
            if (bandeja != null)
            {
                bandeja.RedibujarBandeja();
            }
        }

        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        if (File.Exists(rutaMeta))
        {
            string jsonMeta = File.ReadAllText(rutaMeta);
            MetadatosPartida meta = JsonUtility.FromJson<MetadatosPartida>(jsonMeta);
            tiempoJugadoTotal = meta.horasJugadas;
        }
        else
        {
            tiempoJugadoTotal = 0f;
        }

        ObtenerPartidasGuardadas();
        nombrePartidaActual = nombreArchivo;
        Time.timeScale = velocidadJuego;
    }

    // ==========================================
    // NUEVO FLUJO: NUEVA SIMULACI�N DESDE EL MEN�
    // ==========================================

    // 1. Este m�todo lo ejecuta el bot�n "Nueva Simulaci�n" del Men� General
    public void SolicitarNuevaSimulacion()
    {
        // Si la partida en curso tiene nombre, preguntamos si quiere sobrescribirla
        if (!string.IsNullOrEmpty(nombrePartidaActual))
        {
            accionActual = TipoAccion.NuevaSimulacion;
            textoConfirmacion.text = $"�Quieres GUARDAR la partida actual '{nombrePartidaActual}' antes de iniciar una nueva simulaci�n?";
            ventanaConfirmacion.SetActive(true);

            // NOTA DE DISE�O: En este flujo espec�fico, el bot�n "S�" de la ventana guardar� y resetear�,
            // y necesitamos un bot�n alternativo o una l�gica para el "NO" que resetee directamente.
            // Para cumplir estrictamente tu regla sin rehacer la ventana entera, si le da a "S�", ejecutar� ConfirmarAccionVentana().
        }
        else
        {
            // Si la partida ni siquiera se ha guardado nunca (est� vac�a), iniciamos de cero directamente sin preguntar
            EjecutarReseteoAbsoluto();
        }
    }

    private void EjecutarReseteoAbsoluto()
    {
        float velocidadJuego = Time.timeScale;
        Time.timeScale = 0f;

        // Limpieza radical de entidades mediante las purgas que ya programaste
        GestorLinajes.Instance?.Purga();
        PoolComida.Instance?.Purga();

        // Destrucci�n f�sica de todos los Spawners que est�n en la placa Petri
        SpawnerComida[] spawnersViejos = FindObjectsByType<SpawnerComida>(FindObjectsSortMode.None);
        foreach (SpawnerComida sp in spawnersViejos) Destroy(sp.gameObject);

        // Reseteo total de linajes, tiempos y variables de estado del mundo
        if (GestorLinajes.Instance != null)
        {
            GestorLinajes.Instance.SiguienteIdDisponible = 1;
            GestorLinajes.Instance.plantillasLinajes.Clear();
            GestorLinajes.Instance.nombresLinajes.Clear();
        }

        EvolutionTracker tracker = EvolutionTrackerInstance;
        if (tracker != null)
        {
            tracker.HistorialEspecies.Clear();
            tracker.RangosEspecies.Clear();
        }

        radioPlacaActual = 200f;
        if (GestorEntorno.Instance != null)
        {
            GestorEntorno.Instance.CambiarRadioPlaca(radioPlacaActual);
        }

        // Forzar el redibujado de la bandeja inferior para limpiar cartas viejas
        ControladorInteraccion controlador = FindFirstObjectByType<ControladorInteraccion>();
        if (controlador != null && controlador.panelSelector != null)
        {
            BandejaEspeciesUI bandeja = controlador.panelSelector.GetComponent<BandejaEspeciesUI>();
            if (bandeja != null) bandeja.RedibujarBandeja();
        }

        tiempoJugadoTotal = 0f;
        nombrePartidaActual = ""; // Volvemos a dejar la simulaci�n en estado virgen an�nima

        Debug.Log("[Ecosistema] Nueva simulaci�n iniciada con �xito desde 0.");
        Time.timeScale = 1f; // Reanudamos el tiempo normal de juego
    }

    // ==========================================
    // CONFIRMACI�N CENTRALIZADA (MODIFICADA)
    // ==========================================
    public void ConfirmarAccionVentana()
    {
        ventanaConfirmacion.SetActive(false);

        if (accionActual == TipoAccion.Cargar)
        {
            CargarPartida(archivoPendienteAccion);
        }
        else if (accionActual == TipoAccion.SobrescribirFicha)
        {
            GuardarPartida(archivoPendienteAccion);
            MostrarVentanaExito();
        }
        // NUEVO CASO: Si pulsa "S�" en la advertencia de Nueva Simulaci�n
        else if (accionActual == TipoAccion.NuevaSimulacion)
        {
            GuardarPartida(nombrePartidaActual);
            EjecutarReseteoAbsoluto();
        }
        else if (accionActual == TipoAccion.VolverAlMenu)
        {
            GestorEscenas.IrAMenuPrincipal();
        }
    }

    public List<string> ObtenerPartidasGuardadas()
    {
        List<string> nombresPartidas = new List<string>();
        string rutaCarpeta = Application.persistentDataPath;
        string[] archivos = Directory.GetFiles(rutaCarpeta, "*.json");

        foreach (string archivo in archivos)
        {
            string nombreLimpio = Path.GetFileNameWithoutExtension(archivo);
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
            estadosPrevios = new bool[objetosAOcultarEnFoto.Length];
            for (int i = 0; i < objetosAOcultarEnFoto.Length; i++)
            {
                if (objetosAOcultarEnFoto[i] != null)
                {
                    estadosPrevios[i] = objetosAOcultarEnFoto[i].activeSelf;
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
                    objetosAOcultarEnFoto[i].SetActive(estadosPrevios[i]);
                }
            }
        }

        byte[] bytes = textura.EncodeToPNG();
        File.WriteAllBytes(ruta, bytes);
        Destroy(textura);

        if (menuSeleccionPartidas != null) menuSeleccionPartidas.RefrescarMenu();
    }

    public void BorrarPartida(string nombreArchivo)
    {
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo + ".json");
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombreArchivo + "_meta.json");
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombreArchivo + ".png");

        if (File.Exists(ruta)) File.Delete(ruta);
        if (File.Exists(rutaMeta)) File.Delete(rutaMeta);
        if (File.Exists(rutaImagen)) File.Delete(rutaImagen);

        if (menuSeleccionPartidas != null) menuSeleccionPartidas.RefrescarMenu();
    }

    public void abrirGestor()
    {
        menuSeccionPartidas.SetActive(true);
    }

    public void SolicitarVolverAlMenu()
    {
        accionActual = TipoAccion.VolverAlMenu;
        textoConfirmacion.text = "¿Seguro que quieres volver al Menú Principal?\nEl progreso no guardado se perderá.";
        ventanaConfirmacion.SetActive(true);
    }

    public void SolicitarCargarPartida(string nombreArchivo)
    {
        // En el menú principal no hay partida en curso que perder: navegamos directamente
        if (esMenuPrincipal)
        {
            GestorEscenas.IrACargarPartida(nombreArchivo);
            return;
        }

        archivoPendienteAccion = nombreArchivo;
        accionActual = TipoAccion.Cargar;
        textoConfirmacion.text = $"¿Estás seguro de que quieres cargar '{nombreArchivo}'?\nTodo el progreso actual no guardado se perderá.";
        ventanaConfirmacion.SetActive(true);
    }

    public void SolicitarSobrescribirFicha(string nombreArchivo)
    {
        archivoPendienteAccion = nombreArchivo;
        accionActual = TipoAccion.SobrescribirFicha;
        textoConfirmacion.text = $"�Est�s seguro de que quieres sobrescribir la partida '{nombreArchivo}'?\nLos datos viejos se borrar�n.";
        ventanaConfirmacion.SetActive(true);
    }

    public void AbrirVentanaNuevoGuardado()
    {
        inputNombrePartida.text = nombrePartidaActual;
        ventanaNuevoGuardado.SetActive(true);
    }

    public void ProcesarNuevoGuardado()
    {
        string nombreIntroducido = inputNombrePartida.text.Trim();
        if (string.IsNullOrEmpty(nombreIntroducido)) return;
        ventanaNuevoGuardado.SetActive(false);

        if (nombreIntroducido == nombrePartidaActual)
        {
            GuardarPartida(nombreIntroducido);
            MostrarVentanaExito();
            return;
        }

        string ruta = Path.Combine(Application.persistentDataPath, nombreIntroducido + ".json");

        if (File.Exists(ruta))
        {
            SolicitarSobrescribirFicha(nombreIntroducido);
        }
        else
        {
            nombrePartidaActual = nombreIntroducido;
            GuardarPartida(nombreIntroducido);
            MostrarVentanaExito();
        }
        nombrePartidaActual = nombreIntroducido;
    }

    private void MostrarVentanaExito()
    {
        if (ventanaExito != null)
        {
            ventanaExito.SetActive(true);
        }
    }

    public void DenegarAccionVentana()
    {
        // Primero, pase lo que pase, cerramos la ventana visualmente
        ventanaConfirmacion.SetActive(false);
        if (accionActual == TipoAccion.NuevaSimulacion)
        {
            // Si el usuario puls� "Nueva Simulaci�n", la pregunta era "�Quieres guardar antes de salir?"
            // Pulsar "NO" aqu� significa: "No quiero guardar la partida vieja, pero S� quiero empezar la nueva simulaci�n ya"
            EjecutarReseteoAbsoluto();
        }
        else
        {
            // Si la acci�n era Cargar o Sobrescribir, la pregunta era "�Est�s seguro?"
            // Pulsar "NO" aqu� significa simplemente: "Cancela la operaci�n, no toques nada"
            Debug.Log("[GestorGuardado] Operaci�n cancelada de forma segura por el usuario.");

            // Limpiamos la variable por seguridad para que no se quede apuntando a nada viejo
            archivoPendienteAccion = "";
        }
    }
}