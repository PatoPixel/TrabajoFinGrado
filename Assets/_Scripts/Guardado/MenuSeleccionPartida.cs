using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro; // Si usas TextMeshPro

public class MenuSeleccionPartidas : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject botonPrefab;      // El azul de tu carpeta Assets
    public Transform contenedorBotones;  // El objeto "Content" del ScrollView

    [Header("Referencias Externas")]
    public GestorGuardado gestorGuardado; // Para llamar a la carga real

    private void Awake()
    {
        gameObject.SetActive(false); // Aseguramos que el menú esté cerrado al inicio
    }
    void OnEnable()
    {
        // Cada vez que abras el menú, se actualizará la lista
        RefrescarMenu();
    }

    public void RefrescarMenu()
    {
        // 1. Limpiar fichas viejas
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        // 2. Obtener y ordenar archivos
        string ruta = Application.persistentDataPath;
        DirectoryInfo info = new DirectoryInfo(ruta);

        // Obtenemos los archivos, ignoramos los que terminan en "_meta.json" y los ordenamos
        var archivos = info.GetFiles("*.json")
                           .Where(f => !f.Name.EndsWith("_meta.json"))
                           .OrderByDescending(f => f.LastWriteTime)
                           .ToList();

        // 3. Crear las Fichas
        foreach (FileInfo archivo in archivos)
        {
            GameObject nuevaFicha = Instantiate(botonPrefab, contenedorBotones);
            string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Name);

            // Obtenemos el script de la ficha y le pasamos los datos
            FichaPartidaUI scriptFicha = nuevaFicha.GetComponent<FichaPartidaUI>();
            scriptFicha.ConfigurarFicha(nombreLimpio, archivo.LastWriteTime, gestorGuardado);
        }
    }
    public void Cerrar()
    {
        gameObject.SetActive(false);
    }
}