using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MenuSeleccionPartidas : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject botonPrefab;      // El prefab azul de tu ficha
    public Transform contenedorBotones;  // El objeto "Content" del ScrollView

    [Header("Referencias Externas")]
    public GestorGuardado gestorGuardado; // Para asociarlo a las fichas

    private void Awake()
    {
        gameObject.SetActive(false); // Aseguramos que el menú esté cerrado al inicio
    }

    void OnEnable()
    {
        // Cada vez que abras el menú (o hagas .SetActive(true)), se actualizará la lista
        RefrescarMenu();
    }

    public void RefrescarMenu()
    {
        // 1. Limpiar fichas viejas de forma segura
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        // 2. Obtener y ordenar archivos de la carpeta persistente
        string ruta = Application.persistentDataPath;
        DirectoryInfo info = new DirectoryInfo(ruta);

        // Obtenemos los archivos, ignoramos metadatos y ordenamos por fecha de modificación (más reciente primero)
        var archivos = info.GetFiles("*.json")
                           .Where(f => !f.Name.EndsWith("_meta.json"))
                           .OrderByDescending(f => f.LastWriteTime)
                           .ToList();

        // 3. Crear las Fichas dinámicamente
        foreach (FileInfo archivo in archivos)
        {
            GameObject nuevaFicha = Instantiate(botonPrefab, contenedorBotones);
            string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Name);

            // Obtenemos el script de la ficha y le pasamos los datos reales y la fecha exacta del archivo
            FichaPartidaUI scriptFicha = nuevaFicha.GetComponent<FichaPartidaUI>();
            if (scriptFicha != null)
            {
                scriptFicha.ConfigurarFicha(nombreLimpio, archivo.LastWriteTime, gestorGuardado);
            }
        }

        Debug.Log("Menú de partidas sincronizado y ordenado por fecha.");
    }

    public void Cerrar()
    {
        gameObject.SetActive(false);
    }
}