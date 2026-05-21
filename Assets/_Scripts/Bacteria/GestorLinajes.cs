using System.Collections.Generic;
using UnityEngine;

/*
- Este script se encarga de gestionar los linajes de las bacterias, asignando un
ID unico a cada linaje y manteniendo un registro de las bacterias vivas asociadas a cada ID.
- Proporciona un metodo para obtener un nuevo ID de linaje, que se incrementa automaticamente cada vez que se solicita uno nuevo.
- Tambien incluye una lista de nombres de familias para asignar a los linajes, y un metodo para obtener el nombre de un linaje a partir de su ID.
- Ademas, tiene un metodo para purgar el registro de vida, eliminando todas las bacterias vivas registradas y reiniciando el contador de IDs.
*/

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