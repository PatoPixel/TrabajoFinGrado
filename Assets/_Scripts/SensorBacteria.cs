using UnityEngine;

public class SensorBacteria : MonoBehaviour
{
    [Header("Configuración del Radar")]
    public LayerMask capasObjetivo; // Selecciona "Comida" Y "Bacteria" en el Inspector

    public Transform objetivoMasCercano;
    private SistemaVida sistemaVida;

    private void Start()
    {
        sistemaVida = GetComponent<SistemaVida>();
    }

    void Update()
    {
        BuscarObjetivo();
    }

    void BuscarObjetivo()
    {
        // 1. Escaneamos todo en el radio de visión
        Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, sistemaVida.misStats.radioVision, capasObjetivo);

        float distanciaMinima = Mathf.Infinity;
        Transform candidatoOpcional = null;

        foreach (Collider2D col in detectados)
        {
            // A. Ignorarnos a nosotros mismos
            if (col.gameObject == gameObject) continue;

            // B. Si es una bacteria, aplicar filtros de depredación
            if (col.CompareTag("Bacteria"))
            {
                SistemaVida vidaEncontrada = col.GetComponent<SistemaVida>();

                // Ignorar si es de mi familia
                if (vidaEncontrada.misStats.idLinaje == sistemaVida.misStats.idLinaje) continue;

                // Ignorar si es más grande que yo (no soy tonto, no la cazo)
                if (vidaEncontrada.misStats.tamano >= sistemaVida.misStats.tamano) continue;
            }

            // C. Cálculo de distancia para encontrar al más cercano
            float distancia = Vector2.Distance(transform.position, col.transform.position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                candidatoOpcional = col.transform;
            }
        }

        // 2. Asignamos el resultado
        objetivoMasCercano = candidatoOpcional;

        // 3. Debug Visual
        if (objetivoMasCercano != null)
        {
            Color colorDebug = objetivoMasCercano.CompareTag("Bacteria") ? Color.magenta : Color.red;
            Debug.DrawLine(transform.position, objetivoMasCercano.position, colorDebug);
        }
    }

    private void OnDrawGizmos()
    {
        if (sistemaVida == null) sistemaVida = GetComponent<SistemaVida>();
        if (sistemaVida != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sistemaVida.misStats.radioVision);
        }
    }
}