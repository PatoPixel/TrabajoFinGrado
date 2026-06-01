using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
- Pool de bacterias muertas para reutilización y optimización de rendimiento.
- Persiste entre escenas (DontDestroyOnLoad), pero limpia el pool al cargar
  una nueva escena para evitar referencias a objetos destruidos.
*/

public class BacteriasMuertas : MonoBehaviour
{
    public static BacteriasMuertas Instance;
    public Stack<GameObject> bacteriasMuertas = new Stack<GameObject>();
    [SerializeField] private GameObject bacteriaPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Al cargar una nueva escena, los GameObjects del pool anterior ya están
    /// destruidos — limpiamos el stack para evitar MissingReferenceException.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bacteriasMuertas.Clear();
    }

    public void Purga()
    {
        bacteriasMuertas.Clear();
    }

    public GameObject GetBacteria(Vector3 posicion)
    {
        GameObject obj = null;

        // Buscar el primer objeto válido (los destruidos aparecen como null en Unity)
        while (bacteriasMuertas.Count > 0)
        {
            obj = bacteriasMuertas.Pop();
            if (obj != null) break;
            obj = null;
        }

        if (obj != null)
        {
            obj.transform.position = posicion;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(bacteriaPrefab, posicion, Quaternion.identity);
        }

        return obj;
    }
}
