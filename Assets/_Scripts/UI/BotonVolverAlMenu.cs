using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adjuntar al botón "Menú Principal" del menú de pausa en GamePlay.
/// Al pulsarlo muestra la confirmación de GestorGuardado y si acepta vuelve al menú.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonVolverAlMenu : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        GestorGuardado gestor = Object.FindFirstObjectByType<GestorGuardado>();
        if (gestor != null)
            gestor.SolicitarVolverAlMenu();
        else
            GestorEscenas.IrAMenuPrincipal(); // fallback si no hay gestor
    }
}
