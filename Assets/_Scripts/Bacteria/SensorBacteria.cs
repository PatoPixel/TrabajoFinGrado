using UnityEngine;

/*
- Este script se encarga de controlar los sensores de las bacterias, permitiéndoles detectar comida y amenazas en su entorno.
- Utiliza un sistema de capas para filtrar los objetos que la bacteria puede detectar, y un buffer de colisiones para optimizar el rendimiento.
- La bacteria escanea su entorno cada cierto tiempo, buscando objetos dentro de su radio de visión  
y asignando el más cercano como comida, siempre y cuando no sea de su mismo linaje y sea significativamente más pequeño.
- Si detecta una amenaza (un objeto significativamente más grande que ella), asigna esa amenaza como prioridad máxima, ignorando cualquier comida detectada.

//Posible annadido al juego y no solo depuracion:
- El script también incluye una función para dibujar un gizmo en el editor de Unity, mostrando el radio de visión de la bacteria para facilitar la depuración y el diseño del entorno.
*/

public class SensorBacteria : MonoBehaviour
{
    [Header("Configuraci�n del Radar")]
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
        // Escaneamos todo en el radio de vision
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

                // Ignorar si no tiene stats (aunque no deberia pasar)
                if (vidaEncontrada == null) continue;

                // Ignorar si es de mi familia
                if (vidaEncontrada.misStats.idLinaje == sistemaVida.misStats.idLinaje) continue;
                // Si el otro es un 20% mas grande que yo huyo
                if (vidaEncontrada.misStats.tamano > sistemaVida.misStats.tamano * 1.2f)
                {
                    //  Guardamos la amenaza y abortamos busqueda
                    AmenazaCercana = col.transform;
                    break;
                }

                //Si no es 20% mas pequenno que yo lo ignoro
                if (sistemaVida.misStats.tamano <= vidaEncontrada.misStats.tamano * 1.2f) continue;



                }

            // Calculo de distancia para encontrar al mas cercano
            float distancia = (transform.position - col.transform.position).sqrMagnitude;
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                candidatoOpcional = col.transform;
            }
        }

        // Asignamos el resultado
        comidaCercana = candidatoOpcional;

        // Debug Visual
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