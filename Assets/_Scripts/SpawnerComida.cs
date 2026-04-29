using UnityEngine;

/// <summary>
/// Spawns nutrients at random positions using PoolComida instead of Instantiate/Destroy.
/// </summary>
public class SpawnerComida : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------
    private const float TamanoNutrienteBase = 0.3f;

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Configuracion")]
    [Tooltip("Seconds between each nutrient spawn.")]
    public float tiempoEntreComida = 0.5f;

    [Tooltip("Half-extents of the spawn area (Petri dish bounds).")]
    public Vector2 areaGeneracion = new Vector2(8f, 4f);

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private float _cronometro;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Update()
    {
        _cronometro += Time.deltaTime;
        if (_cronometro >= tiempoEntreComida)
        {
            GenerarComida();
            _cronometro = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private void GenerarComida()
    {
        if (PoolComida.Instance == null) return;

        float x = Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float y = Random.Range(-areaGeneracion.y, areaGeneracion.y);
        Vector3 posicion = new Vector3(x, y, 0f);

        GameObject obj = PoolComida.Instance.GetComida(posicion, TamanoNutrienteBase);
        obj.GetComponent<Comida>()?.Inicializar(TamanoNutrienteBase);
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaGeneracion.x * 2f, areaGeneracion.y * 2f, 0f));
    }
}
#if false
// Old code — excluded from compilation
public class SpawnerComidaOld
{
    [Header("Configuraci�n")]
    public GameObject prefabComida; // Aqu� arrastraremos el Prefab del nutriente
    public float tiempoEntreComida = 0.5f; // Segundos entre cada aparici�n
    public Vector2 areaGeneracion = new Vector2(8f, 4f); // Tama�o de tu placa de petri

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
        // Calculamos una posici�n al azar dentro de los l�mites
        float xAleatorio = Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float yAleatorio = Random.Range(-areaGeneracion.y, areaGeneracion.y);
        Vector2 posicion = new Vector2(xAleatorio, yAleatorio);

        // Creamos la comida en esa posici�n
        Instantiate(prefabComida, posicion, Quaternion.identity);
    }

    // Esto sirve para ver el �rea de generaci�n en la ventana Scene (no se ve en el juego)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaGeneracion.x * 2, areaGeneracion.y * 2, 0));
    }
}
#endif
