using UnityEngine;

/*
- Este script se encarga de detectar clics del mouse sobre las bacterias en el mundo del juego.
- Cuando el jugador hace clic sobre una bacteria, se abre un panel de inspección con información
detallada sobre esa bacteria, y se notifica al EvolutionTracker para que cambie la familia seleccionada.
- Requiere que las bacterias tengan un componente SistemaVida para funcionar correctamente (asi que las comidas por ejemplo les da igual).
*/

public class SelectorBacterias : MonoBehaviour
{
    [Header("Conexion con el Tracker")]
    public EvolutionTracker trackerEvolucion;

    [Header("Panel de Inspeccion")]
    public PanelInspectorBacteria panelInspector;

    private void Update()
    {
        // Solo respondemos al clic izquierdo del mouse.
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 posicionRatonMundo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D bacteriaTocada  = Physics2D.OverlapPoint(posicionRatonMundo);

        if (bacteriaTocada == null)
        {
            Debug.Log($"[SelectorBacterias] Clic en {posicionRatonMundo} — ningun collider.");
            return;
        }

        Debug.Log($"[SelectorBacterias] Collider tocado: '{bacteriaTocada.gameObject.name}' tag='{bacteriaTocada.tag}'");

        SistemaVida vida = bacteriaTocada.GetComponent<SistemaVida>();

        if (vida == null)
        {
            Debug.Log("[SelectorBacterias] El collider no tiene SistemaVida, ignorando.");
            return;
        }

        Debug.Log($"[SelectorBacterias] Bacteria seleccionada: {vida.gameObject.name} (linaje {vida.misStats.idLinaje})");

        // Notify the evolution tracker to switch the tracked family.
        if (trackerEvolucion != null)
            trackerEvolucion.CambiarFamiliaSeleccionada(vida.misStats.idLinaje);

        // Open the inspection panel for this bacteria.
        if (panelInspector != null)
            panelInspector.Abrir(vida);
        else
            Debug.LogWarning("[SelectorBacterias] panelInspector no asignado en el Inspector.");
    }
}
