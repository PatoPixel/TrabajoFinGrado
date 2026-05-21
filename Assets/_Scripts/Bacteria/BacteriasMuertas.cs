using System.Collections.Generic;
using UnityEngine;

/*
- Este script se encarga de gestionar las bacterias muertas, utilizando un pool de objetos para optimizar el rendimiento.
- Cuando una bacteria muere, se desactiva y se guarda en una pila (stack) para su reutilización futura.
- Cuando se necesita una nueva bacteria, se verifica si hay alguna disponible en la pila. Si es así, se reutiliza esa bacteria, de lo contrario, se instancia una nueva.
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
        }
    }

    public GameObject GetBacteria(Vector3 posicion)
    {
        GameObject obj;
        if (bacteriasMuertas.Count > 0)
        {
            obj = bacteriasMuertas.Pop();
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
