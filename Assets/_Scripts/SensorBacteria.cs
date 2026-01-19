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
        Collider2D comidaDetectada = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaComida);

        if (comidaDetectada != null)
        {
            // ¡Hemos visto algo! Guardamos su Transform para saber dónde está
            comidaMasCercana = comidaDetectada.transform;
            
            // DIBUJO DE DEBUG (Opcional): Dibuja una línea roja hacia la comida
            Debug.DrawLine(transform.position, comidaMasCercana.position, Color.red);
        }
        else
        {
            // No hay nada cerca :(
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