using UnityEngine;

public class MovimientoAleatorio : MonoBehaviour
{    
    public enum EstadoBacteria
    {
        Vagando,
        Persiguiendo
    }
    [Header("Configuraci�n")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float tiempoEntreCambios = 2f;
    [SerializeField] private float cooldownChoque = 0.5f;
    [SerializeField] private float giroMaximoPorSegundo = 180f; // grados/seg

    private Vector2 direccionObjetivo;
    private float tiempoSiguienteChoque;
    private Rigidbody2D miCuerpoFisico;
    private float tiempoRestante;
    private SensorBacteria misOjos;
    private SistemaVida sistemaVida;
    private EstadoBacteria estadoActual;


    void Start()
    {
        miCuerpoFisico = GetComponent<Rigidbody2D>();
        misOjos = GetComponent<SensorBacteria>();
        sistemaVida = GetComponent<SistemaVida>();
        estadoActual = EstadoBacteria.Vagando;
        CambiarDireccion();
    }

    void Update()
    {
       switch(estadoActual)
        {
            case EstadoBacteria.Vagando:
                LogicaVagar();
                break;
            case EstadoBacteria.Persiguiendo:
                LogicaPerseguir();
                break;
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
        // Verificamos el cooldown para no calcular esto 50 veces por segundo si vibra
        if (Time.time > tiempoSiguienteChoque)
        {
            // 1. OBTENER LA NORMAL
            // El "ContactPoint" es el punto exacto donde chocamos. 
            // La "normal" es la dirección hacia afuera de la pared.
            Vector2 normalPared = colision.contacts[0].normal;

            // 2. CÁLCULO VECTORIAL
            // Reflejamos nuestra dirección actual sobre la normal de la pared.
            // Esto nos da la dirección exacta de salida.
            direccionObjetivo = Vector2.Reflect(direccionObjetivo, normalPared).normalized;

            // 3. GESTIÓN DE ESTADO
            // Si estábamos obsesionados persiguiendo comida y nos chocamos (quizás la comida está tras un muro),
            // forzamos a la bacteria a "despejarse" y vagar un rato.
            if (estadoActual == EstadoBacteria.Persiguiendo)
            {
                estadoActual = EstadoBacteria.Vagando;
            }

            // Reiniciamos tiempos
            tiempoRestante = tiempoEntreCambios; // Mantenemos esta nueva dirección un rato
            tiempoSiguienteChoque = Time.time + cooldownChoque;

            // Debug visual para entender el rebote
            Debug.DrawRay(colision.contacts[0].point, normalPared, Color.green, 1f);
            Debug.DrawRay(colision.contacts[0].point, direccionObjetivo, Color.cyan, 1f);
        }
    }

    private void LogicaVagar()
    {
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0)
        {
            CambiarDireccion();
        }
        if (sistemaVida.EnergiaActual <= 80 && misOjos.comidaMasCercana != null)
        {
            estadoActual = EstadoBacteria.Persiguiendo;
        }
    }

    private void LogicaPerseguir()
    {
        if (misOjos.comidaMasCercana == null)
        {
            estadoActual = EstadoBacteria.Vagando;
            return; // "return" aquí hace que la función se acabe YA. No sigue leyendo abajo.
        }
        
        // 1. Calculamos el vector hacia la comida (Destino - Origen)
        Vector2 direccionAComida = misOjos.comidaMasCercana.position - transform.position;

        // 2. Normalizamos para tener solo la dirección
        direccionObjetivo = direccionAComida.normalized;

        // 3. Hack importante:
        // Mantenemos el "tiempoRestante" alto para que NO se active el "CambiarDireccion()" aleatorio
        // mientras estemos persiguiendo comida.
        tiempoRestante = tiempoEntreCambios;

        // Opcional: Dibujar línea amarilla para ver que está "pensando" en ir allí
        Debug.DrawLine(transform.position, misOjos.comidaMasCercana.position, Color.yellow);

        
    }

} // Esta es la última llave del script
