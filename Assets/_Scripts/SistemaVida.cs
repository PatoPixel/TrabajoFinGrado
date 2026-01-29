using System;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    public DatosGeneticos misStats;
    private bool reproduciendose = false;

    [Header("Monitor")]
    [SerializeField] private float energiaActual;
    public float EnergiaActual { get => energiaActual; set => energiaActual = value; }

    void Start()
    {
        // 1. Si los stats están a 0, inicializamos con valores base
        if (misStats.idLinaje == 0)
        {
            AsignarStatsBase();

            // Ahora usamos la propiedad Instance que es más inteligente
            if (GestorLinajes.Instance != null)
            {
                misStats.idLinaje = GestorLinajes.Instance.ObtenerNuevoId();
                misStats.generaciones = 1;
                gameObject.name = $"Linaje_{misStats.idLinaje}_Fundadora";
                Debug.Log($"<color=cyan>ID asignado correctamente a {gameObject.name}</color>");
            }
            else
            {
                // Este es el último recurso si el objeto NO existe en la escena
                Debug.LogError("CRÍTICO: El objeto GestorLinajes no existe en la jerarquía. ¡Créalos!");
            }
        }
        // 2. IMPORTANTE: Sincronizar energía y componentes
        energiaActual = misStats.energiaMax * 0.7f; // Nacen con un 70% de energía
    }
    private void AsignarStatsBase()
    {
        misStats.generaciones = 1;
        misStats.velocidad = 3.5f;
        misStats.radioVision = 5f;
        misStats.energiaMax = 100f;
        misStats.consumo = 1.5f;
        misStats.tamano = 1f;
        misStats.rangoMutacion = 0.15f;
        misStats.vidaUtil = 60f;        
    }
    void Update()
    {
        energiaActual -= misStats.consumo * Time.deltaTime;

        if (energiaActual <= 0)
        {
            Morir();
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
            GameObject hijo = Instantiate(gameObject, transform.position, Quaternion.identity);

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

        // 2. Aplicamos cambios físicos y visuales
        transform.localScale = Vector3.one * misStats.tamano;
        gameObject.name = $"Linaje_{misStats.idLinaje}_Gen_{misStats.generaciones}";

        Invoke("ResetReproduccion", 5f);
    }
    private void ResetReproduccion() => reproduciendose = false;
    private DatosGeneticos MutarGenes(DatosGeneticos padre)
    {
        float var = padre.rangoMutacion;

        // Creamos el nuevo struct con los datos mutados
        DatosGeneticos hijo = new DatosGeneticos
        {
            idLinaje = padre.idLinaje, // Mantenemos familia
            generaciones = padre.generaciones + 1, // Subimos generación

            velocidad = padre.velocidad * UnityEngine.Random.Range(1 - var, 1 + var),
            energiaMax = padre.energiaMax * UnityEngine.Random.Range(1 - var, 1 + var),
            consumo = padre.consumo * UnityEngine.Random.Range(1 - var, 1 + var),
            tamano = padre.tamano * UnityEngine.Random.Range(1 - var, 1 + var),
            radioVision = padre.radioVision * UnityEngine.Random.Range(1 - var, 1 + var),

            rangoMutacion = padre.rangoMutacion // Se mantiene o podría mutar también
        };

        return hijo;
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