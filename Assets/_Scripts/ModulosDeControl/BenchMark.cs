using System.Linq;
using TMPro;
using UnityEngine;
/*
- Presionar espacio para spawnear una cantidad de bacterias en posiciones aleatorias dentro de un area determinada
*/
public class BenchmarkManager : MonoBehaviour
{
    [Header("Configuraci�n")]
    public int cantidadASpawnear = 100;
    public Vector2 areaGeneracion = new Vector2(8f, 4f);

    void Update()
    {
        if (TMP_InputField.allSelectablesArray != null && TMP_InputField.allSelectablesArray.Any(s => s is TMP_InputField input && input.isFocused))
            return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < cantidadASpawnear; i++)
            {
                // Usamos la Pool
                GameObject b = BacteriasMuertas.Instance.GetBacteria(RandomPosition());
                b.GetComponent<SistemaVida>().AsignarStatsBase();
            }
        }
    }

    private Vector3 RandomPosition()
    {
        //Generar posicion aleatoria dentro de los valores establecidos
        float x = UnityEngine.Random.Range(-areaGeneracion.x, areaGeneracion.x);
        float y = UnityEngine.Random.Range(-areaGeneracion.y, areaGeneracion.y);

        return new Vector3(x, y, 0);
    }
}