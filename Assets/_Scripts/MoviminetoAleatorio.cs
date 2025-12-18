using UnityEngine;

public class MovimientoAleatorio : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float tiempoEntreCambios = 2f;
    [SerializeField] private float velocidadRotacion = 5f; 
    [SerializeField] private float cooldownChoque = 0.5f; 

    private float tiempoSiguienteChoque;
    private Rigidbody2D miCuerpoFisico;
    private Vector2 direccionMovimiento;
    private float tiempoRestante;

    void Start()
    {
        miCuerpoFisico = GetComponent<Rigidbody2D>();
        CambiarDireccion();
    }

    void Update()
    {
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0)
        {
            CambiarDireccion();
        }
    }

    void FixedUpdate()
    {
        // 1. MOVER
        miCuerpoFisico.linearVelocity = direccionMovimiento * velocidad;

        // 2. GIRAR (NUEVO)
        if (miCuerpoFisico.linearVelocity.magnitude > 0.1f)
        {
            float angulo = Mathf.Atan2(miCuerpoFisico.linearVelocity.y, miCuerpoFisico.linearVelocity.x) * Mathf.Rad2Deg;

            // Al mirar a la derecha, el ajuste es 0
            float ajusteGrados = 0;

            Quaternion rotacionObjetivo = Quaternion.AngleAxis(angulo + ajusteGrados, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.fixedDeltaTime);
        }
    }

    void CambiarDireccion()
    {
        direccionMovimiento = Random.insideUnitCircle.normalized;
        tiempoRestante = tiempoEntreCambios;
    }

    // Esta función se activa cuando la bacteria choca físicamente con algo sólido
    private void OnCollisionEnter2D(Collision2D colision)
    {
        if (Time.time > tiempoSiguienteChoque)
        {
            Debug.Log("Choque válido contra: " + colision.gameObject.name);

            CambiarDireccion();
            tiempoRestante = tiempoEntreCambios;

            // Bloqueamos nuevos choques hasta dentro de medio segundo (0.5s)
            tiempoSiguienteChoque = Time.time + cooldownChoque;
        }
    }
} // Esta es la última llave del script
