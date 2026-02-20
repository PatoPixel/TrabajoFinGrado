using System.Collections.Generic;
using UnityEngine;

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
