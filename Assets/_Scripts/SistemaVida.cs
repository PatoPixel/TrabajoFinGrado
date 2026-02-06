using System;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    public DatosGeneticos misStats;
    private bool reproduciendose = false;

    [Header("Monitor")]
    [SerializeField] private string nombreDePila;
    [SerializeField] private float energiaActual;
    [SerializeField] private float edadActual;
    public float EnergiaActual { get => energiaActual; set => energiaActual = value; }

    void Start()
    {
        if (string.IsNullOrEmpty(nombreDePila))
        {
            nombreDePila = gameObject.name;
        }
        // 1. Si los stats están a 0, inicializamos con valores base
        if (misStats.idLinaje == 0)
        {
            AsignarStatsBase();

            // Ahora usamos la propiedad Instance que es más inteligente
            if (GestorLinajes.Instance != null)
            {
                misStats.idLinaje = GestorLinajes.Instance.ObtenerNuevoId();
                misStats.generaciones = 1;

            }
            else
            {
                // Este es el último recurso si el objeto NO existe en la escena
                Debug.LogError("CRÍTICO: El objeto GestorLinajes no existe en la jerarquía.");
            }
        }
        // 2. IMPORTANTE: Sincronizar energía y componentes
        energiaActual = misStats.energiaMax * 0.7f; // Nacen con un 70% de energía

    }
    public void AsignarStatsBase()
    {
        misStats.generaciones = 1;
        misStats.velocidad = 2f;
        misStats.radioVision = 2f;
        misStats.energiaMax = 100f;
        misStats.tamano = 1f;
        misStats.rangoMutacion = 0.10f;
        misStats.vidaUtil = 60f;  
        misStats.consumo = ((float) Math.Pow(misStats.tamano, 3f) + (float) Math.Pow(misStats.velocidad, 2) + misStats.radioVision)/4;
        if (misStats.colorLinaje.a == 0)
        {
            misStats.colorLinaje = GetComponent<SpriteRenderer>().color;
        }

    }
    void Update()
    {
        energiaActual -= misStats.consumo * Time.deltaTime;

        edadActual += Time.deltaTime;
        if (energiaActual <= 0)
        {
            Morir();
        }
        if (edadActual > misStats.vidaUtil)
        {
            // Ejecutamos el azar solo una vez por segundo, no por frame, para optimizar
            if (CheckMuerteNatural()) Morir();
        }
    }

    public void Alimentar(float cantidad)
    {
        energiaActual += cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0, misStats.energiaMax);
        reproduciendose = false;
    }

    public void Reproducir(float costoReproduccion)
    {
        energiaActual -= costoReproduccion;

        if (!reproduciendose)
        {
            GameObject hijo = BacteriasMuertas.Instance.GetBacteria(transform.position);

            SistemaVida vidaHijo = hijo.GetComponent<SistemaVida>();

            if (vidaHijo != null)
            {
                vidaHijo.InicializarHijo(misStats);
            }
            reproduciendose = true;
        }
    }

    public void InicializarHijo(DatosGeneticos genesPadre)
    {
        // 1. Generamos el nuevo paquete de ADN
        this.misStats = MutarGenes(genesPadre);
        this.energiaActual = misStats.energiaMax * 0.5f;
        this.reproduciendose = true;
        transform.localScale = Vector3.one * misStats.tamano;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = misStats.colorLinaje;
        }
        // 2. Aplicamos cambios físicos y visuales
        string nombreBase = GestorLinajes.Instance.GetNombrePorId(misStats.idLinaje);
        gameObject.name = $"{nombreBase} (Gen {misStats.generaciones})";
        edadActual = 0;
        Invoke("ResetReproduccion", 5f);
    }
    private void ResetReproduccion() => reproduciendose = false;
    private DatosGeneticos MutarGenes(DatosGeneticos padre)
    {
        float var = padre.rangoMutacion;

        // 1. Calculamos primero los valores mutados de forma local
        float nVelocidad = padre.velocidad * UnityEngine.Random.Range(1 - var, 1 + var);
        float nTamano = padre.tamano * UnityEngine.Random.Range(1 - var / 2, 1 + var / 2);
        float nRadio = padre.radioVision * UnityEngine.Random.Range(1 - var, 1 + var);
        float nVidaUtil = padre.vidaUtil * UnityEngine.Random.Range(1 - var, 1 + var);
        // 2. Calculamos el consumo usando los NUEVOS valores locales
        // Así evitamos usar 'misStats' del padre por error
        float nConsumo = DatosGeneticos.CalcularGasto(nTamano, nVelocidad, nRadio);

        // 3. Empaquetamos en el nuevo struct
        return new DatosGeneticos
        {
            idLinaje = padre.idLinaje,
            generaciones = padre.generaciones + 1,
            velocidad = nVelocidad,
            tamano = nTamano,
            radioVision = nRadio,
            consumo = nConsumo, // Asignación limpia
            vidaUtil = nVidaUtil,
            energiaMax = padre.energiaMax * UnityEngine.Random.Range(1 - var, 1 + var),
            rangoMutacion = padre.rangoMutacion,
            colorLinaje = padre.colorLinaje
        };
    }



    private void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        if (otroObjeto.CompareTag("Comida"))
        {
            Alimentar(40f);
            Destroy(otroObjeto.gameObject);
        }
    }

    private bool CheckMuerteNatural()
    {
        // Calculamos qué tan "viejo" es respecto a su límite (ej: 1.2 es un 20% extra)
        float factorExceso = edadActual / misStats.vidaUtil;

        // Probabilidad base que sube con el tiempo
        float probabilidad = (factorExceso - 1) * 0.1f; // Ajusta el 0.1f a tu gusto

        return UnityEngine.Random.value < (probabilidad * Time.deltaTime);
    }



    public void Morir()
    {
        CancelInvoke(); // Detenemos cualquier proceso pendiente (como la reproducción)
        gameObject.SetActive(false);
        BacteriasMuertas.Instance.bacteriasMuertas.Push(gameObject);
    }
    public void OnEnable()
    {
        if (!GestorLinajes.RegistroVida.ContainsKey(gameObject.GetInstanceID()))
        {
            GestorLinajes.RegistroVida[gameObject.GetInstanceID()] = this;
        }
    }
    public void OnDisable()
    {
        GestorLinajes.RegistroVida.Remove(gameObject.GetInstanceID());
    }
}