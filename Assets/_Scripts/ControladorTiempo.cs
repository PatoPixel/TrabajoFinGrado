using UnityEngine;

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
        // Guardamos el valor original de la física (0.02 por defecto en Unity)
        baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        // Empezamos siempre en velocidad normal
        CambiarVelocidad(velocidadNormal);
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    public void BotonParar() => CambiarVelocidad(velocidadParado);
    public void BotonNormal() => CambiarVelocidad(velocidadNormal);
    public void BotonRapido() => CambiarVelocidad(velocidadRapida);

    // --- LÓGICA CENTRAL ---

    public void CambiarVelocidad(float nuevaEscala)
    {
        EscalaActual = nuevaEscala;
        Time.timeScale = nuevaEscala;

        // Si la velocidad es 0, no multiplicamos el fixedDeltaTime (evitamos errores)
        // Si no es 0, lo ajustamos proporcionalmente como hacías tú
        if (nuevaEscala > 0)
        {
            Time.fixedDeltaTime = baseFixedDeltaTime * nuevaEscala;
        }
        else
        {
            // Cuando está pausado, mantenemos el fixedDeltaTime normal o pequeño
            Time.fixedDeltaTime = baseFixedDeltaTime;
        }

        Debug.Log("Simulación a x" + nuevaEscala);
    }

    void Update()
    {
        // Mantengo tu acceso rápido por teclado, pero ahora cicla: 1 -> 5 -> 0 -> 1...
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Time.timeScale == velocidadNormal) CambiarVelocidad(velocidadRapida);
            else if (Time.timeScale == velocidadRapida) CambiarVelocidad(velocidadParado);
            else CambiarVelocidad(velocidadNormal);
        }
    }
}