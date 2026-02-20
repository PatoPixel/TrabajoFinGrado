using UnityEngine;

public class SelectorBacterias : MonoBehaviour
{
    [Header("Conexión con el Tracker")]
    // Referencia al Tracker para decirle qué familia queremos ver
    public EvolutionTracker trackerEvolucion;

    void Update()
    {
        // 1. Detectar el clic izquierdo del ratón (0 es el botón izquierdo)
        if (Input.GetMouseButtonDown(0))
        {
            // 2. Traducir los píxeles de la pantalla al mundo 2D usando la cámara principal
            Vector2 posicionRatonMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 3. Pinchar con un "punto físico" invisible en esa coordenada
            Collider2D bacteriaTocada = Physics2D.OverlapPoint(posicionRatonMundo);

            // 4. Comprobar si hemos tocado una bacteria
            if (bacteriaTocada != null)
            {
                // Obtenemos su información (¡Ojo! Usar GetComponent en un clic está bien, 
                // pero no lo haríamos en un Update de 1000 bacterias)
                SistemaVida vida = bacteriaTocada.GetComponent<SistemaVida>();

                if (vida != null)
                {
                    int idFamilia = vida.misStats.idLinaje;
                    // Aquí le diremos al Tracker que cambie la gráfica
                    if (trackerEvolucion != null)
                    {
                        trackerEvolucion.CambiarFamiliaSeleccionada(idFamilia);
                    }
                }
            }
        }
    }
}