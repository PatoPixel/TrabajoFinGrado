using System.Collections.Generic;
using UnityEngine;

/*
- Pool de objetos para la comida, para evitar instanciaciones y destrucciones constantes que puedan afectar al rendimiento
- Tiene una capacidad inicial que se pre-calienta al inicio, y luego puede crecer dinámicamente si se necesitan más objetos de los que hay en la pool
- Cada objeto de comida tiene un método para inicializar su tamaño y energía, y otro para devolverlo a la pool cuando ya no se necesita
*/


public class PoolComida : MonoBehaviour
{
    private const int CapacidadInicial = 200;
    public static PoolComida Instance { get; private set; }

    [SerializeField] private GameObject prefabComida;
    private readonly Stack<GameObject> _pool = new Stack<GameObject>(CapacidadInicial);

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

    public void Devolver(GameObject obj)
    {
        //Si el objeto ya está inactivo, no lo devolvemos a la pool para evitar duplicados
        if (!obj.activeSelf) return;
        obj.SetActive(false);
        _pool.Push(obj);
    }

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
