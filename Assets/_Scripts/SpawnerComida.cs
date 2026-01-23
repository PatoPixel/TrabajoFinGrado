using UnityEngine;

public class SpawnerComida : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject prefabComida; // Aquí arrastraremos el Prefab del nutriente
    public float tiempoEntreComida = 0.5f; // Segundos entre cada aparición
    public Vector2 areaGeneracion = new Vector2(8f, 4f); // Tamaño de tu placa de petri

    private float cronometro;

    void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro >= tiempoEntreComida)
        {
            GenerarComida();
            cronometro = 0;
        }
    }

    void GenerarComida()
    {
        // Calculamos una posición al azar dentro de los límites
        float xAleatorio = Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float yAleatorio = Random.Range(-areaGeneracion.y, areaGeneracion.y);
        Vector2 posicion = new Vector2(xAleatorio, yAleatorio);

        // Creamos la comida en esa posición
        Instantiate(prefabComida, posicion, Quaternion.identity);
    }

    // Esto sirve para ver el área de generación en la ventana Scene (no se ve en el juego)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaGeneracion.x * 2, areaGeneracion.y * 2, 0));
    }
}