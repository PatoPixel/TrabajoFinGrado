using UnityEngine;

public class MovimientoAleatorio : MonoBehaviour
{
    public enum EstadoBacteria
    {
        Vagando,
        Persiguiendo,
        Reproduciendo
    }

    [Header("Configuración de Movimiento")]
    [SerializeField] private float tiempoEntreCambios = 2f;
    [SerializeField] private float cooldownChoque = 0.5f;
    [SerializeField] private float giroMaximoPorSegundo = 180f;
    [SerializeField] private float suavizadoVelocidad = 2f;

    [Header("Fuerzas Biológicas")]
    [SerializeField] private float fuerzaRepulsion = 2f;
    [SerializeField] private float fuerzaSeparacion = 1f;

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
        switch (estadoActual)
        {
            case EstadoBacteria.Vagando:
                LogicaVagar();
                break;
            case EstadoBacteria.Persiguiendo:
                LogicaPerseguir();
                break;
            case EstadoBacteria.Reproduciendo:
                LogicaReproducirse();
                break;
        }
    }

    void FixedUpdate()
    {
        float anguloObjetivo = Mathf.Atan2(direccionObjetivo.y, direccionObjetivo.x) * Mathf.Rad2Deg;
        float anguloActual = transform.eulerAngles.z;
        float diferencia = Mathf.Abs(Mathf.DeltaAngle(anguloActual, anguloObjetivo));
        float maxGiro = giroMaximoPorSegundo * Time.fixedDeltaTime;

        float anguloNuevo = Mathf.MoveTowardsAngle(anguloActual, anguloObjetivo, maxGiro);
        transform.rotation = Quaternion.Euler(0, 0, anguloNuevo);

        float factorVelocidad = Mathf.InverseLerp(180f, 0f, diferencia);
        Vector2 velocidadDeseada = (Vector2)transform.right * sistemaVida.misStats.velocidad * factorVelocidad;

        miCuerpoFisico.linearVelocity = Vector2.Lerp(miCuerpoFisico.linearVelocity, velocidadDeseada, Time.fixedDeltaTime * suavizadoVelocidad);
    }

    void CambiarDireccion()
    {
        direccionObjetivo = Random.insideUnitCircle.normalized;
        tiempoRestante = tiempoEntreCambios;
    }

    // --- LÓGICA DE CHOQUE EVOLUCIONADA ---
    private void OnCollisionEnter2D(Collision2D colision)
    {
        if (colision.gameObject.CompareTag("Bacteria"))
        {
            SistemaVida otraVida = colision.gameObject.GetComponent<SistemaVida>();
            if (otraVida == null) return;

            // 1. RECONOCIMIENTO DE FAMILIA (Rigor científico: Firma química/ID)
            if (otraVida.misStats.idLinaje == this.sistemaVida.misStats.idLinaje)
            {
                // Si es familia, ignoramos el choque físico brusco para evitar el efecto Pinball
                // Se deslizarán gracias al OnTriggerStay.
                return;
            }

            // 2. DEPREDACIÓN (El grande se come al chico)
            // Si soy un 20% más grande que la otra, la devoro
            if (this.sistemaVida.misStats.tamano > otraVida.misStats.tamano * 1.2f)
            {
                this.sistemaVida.Alimentar(otraVida.EnergiaActual + 10f);
                otraVida.SendMessage("Morir"); // Enviamos señal de muerte a la otra
                return;
            }

            // 3. REACCIÓN DE EVITACIÓN (Solo con extraños de tamaño similar)
            if (Time.time > tiempoSiguienteChoque)
            {
                ReaccionarSusto(colision.transform.position);
            }
        }
        else // CHOQUE CON PAREDES
        {
            Vector2 normalPared = colision.contacts[0].normal;
            direccionObjetivo = Vector2.Reflect(direccionObjetivo, normalPared).normalized;
            if (estadoActual == EstadoBacteria.Persiguiendo) estadoActual = EstadoBacteria.Vagando;
            tiempoSiguienteChoque = Time.time + cooldownChoque;
        }
    }

    private void ReaccionarSusto(Vector3 posicionAmenaza)
    {
        Vector2 direccionHuida = (transform.position - posicionAmenaza).normalized;
        direccionObjetivo = Quaternion.Euler(0, 0, Random.Range(-45, 45)) * direccionHuida;
        miCuerpoFisico.AddForce(direccionHuida * fuerzaRepulsion, ForceMode2D.Impulse);
        sistemaVida.EnergiaActual -= 0.5f;
        tiempoSiguienteChoque = Time.time + cooldownChoque;
    }

    private void OnTriggerStay2D(Collider2D otro)
    {
        if (otro.CompareTag("Bacteria"))
        {
            SistemaVida otraVida = otro.GetComponent<SistemaVida>();
            if (otraVida != null && otraVida.misStats.idLinaje == this.sistemaVida.misStats.idLinaje)
            {
                // Fuerza de separación muy débil para familia (se permiten solapar más)
                AplicarSeparacion(otro.transform.position, fuerzaSeparacion * 0.5f);
            }
            else
            {
                // Fuerza de separación normal para extraños (Quimiotaxis negativa)
                AplicarSeparacion(otro.transform.position, fuerzaSeparacion);
            }
        }
    }

    private void AplicarSeparacion(Vector3 posicionOtro, float intensidadBase)
    {
        Vector2 direccionEmpuje = transform.position - posicionOtro;
        float intensidad = intensidadBase / (direccionEmpuje.magnitude + 0.1f);
        miCuerpoFisico.AddForce(direccionEmpuje.normalized * intensidad);
    }

    private void LogicaVagar()
    {
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0) CambiarDireccion();

        if (sistemaVida.EnergiaActual >= 90) estadoActual = EstadoBacteria.Reproduciendo;
        else if (sistemaVida.EnergiaActual <= 80 && misOjos.objetivoMasCercano != null) estadoActual = EstadoBacteria.Persiguiendo;
    }

    private void LogicaPerseguir()
    {
        if (sistemaVida.EnergiaActual >= 90) { estadoActual = EstadoBacteria.Reproduciendo; return; }
        if (misOjos.objetivoMasCercano == null) { estadoActual = EstadoBacteria.Vagando; return; }

        Vector2 direccionAComida = misOjos.objetivoMasCercano.position - transform.position;
        direccionObjetivo = direccionAComida.normalized;
        tiempoRestante = tiempoEntreCambios;
    }

    private void LogicaReproducirse()
    {
        sistemaVida.Reproducir(50);
        estadoActual = EstadoBacteria.Vagando;
    }
}