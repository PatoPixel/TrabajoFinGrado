using UnityEngine;
using UnityEngine.UI;

/// <summary>Conecta el botón "Siguiente" del tutorial a TutorialManager.SiguientePaso()</summary>
[RequireComponent(typeof(Button))]
public class BotonSiguienteTutorial : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            TutorialManager tutorial = Object.FindFirstObjectByType<TutorialManager>();
            if (tutorial != null) tutorial.SiguientePaso();
        });
    }
}
