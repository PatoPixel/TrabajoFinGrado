using System.Linq;
using TMPro;
using UnityEngine;
/*
- Controla la velocidad de la simulacion (tiempo) segun el boton que se pulse: parado, normal o rapido
- Ajusta el Time.timeScale de Unity para cambiar la velocidad global del juego, y tambien
ajusta el Time.fixedDeltaTime para que la fisica siga funcionando correctamente incluso a velocidades muy altas o con la simulacion pausada
- Permite cambiar la velocidad con atajos de teclado (T) para alternar entre las 3 velocidades
*/

public class ControladorTiempo : MonoBehaviour
{
    [SerializeField] private float velocidadNormal = 1f;
    [SerializeField] private float velocidadRapida = 5f;
    [SerializeField] private float velocidadParado = 0f;

    private float baseFixedDeltaTime;
    // Guardamos la velocidad actual por si otros scripts (como el de Guardado) necesitan consultarla
    public float EscalaActual { get; private set; }

    void Awake()
    {
        // Guardamos el valor original de la fisica (0.02 por defecto en Unity)
        baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        // Empezamos siempre en velocidad normal
        CambiarVelocidad(velocidadNormal);
    }

    // --- METODOS PARA LOS BOTONES ---

    public void BotonParar() => CambiarVelocidad(velocidadParado);
    public void BotonNormal() => CambiarVelocidad(velocidadNormal);
    public void BotonRapido() => CambiarVelocidad(velocidadRapida);

    // --- LOGICA ---

    public void CambiarVelocidad(float nuevaEscala)
    {
        EscalaActual = nuevaEscala;
        Time.timeScale = nuevaEscala;

        // Si la velocidad es 0, no multiplicamos el fixedDeltaTime (evitamos errores)
        // Si no es 0, lo ajustamos proporcionalmente para que la fisica siga funcionando correctamente
        if (nuevaEscala > 0)
        {
            Time.fixedDeltaTime = baseFixedDeltaTime * nuevaEscala;
        }
        else
        {
            // Cuando este pausado, mantenemos el fixedDeltaTime normal para evitar problemas con la fisica al reanudar
            Time.fixedDeltaTime = baseFixedDeltaTime;
        }

        Debug.Log("Simulacion a x" + nuevaEscala);
    }

    void Update()
    {
        // Comprobar si algún TMP_InputField está enfocado para evitar el atajo de teclado
        if (TMP_InputField.allSelectablesArray != null && TMP_InputField.allSelectablesArray.Any(s => s is TMP_InputField input && input.isFocused))
            return;
        if (ControladorMenuPausa.juegoPausado) return;

        // Atajo de teclado para cambiar la velocidad (T) - Alterna entre normal, rapido y parado
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Time.timeScale == velocidadNormal) CambiarVelocidad(velocidadRapida);
            else if (Time.timeScale == velocidadRapida) CambiarVelocidad(velocidadParado);
            else CambiarVelocidad(velocidadNormal);
        }
    }
}