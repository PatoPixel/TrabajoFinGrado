using UnityEngine;

public class SelectorBacterias : MonoBehaviour
{
    [Header("Conexion con el Tracker")]
    public EvolutionTracker trackerEvolucion;

    [Header("Panel de Inspeccion")]
    public PanelInspectorBacteria panelInspector;

    private void Update()
    {
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
