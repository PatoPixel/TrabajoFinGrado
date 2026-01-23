using UnityEngine;

public class SensorBacteria : MonoBehaviour
{
    [Header("Configuración del Radar")]
    public float radioDeteccion = 5f; // Cuán lejos ve
    public LayerMask capaComida;      // FILTRO: ¿Qué es comida?

    // Variable para guardar la comida que hemos encontrado (si hay alguna)
    public Transform comidaMasCercana; 

    void Update()
    {
        // En cada frame, buscamos si hay algo rico cerca
        BuscarComida();
    }

    void BuscarComida()
    {
        // 1. LANZAR EL RADAR
        // Physics2D.OverlapCircle devuelve UN collider si encuentra algo en esa capa.
        // (Nota: Si quisieras encontrar TODAS las comidas cercanas, usarías OverlapCircleAll)


        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, radioDeteccion, capaComida);

        Collider2D comidaMasCercanaCollider = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaComida);

        
        if (comidaMasCercanaCollider != null)
        {

            foreach (Collider2D comida in collider2Ds)
            {
                if (comidaMasCercana == null)
                {
                    comidaMasCercana = comida.transform;
                }
                else
                {
                    float distanciaComidaActual = Vector2.Distance(transform.position, comida.transform.position);
                    float distanciaComidaMasCercana = Vector2.Distance(transform.position, comidaMasCercana.position);
                    if (distanciaComidaActual < distanciaComidaMasCercana)
                    {
                        comidaMasCercana = comida.transform;
                    }
                }
            }

            // DIBUJO DE DEBUG (Opcional): Dibuja una línea roja hacia la comida
            Debug.DrawLine(transform.position, comidaMasCercana.position, Color.red);
        }
        else
        {
            comidaMasCercana = null;
        }
    }

    // VITAL: Dibujar el rango en el editor para saber qué estamos haciendo
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Dibuja un círculo hueco representando el rango de visión
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}