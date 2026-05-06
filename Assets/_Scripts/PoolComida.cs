using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for nutrient GameObjects. Mirrors BacteriasMuertas pattern.
/// Call GetComida() to rent an instance and Comida.Devolver() to return it.
/// </summary>
public class PoolComida : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------
    private const int CapacidadInicial = 200;

    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static PoolComida Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [SerializeField] private GameObject prefabComida;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private readonly Stack<GameObject> _pool = new Stack<GameObject>(CapacidadInicial);

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PreCalentar();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a nutrient instance placed at the given world position.
    /// Scale is set proportional to <paramref name="tamano"/> if > 0.
    /// </summary>
    public GameObject GetComida(Vector3 posicion, float tamano = 0.3f)
    {
        GameObject obj = _pool.Count > 0
            ? _pool.Pop()
            : Instantiate(prefabComida);

        obj.transform.position = posicion;
        obj.transform.localScale = Vector3.one * Mathf.Max(0.1f, tamano);
        obj.SetActive(true);
        return obj;
    }

    /// <summary>Returns a nutrient instance to the pool.</summary>
    public void Devolver(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Push(obj);
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private void PreCalentar()
    {
        for (int i = 0; i < CapacidadInicial; i++)
        {
            GameObject obj = Instantiate(prefabComida);
            obj.SetActive(false);
            _pool.Push(obj);
        }
    }

    public void Purga()
    {
        Comida[] comidasEnMapa = FindObjectsByType<Comida>(FindObjectsSortMode.None);
        foreach (Comida comida in comidasEnMapa)
        {
            Devolver(comida.gameObject);
        }
    }
}
