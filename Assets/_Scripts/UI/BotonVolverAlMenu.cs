using UnityEngine;
using UnityEngine.UI;

/*
- Botón que vuelve al menú principal desde la pantalla de juego.
*/
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
