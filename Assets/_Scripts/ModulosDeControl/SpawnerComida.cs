using UnityEngine;

/*
- Este script se encarga de generar comida (nutrientes) de forma periódica dentro
del área de juego (la "placa de Petri").
- Utiliza el PoolComida para obtener objetos de comida sin necesidad de instanciarlos cada
vez, lo que mejora el rendimiento.
*/

public class SpawnerComida : MonoBehaviour
{

    private const float TamanoNutrienteBase = 0.3f;
    [Header("Configuracion")]
    [Tooltip("Seconds between each nutrient spawn.")]
    public float tiempoEntreComida = 0.5f;

    [Tooltip("Half-extents of the spawn area (Petri dish bounds).")]
    public Vector2 areaGeneracion = new Vector2(8f, 4f);

    private float _cronometro;

    private void Update()
    {
        _cronometro += Time.deltaTime;
        if (_cronometro >= tiempoEntreComida)
        {
            GenerarComida();
            _cronometro = 0f;
        }
    }

    private void GenerarComida()
    {
        if (PoolComida.Instance == null) return;

        float x = Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float y = Random.Range(-areaGeneracion.y, areaGeneracion.y);
        Vector3 posicion = new Vector3(x, y, 0f);

        GameObject obj = PoolComida.Instance.GetComida(posicion, TamanoNutrienteBase);
        obj.GetComponent<Comida>()?.Inicializar(TamanoNutrienteBase);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaGeneracion.x * 2f, areaGeneracion.y * 2f, 0f));
    }
}
