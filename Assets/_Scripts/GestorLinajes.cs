using UnityEngine;

public class GestorLinajes : MonoBehaviour
{
    private static GestorLinajes _instance;

    public static GestorLinajes Instance
    {
        get
        {
            // Si la instancia es nula, la buscamos activamente en la escena
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GestorLinajes>();
            }
            return _instance;
        }
    }

    private int siguienteIdDisponible = 1;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject); // Opcional si cambias de escena
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
}