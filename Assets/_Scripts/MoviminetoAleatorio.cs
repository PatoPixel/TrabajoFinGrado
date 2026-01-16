using UnityEngine;

public class MovimientoAleatorio : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float tiempoEntreCambios = 2f;
    [SerializeField] private float velocidadRotacion = 5f; 
    [SerializeField] private float cooldownChoque = 0.5f;
    [SerializeField] private float giroMaximoPorSegundo = 180f; // grados/seg

    private Vector2 direccionObjetivo;
    private float tiempoSiguienteChoque;
    private Rigidbody2D miCuerpoFisico;
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
        // ROTACIÓN SUAVE
        float anguloObjetivo = Mathf.Atan2(
            direccionObjetivo.y,
            direccionObjetivo.x
        ) * Mathf.Rad2Deg;

        float anguloActual = transform.eulerAngles.z;
        float diferencia = Mathf.Abs(
    Mathf.DeltaAngle(anguloActual, anguloObjetivo)
);
        float maxGiro = giroMaximoPorSegundo * Time.fixedDeltaTime;

        float anguloNuevo = Mathf.MoveTowardsAngle(
            anguloActual,
            anguloObjetivo,
            maxGiro
        );

        transform.rotation = Quaternion.Euler(0, 0, anguloNuevo);
        float factorVelocidad = Mathf.InverseLerp(180f, 0f, diferencia);
        // MOVIMIENTO: SIEMPRE HACIA DONDE MIRA
        miCuerpoFisico.linearVelocity = (Vector2)transform.right * velocidad * factorVelocidad;
    }

    void CambiarDireccion()
    {
        direccionObjetivo = Random.insideUnitCircle.normalized;
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
