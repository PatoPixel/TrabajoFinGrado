using UnityEngine;

/*
- Este script se encarga de controlar el movimiento de las bacterias, utilizando un sistema de estados para determinar su comportamiento en cada momento.
- Las bacterias pueden estar en uno de los siguientes estados: Vagando, Persiguiendo, Reproduciendo o Huyendo.
- En el estado de Vagando, la bacteria se mueve de forma aleatoria por el entorno, cambiando de dirección cada cierto tiempo o al chocar con una pared.
- En el estado de Persiguiendo, la bacteria se dirige hacia una fuente de comida que ha detectado, pero si detecta una amenaza cercana, cambia al estado de Huyendo.
- En el estado de Reproduciendo, la bacteria crea una copia de sí misma aplicandole mutaciones y luego vuelve al estado de Vagando.
- En el estado de Huyendo, la bacteria se aleja rápidamente de la amenaza detectada, pero si la amenaza desaparece, vuelve al estado de Vagando.
- El script también incluye lógica para manejar colisiones con otras bacterias, determinando si debe comer a la otra bacteria 
(si es de un linaje diferente y es significativamente más pequeña) o simplemente cambiar de dirección para evitar el choque.
- Además, se aplica una fuerza de separación para evitar que las bacterias se amontonen unas encima de otras, especialmente si son del mismo linaje.
*/

public class MovimientoAleatorio : MonoBehaviour
{
    public enum EstadoBacteria { Vagando, Persiguiendo, Reproduciendo, Huyendo }

    [SerializeField] private float tiempoEntreCambios = 2f;
    [SerializeField] private float cooldownChoque = 0.5f;
    [SerializeField] private float giroMaximoPorSegundo = 180f;
    [SerializeField] private float suavizadoVelocidad = 2f;
    [SerializeField] private float fuerzaSeparacion = 1f;
    [SerializeField] private float costoReproducir = 50f;
    
    private Vector2 direccionObjetivo;
    private float tiempoSiguienteChoque;
    private Rigidbody2D miCuerpoFisico;
    private float tiempoRestante;
    private SensorBacteria misOjos;
    private SistemaVida sistemaVida;
    private EstadoBacteria estadoActual;
    private float anguloObjetivoCacheado; 

    void Start()
    {
        sistemaVida = GetComponent<SistemaVida>();
        if (sistemaVida == null) { enabled = false; return; }

        miCuerpoFisico = GetComponent<Rigidbody2D>();
        misOjos = GetComponent<SensorBacteria>();

        estadoActual = EstadoBacteria.Vagando;
        CambiarDireccion();
    }

    void Update()
    {
        // Elegimos que hacer segun como este la bacteria
        switch (estadoActual)
        {
            case EstadoBacteria.Vagando: LogicaVagar(); break;
            case EstadoBacteria.Persiguiendo: LogicaPerseguir(); break;
            case EstadoBacteria.Reproduciendo: LogicaReproducirse(); break;
            case EstadoBacteria.Huyendo: LogicaHuir(); break;
        }
    }

    void FixedUpdate()
    {
        // Girar la bacteria poco a poco hacia donde quiere ir
        float anguloActual = transform.eulerAngles.z;
        float diferencia = Mathf.Abs(Mathf.DeltaAngle(anguloActual, anguloObjetivoCacheado));
        float maxGiro = giroMaximoPorSegundo * Time.fixedDeltaTime;
        float anguloNuevo = Mathf.MoveTowardsAngle(anguloActual, anguloObjetivoCacheado, maxGiro);
        miCuerpoFisico.MoveRotation(anguloNuevo);

        // Si esta muy girada, que vaya mas lento, si esta recta, que acelere
        float factorVelocidad = Mathf.InverseLerp(180f, 0f, diferencia);

        // Modificamos la velocidad segun el estado actual
        float mult = 1.0f;   
        if (estadoActual == EstadoBacteria.Huyendo) mult = 1.5f;
        else if (estadoActual == EstadoBacteria.Persiguiendo) mult = 1.2f;

        sistemaVida.MultiplicadorActividad = mult;

        float veloFinal = sistemaVida.misStats.velocidad * sistemaVida.MultiplicadorActividad * factorVelocidad;
        Vector2 velocidadDeseada = (Vector2)transform.right * veloFinal;
        miCuerpoFisico.linearVelocity = Vector2.Lerp(miCuerpoFisico.linearVelocity, velocidadDeseada, Time.fixedDeltaTime * suavizadoVelocidad);


    }

    void CambiarDireccion()
    {
        // Elegimos un sitio nuevo al azar para ir
        direccionObjetivo = Random.insideUnitCircle.normalized;
        ActualizarAnguloCacheado();
        tiempoRestante = tiempoEntreCambios;
    }

    private void OnCollisionEnter2D(Collision2D colision)
    {
        if (colision.gameObject.CompareTag("Bacteria"))
        {
            int idOtro = colision.gameObject.GetInstanceID();
            // Miramos en la lista de "vivos" para ver quien es el otro
            if (GestorLinajes.RegistroVida.TryGetValue(idOtro, out SistemaVida otraVida))
            {
                // Si es de mi familia, pasamos de el
                if (otraVida.misStats.idLinaje == this.sistemaVida.misStats.idLinaje) return;

                // Si soy mas grande, me lo como
                if (this.sistemaVida.misStats.tamano > otraVida.misStats.tamano * 1.2f)
                {
                    this.sistemaVida.Alimentar(otraVida.EnergiaActual + 10f);
                    otraVida.Morir();
                    return;
                }

                // Si me choco con alguien que no es de mi familia y es mas o menos de mi tamaño simplemente voy a otro lado
                if (Time.time > tiempoSiguienteChoque) ChoqueConBacteriaMismoTamanno(colision.transform.position);
            }
        }
        else
        {
            // Si chocamos con una pared, rebotamos
            Vector2 normalPared = colision.contacts[0].normal;
            direccionObjetivo = Vector2.Reflect(direccionObjetivo, normalPared).normalized;
            ActualizarAnguloCacheado(); // Recalculamos el angulo tras rebotar
            if (estadoActual == EstadoBacteria.Persiguiendo) estadoActual = EstadoBacteria.Vagando;
            tiempoSiguienteChoque = Time.time + cooldownChoque;
        }
    }

    private void ChoqueConBacteriaMismoTamanno(Vector3 posicionAmenaza)
    {
        // Salimos corriendo para el lado contrario
        direccionObjetivo = (transform.position - posicionAmenaza).normalized;
        ActualizarAnguloCacheado();
        miCuerpoFisico.linearVelocity *= 0.5f;
        sistemaVida.EnergiaActual -= 2.5f;
        tiempoSiguienteChoque = Time.time + cooldownChoque;
    }

    private void OnTriggerStay2D(Collider2D otro)
    {
        // Esto es para que no se amontonen unas encima de otras
        if (otro.CompareTag("Bacteria"))
        {
            int idOtro = otro.gameObject.GetInstanceID();
            if (GestorLinajes.RegistroVida.TryGetValue(idOtro, out SistemaVida otraVida))
            {
                float fuerza = (otraVida.misStats.idLinaje == sistemaVida.misStats.idLinaje) ? fuerzaSeparacion * 0.5f : fuerzaSeparacion;
                AplicarSeparacion(otro.transform.position, fuerza);
            }
        }
    }

    private void AplicarSeparacion(Vector3 posicionOtro, float intensidadBase)
    {
        // Empujamos un poco a la otra bacteria para dejar espacio
        Vector2 direccionEmpuje = transform.position - posicionOtro;
        float intensidad = intensidadBase / (direccionEmpuje.sqrMagnitude + 0.1f);
        miCuerpoFisico.AddForce(direccionEmpuje.normalized * intensidad);
    }

    private void LogicaVagar()
    {
        // Caminar normal hasta que toque cambiar de direccion o pase algo
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0) CambiarDireccion();
        //Si vemos una amenaza, salimos rapido a huir
        if (misOjos.AmenazaCercana != null)
        {
            estadoActual = EstadoBacteria.Huyendo;
            return;
        }
        if (sistemaVida.EnergiaActual >= sistemaVida.misStats.energiaMax * 0.9f) estadoActual = EstadoBacteria.Reproduciendo;
        else if (sistemaVida.EnergiaActual <= sistemaVida.misStats.energiaMax * 0.8f && misOjos.comidaCercana != null) estadoActual = EstadoBacteria.Persiguiendo;
    }

    private void LogicaPerseguir()
    {
        // Si vemos comida o algo pequeño, vamos a por ello
        if (misOjos.AmenazaCercana != null)
        {
            estadoActual = EstadoBacteria.Huyendo;
            return;
        }
        if (sistemaVida.EnergiaActual >= sistemaVida.misStats.energiaMax * 0.9f) { estadoActual = EstadoBacteria.Reproduciendo; return; }
        if (misOjos.comidaCercana == null) { estadoActual = EstadoBacteria.Vagando; return; }

        direccionObjetivo = (misOjos.comidaCercana.position - transform.position).normalized;
        ActualizarAnguloCacheado();
    }

    private void LogicaReproducirse()
    {
        // Creamos una copia y volvemos a caminar
        sistemaVida.Reproducir(costoReproducir);
        estadoActual = EstadoBacteria.Vagando;
    }

    private void LogicaHuir()
    {
        // Si hay una amenaza, salimos corriendo, si no, volvemos a vagar
        if (misOjos.AmenazaCercana != null)
        {
            direccionObjetivo = (transform.position - misOjos.AmenazaCercana.position).normalized;
            ActualizarAnguloCacheado();
        }
        else
        {
            estadoActual = EstadoBacteria.Vagando;
        }
    }

    // Funcion pequeñita para no escribir lo mismo todo el rato
    private void ActualizarAnguloCacheado()
    {
        anguloObjetivoCacheado = Mathf.Atan2(direccionObjetivo.y, direccionObjetivo.x) * Mathf.Rad2Deg;
    }
}