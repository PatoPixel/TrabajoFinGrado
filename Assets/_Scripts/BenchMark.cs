using UnityEngine;

public class BenchmarkManager : MonoBehaviour
{
    [Header("Configuración")]
    public int cantidadASpawnear = 100;
    public Vector2 areaGeneracion = new Vector2(8f, 4f);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < cantidadASpawnear; i++)
            {
                // Usamos tu Pool para no destruir la memoria
                GameObject b = BacteriasMuertas.Instance.GetBacteria(RandomPosition());
                b.GetComponent<SistemaVida>().AsignarStatsBase();
            }
        }
    }

    private Vector3 RandomPosition()
    {
        // Define los límites de tu mapa (ajusta estos números a tu cámara/escenario)
        float x = UnityEngine.Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float y = UnityEngine.Random.Range(-areaGeneracion.y, areaGeneracion.y);

        return new Vector3(x, y, 0);
    }
}