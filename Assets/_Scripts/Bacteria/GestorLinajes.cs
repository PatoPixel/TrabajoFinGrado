using System.Collections.Generic;
using UnityEngine;

public class GestorLinajes : MonoBehaviour
{
    private static GestorLinajes _instance;

    public static Dictionary<int, SistemaVida> RegistroVida = new Dictionary<int, SistemaVida>();
    public Dictionary<int, DatosGeneticos> plantillasLinajes = new Dictionary<int, DatosGeneticos>();
    public Dictionary<int, string> nombresLinajes = new Dictionary<int, string>();

    public static GestorLinajes Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<GestorLinajes>();
            return _instance;
        }
    }

    [Header("Identidad de Linajes")]
    [SerializeField]
    private List<string> nombresFamilias = new List<string>
    {
        "Izan", "Ale", "Irene", "Ramon", "David", "Martin",
        "Jaime", "Gentil", "Chente", "Jose", "Hosco",
        "Marco", "Estevez", "Peralta", "Walker", "Pablo", "Santi"
    };

    private int siguienteIdDisponible = 1;
    public int SiguienteIdDisponible
    {
        get { return siguienteIdDisponible; }
        set { siguienteIdDisponible = value; }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int ObtenerNuevoId()
    {
        int idAEntregar = siguienteIdDisponible;
        siguienteIdDisponible++;
        return idAEntregar;
    }

    public int RegistrarLinajeManual(string nombre, DatosGeneticos stats)
    {
        int nuevoId = ObtenerNuevoId();
        stats.idLinaje = nuevoId;
        stats.generaciones = 1; // Es la cepa original de este linaje

        // Guardamos el ADN y el nombre en los diccionarios
        plantillasLinajes[nuevoId] = stats;
        nombresLinajes[nuevoId] = nombre;

        return nuevoId;
    }
    public string GetNombrePorId(int id)
    {
        if (nombresLinajes.ContainsKey(id))
            return nombresLinajes[id];

        if (nombresFamilias == null || nombresFamilias.Count == 0)
            return "Bacteria";

        int indice = (id - 1) % nombresFamilias.Count;
        return nombresFamilias[indice];
    }

    public DatosGeneticos ObtenerPlantilla(int id)
    {
        if (plantillasLinajes.ContainsKey(id))
            return plantillasLinajes[id];

        return new DatosGeneticos(); // Devuelve vacío si hay error
    }

    public void RegistrarVida(int id, SistemaVida vida) { if (!RegistroVida.ContainsKey(id)) RegistroVida.Add(id, vida); }
    public void EliminarVida(int id) { if (RegistroVida.ContainsKey(id)) RegistroVida.Remove(id); }

    public void Purga()
    {
        var claves = new List<int>(RegistroVida.Keys);
        foreach (var id in claves)
        {
            if (RegistroVida.TryGetValue(id, out var vida) && vida != null) vida.Purga();
        }
        siguienteIdDisponible = 1;
        plantillasLinajes.Clear();
        nombresLinajes.Clear();
    }
}