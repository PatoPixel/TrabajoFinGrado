using UnityEngine;
using UnityEngine.UI;

/*
- Botón que avanza al siguiente paso del tutorial.
*/
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
