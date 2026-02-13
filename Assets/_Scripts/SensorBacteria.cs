using UnityEngine;

public class SensorBacteria : MonoBehaviour
{
    [Header("Configuración del Radar")]
    public LayerMask capasObjetivo;

    public Transform comidaCercana;
    public Transform AmenazaCercana;
    private SistemaVida sistemaVida;
    private float contadorBusqueda = 0.2f;

    private static Collider2D[] resultadosBuffer = new Collider2D[16];
    private ContactFilter2D filtro;

    private void Start()
    {
        sistemaVida = GetComponent<SistemaVida>();
        
        filtro.SetLayerMask(capasObjetivo);
        filtro.useLayerMask = true;
        filtro.useTriggers = true;
    }

    void Update()
    {
        contadorBusqueda -= Time.deltaTime;
        if (contadorBusqueda <= 0)
        {
            BuscarObjetivo();
            contadorBusqueda = Random.Range(0.1f,0.3f);
        }
    }

    void BuscarObjetivo()
    {
        // 1. Escaneamos todo en el radio de visión
        int cantidadDetectada = Physics2D.OverlapCircle(
                transform.position,
                sistemaVida.misStats.radioVision,
                filtro,
                resultadosBuffer
            );

        float distanciaMinima = Mathf.Infinity;
        Transform candidatoOpcional = null;
        AmenazaCercana = null;
        comidaCercana = null;
        for (int i = 0; i < cantidadDetectada; i++)
        {
            
            Collider2D col = resultadosBuffer[i];
            // Ignorarnos a nosotros mismos
            if (col.gameObject == gameObject) continue;
          
            int idDelDetectado = col.gameObject.GetInstanceID();
            if (GestorLinajes.RegistroVida.TryGetValue(idDelDetectado, out SistemaVida vidaEncontrada))
            {

                // Ignorar si no tiene stats (por seguridad, aunque no debería pasar)
                if (vidaEncontrada == null) continue;

                // Ignorar si es de mi familia
                if (vidaEncontrada.misStats.idLinaje == sistemaVida.misStats.idLinaje) continue;
                // Si el otro es un 20% más grande que yo huyo
                if (vidaEncontrada.misStats.tamano > sistemaVida.misStats.tamano * 1.2f)
                {
                    // PRIORIDAD MÁXIMA: Guardamos la amenaza y abortamos búsqueda
                    AmenazaCercana = col.transform;
                    break;
                }

                //Si no es 20% mas pequeño que yo lo ignoro
                if (sistemaVida.misStats.tamano <= vidaEncontrada.misStats.tamano * 1.2f) continue;



                }

            // Cálculo de distancia para encontrar al más cercano
            float distancia = (transform.position - col.transform.position).sqrMagnitude;
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                candidatoOpcional = col.transform;
            }
        }

        // 2. Asignamos el resultado
        comidaCercana = candidatoOpcional;

        // 3. Debug Visual
        if (comidaCercana != null)
        {
            Color colorDebug = comidaCercana.CompareTag("Bacteria") ? Color.magenta : Color.yellow;
            Debug.DrawLine(transform.position, comidaCercana.position, colorDebug);
        }

        if(AmenazaCercana != null)
        {
            Debug.DrawLine(transform.position, AmenazaCercana.position, Color.red);
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