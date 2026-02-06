using UnityEngine;

public class SensorBacteria : MonoBehaviour
{
    [Header("Configuración del Radar")]
    public LayerMask capasObjetivo; // Selecciona "Comida" Y "Bacteria" en el Inspector

    public Transform objetivoMasCercano;
    private SistemaVida sistemaVida;
    private float contadorBusqueda = 0.2f;

    private void Start()
    {
        sistemaVida = GetComponent<SistemaVida>();
    }

    void Update()
    {
        contadorBusqueda -= Time.deltaTime;
        if (contadorBusqueda <= 0)
        {
            BuscarObjetivo();
            contadorBusqueda = 0.2f; // Reiniciamos el contador para la próxima búsqueda
        }
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
          
            int idDelDetectado = col.gameObject.GetInstanceID();
            if(GestorLinajes.RegistroVida.TryGetValue(idDelDetectado, out SistemaVida vidaEncontrada))
            {
                // Ignorar si no tiene stats (por seguridad, aunque no debería pasar)
                if (vidaEncontrada == null) continue;

                // Ignorar si es de mi familia
                if (vidaEncontrada.misStats.idLinaje == sistemaVida.misStats.idLinaje) continue;
                
                // Ignorar si es más grande que yo (no soy tonto, no la cazo)
                if (vidaEncontrada.misStats.tamano * 1.2f >= sistemaVida.misStats.tamano) continue;
            }
            

            // C. Cálculo de distancia para encontrar al más cercano
            float distancia = (transform.position - col.transform.position).sqrMagnitude;
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