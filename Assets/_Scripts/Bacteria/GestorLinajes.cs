using System.Collections.Generic;
using UnityEngine;

public class GestorLinajes : MonoBehaviour
{
    private static GestorLinajes _instance;
    public static Dictionary<int, SistemaVida> RegistroVida = new Dictionary<int, SistemaVida>();
    public static GestorLinajes Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GestorLinajes>();
            }
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
    public int SiguienteIdDisponible { 
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

    // Nueva función para que las bacterias consulten su nombre
    public string GetNombrePorId(int id)
    {
        if (nombresFamilias == null || nombresFamilias.Count == 0)
            return "Bacteria";

        // Usamos el ID para elegir un nombre de la lista. 
        // El (id - 1) es porque los IDs empiezan en 1 pero la lista en 0.
        // El % asegura que si hay 100 linajes y 17 nombres, no de error.
        int indice = (id - 1) % nombresFamilias.Count;
        return nombresFamilias[indice];
    }

    public void RegistrarVida(int id, SistemaVida vida)
    {
        if (!RegistroVida.ContainsKey(id))
        {
            RegistroVida.Add(id, vida);
        }
    }

    public void EliminarVida(int id)
    {
        if (RegistroVida.ContainsKey(id))
        {
            RegistroVida.Remove(id);
        }
    }

    public void Purga()
    {
        var claves = new List<int>(RegistroVida.Keys);

        foreach (var id in claves)
        {
            if (RegistroVida.TryGetValue(id, out var vida) && vida != null)
            {
                vida.Purga();
            }
        }
        siguienteIdDisponible = 1;
    }
}