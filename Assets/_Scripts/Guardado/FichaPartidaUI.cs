using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GestorGuardado;

public class FichaPartidaUI : MonoBehaviour
{
    [Header("Textos de la Ficha")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoFecha;
    public TextMeshProUGUI textoFechaCreación;

    // Dejo estos listos para cuando metamos los datos extra
    public TextMeshProUGUI textoBacterias;
    public TextMeshProUGUI textoLinajes;
    public TextMeshProUGUI textoHorasJugadas;

    [Header("Botones")]
    public Button botonCargar;
    public Button botonGuardar; // Para sobrescribir
    public Button botonBorrar;

    [Header("Imágenes")]
    public Image imagenCaptura;

    // Referencia al gestor maestro para poder darle las órdenes
    private GestorGuardado gestor;
    private string nombrePartidaAsociada;

    // Este método lo llamará el Menú al crear la ficha
    public void ConfigurarFicha(string nombre, DateTime fechaUltima, GestorGuardado gestorPrincipal)
    {
        nombrePartidaAsociada = nombre;
        gestor = gestorPrincipal;

        // 1. Datos básicos
        textoNombre.text = nombre;

        // 2. Buscar y cargar la Captura de Pantalla
        string rutaImagen = Path.Combine(Application.persistentDataPath, nombre + ".png");
        if (File.Exists(rutaImagen))
        {
            // Convertimos el archivo del disco en una textura de Unity
            byte[] bytesImagen = File.ReadAllBytes(rutaImagen);
            Texture2D textura = new Texture2D(2, 2);
            textura.LoadImage(bytesImagen); // Esto auto-ajusta el tamaño

            // Convertimos la textura en un Sprite para la UI
            imagenCaptura.sprite = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f));
        }

        // 3. Buscar y cargar los Metadatos
        string rutaMeta = Path.Combine(Application.persistentDataPath, nombre + "_meta.json");
        if (File.Exists(rutaMeta))
        {
            string jsonMeta = File.ReadAllText(rutaMeta);
            MetadatosPartida meta = JsonUtility.FromJson<MetadatosPartida>(jsonMeta);

            textoBacterias.text = "Bacterias: " + meta.totalBacterias.ToString();
            textoLinajes.text = "Linajes: " + meta.totalLinajesRestantes.ToString();
            textoFechaCreación.text = "Creado: " + meta.fechaCreacion;

            // Formateamos los segundos en "00h 00m"
            TimeSpan tiempo = TimeSpan.FromSeconds(meta.horasJugadas);
            textoHorasJugadas.text = "Tiempo: " + string.Format("{0:D2}h {1:D2}m", tiempo.Hours, tiempo.Minutes);
            textoFecha.text = "Última Modificación: " + fechaUltima.ToString("dd/MM/yyyy HH:mm");
        }
        else
        {
            // Si es una partida antigua que no tenía metadatos, ponemos valores por defecto
            textoBacterias.text = "Bacterias: ---";
            textoLinajes.text = "Linajes: ---";
            textoFechaCreación.text = "Creado: ---";
            textoHorasJugadas.text = "Tiempo: ---";
        }

        // Limpiamos la memoria de los botones por si acaso
        botonCargar.onClick.RemoveAllListeners();
        botonBorrar.onClick.RemoveAllListeners();
        botonGuardar.onClick.RemoveAllListeners();

        // Le decimos a cada botón qué debe hacer cuando lo pulsen
        botonCargar.onClick.AddListener(AlPulsarCargar);
        botonBorrar.onClick.AddListener(AlPulsarBorrar);
        botonGuardar.onClick.AddListener(AlPulsarSobrescribir);
    }

    private void AlPulsarCargar()
    {
        Debug.Log("Cargando: " + nombrePartidaAsociada);
        gestor.CargarPartida(nombrePartidaAsociada);
    }

    private void AlPulsarBorrar()
    {
        Debug.Log("Borrando: " + nombrePartidaAsociada);
        gestor.BorrarPartida(nombrePartidaAsociada);
        Destroy(this.gameObject); // Destruimos la ficha visualmente
    }

    private void AlPulsarSobrescribir()
    {
        Debug.Log("Sobrescribiendo: " + nombrePartidaAsociada);
        gestor.GuardarPartida();
        // Aquí podríamos actualizar la fecha visualmente
    }

    void OnDestroy()
    {
        // Liberamos la RAM de la imagen cuando la ficha se destruya
        if (imagenCaptura != null && imagenCaptura.sprite != null)
        {
            Destroy(imagenCaptura.sprite.texture);
            Destroy(imagenCaptura.sprite);
        }
    }
}