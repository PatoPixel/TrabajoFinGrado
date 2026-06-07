using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
- Botón que abre el tutorial desde el menú principal.
- Se encarga de fijar el texto del botón y conectar su evento onClick a la
*/
[RequireComponent(typeof(Button))]
public class BotonTutorial : MonoBehaviour
{
    private void Awake()
    {
        // Fijar texto
        TextMeshProUGUI tmp = GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = "Tutorial";

        // Cablear botón
        GestorEscenas gestor = Object.FindFirstObjectByType<GestorEscenas>();
        if (gestor != null)
            GetComponent<Button>().onClick.AddListener(gestor.IrATutorial);
    }
}
