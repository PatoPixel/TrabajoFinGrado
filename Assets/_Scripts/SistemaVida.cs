using System;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    [Header("Configuración de Energía")]
    [SerializeField] private float energiaMaxima = 100f;
    [SerializeField] private float consumoPorSegundo = 2.5f;

    [Header("Genética")]
    [SerializeField, Range(0f, 0.5f)] private float rangoMutacion = 0.1f; // 10% de variación

    private bool reproduciendose = false;

    [Header("Monitor")]
    [SerializeField] private float energiaActual;
    public float EnergiaActual { get => energiaActual; set => energiaActual = value; }

    // Referencias a mis propios componentes para leer mis stats actuales
    private MovimientoAleatorio miMovimiento;
    private SensorBacteria miSensor;

    void Start()
    {
        // Cacheamos los componentes
        miMovimiento = GetComponent<MovimientoAleatorio>();
        miSensor = GetComponent<SensorBacteria>();

        if (energiaActual <= 0)
        {
            energiaActual = energiaMaxima * 0.5f;
        }
    }

    void Update()
    {
        energiaActual -= consumoPorSegundo * Time.deltaTime;

        if (energiaActual <= 0)
        {
            Morir();
        }
    }

    public void Alimentar(float cantidad)
    {
        energiaActual += cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0, energiaMaxima);
        reproduciendose = false;
    }

    public void Reproducir(float costoReproduccion)
    {
        energiaActual -= costoReproduccion;

        if (!reproduciendose)
        {
            // 1. Instanciamos al hijo
            GameObject hijo = Instantiate(gameObject, transform.position, Quaternion.identity);

            // 2. Obtenemos TODOS los componentes del hijo que queremos mutar
            SistemaVida vidaHijo = hijo.GetComponent<SistemaVida>();
            MovimientoAleatorio movHijo = hijo.GetComponent<MovimientoAleatorio>();
            SensorBacteria sensorHijo = hijo.GetComponent<SensorBacteria>();

            // 3. Obtenemos los valores actuales del PADRE (nosotros)
            float velPadre = miMovimiento.Velocidad;
            float visionPadre = miSensor.radioDeteccion;
            string nombrePadre = gameObject.name; // Ej: "Bacteria_Gen_5"
            string[] partesNombre = nombrePadre.Split('_');

            // 4. Ejecutamos la mutación en el hijo pasándole todos los datos del padre
            if (vidaHijo != null)
            {

                vidaHijo.Mutar(energiaMaxima, consumoPorSegundo, velPadre, visionPadre, partesNombre[0], int.Parse(partesNombre[1]), rangoMutacion, movHijo, sensorHijo);
            }

            reproduciendose = true;
        }
    }

    // --- FUNCIÓN DE MUTACIÓN EXPANDIDA ---
    public void Mutar(float maxPadre, float consumoPadre, float velPadre, float visionPadre,String nombrePadre, int generacion, float variacion,
                      MovimientoAleatorio scriptMovimiento, SensorBacteria scriptSensor)
    {
        // A. Cálculamos factores aleatorios (ej: entre 0.9 y 1.1)
        float genVida = UnityEngine.Random.Range(1f - variacion, 1f + variacion);
        float genConsumo = UnityEngine.Random.Range(1f - variacion, 1f + variacion);
        float genVelocidad = UnityEngine.Random.Range(1f - variacion, 1f + variacion);
        float genVision = UnityEngine.Random.Range(1f - variacion, 1f + variacion);

        // B. Aplicamos mutaciones a ESTE script (SistemaVida)
        this.energiaMaxima = maxPadre * genVida;
        this.consumoPorSegundo = consumoPadre * genConsumo;

        // C. Aplicamos mutaciones a los OTROS scripts del hijo
        if (scriptMovimiento != null)
        {
            scriptMovimiento.Velocidad = velPadre * genVelocidad;
        }

        if (scriptSensor != null)
        {
            scriptSensor.radioDeteccion = visionPadre * genVision;
        }

        // D. Configuración inicial del nacimiento
        this.reproduciendose = false;
        this.energiaActual = this.energiaMaxima * 0.5f;
        this.gameObject.name = nombrePadre + "_" + (generacion + 1);

        // Debug para ver qué pasó
        Debug.Log($"Nació hijo mutante: Vel {scriptMovimiento.Velocidad} (Padre: {velPadre}) | Visión {scriptSensor.radioDeteccion}");
    }

    private void Morir()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        if (otroObjeto.CompareTag("Comida"))
        {
            Alimentar(40f);
            Destroy(otroObjeto.gameObject);
        }
    }
}