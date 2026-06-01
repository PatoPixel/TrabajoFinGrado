using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Se adjunta al botón "Tutorial" del Menú Principal.
/// Fija el texto y conecta el onClick a GestorEscenas.IrATutorial().
/// </summary>
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
