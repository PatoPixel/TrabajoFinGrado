using System;
using UnityEngine;

public class SistemaVida : MonoBehaviour
{
    public DatosGeneticos misStats;
    private float factorRitmo = 5f;

    [Header("Monitor")]
    [SerializeField] private string nombreDePila;
    [SerializeField] private float energiaActual;
    [SerializeField] private float edadActual;
    [SerializeField] private float cooldownRestante;
    public String NombreDePila { get => nombreDePila; set => nombreDePila = value; }
    public float EnergiaActual { get => energiaActual; set => energiaActual = value; }
    public float EdadActual { get => edadActual; set => edadActual = value; }
    public float CooldownRestante { get => cooldownRestante; set => cooldownRestante = value; }
    public float MultiplicadorActividad { get; set; } = 1f;
    void Start()
    {
        if (string.IsNullOrEmpty(nombreDePila))
        {
            nombreDePila = gameObject.name;
        }
        // 1. Si los stats est�n a 0, inicializamos con valores base
        if (misStats.idLinaje == 0)
        {
            AsignarStatsBase();

            // Ahora usamos la propiedad Instance que es m�s inteligente
            if (GestorLinajes.Instance != null)
            {
                misStats.idLinaje = GestorLinajes.Instance.ObtenerNuevoId();
                misStats.generaciones = 1;

            }
            else
            {
                // Este es el �ltimo recurso si el objeto NO existe en la escena
                Debug.LogError("CR�TICO: El objeto GestorLinajes no existe en la jerarqu�a.");
            }
        } 
         // 2. IMPORTANTE: Sincronizar energ�a y componentes
         energiaActual = misStats.energiaMax * 0.7f; // Nacen con un 70% de energ�a

    }
    public void AsignarStatsBase()
    {
        misStats.generaciones = 1;
        misStats.velocidad = 2f;
        misStats.radioVision = 2f;
        misStats.energiaMax = 100f;
        misStats.tamano = 1f;
        misStats.rangoMutacion = 0.10f;
        misStats.vidaUtil = 100f / (((float)Math.Pow(misStats.tamano, 3f) + (float)Math.Pow(misStats.velocidad, 2) + misStats.radioVision) / 4);
        misStats.consumo = ((float) Math.Pow(misStats.tamano, 3f) + (float) Math.Pow(misStats.velocidad, 2) + misStats.radioVision)/4;
        misStats.tiempreEntreReproduccion = (misStats.energiaMax / misStats.vidaUtil) * factorRitmo;
        cooldownRestante = misStats.tiempreEntreReproduccion;
        if (misStats.colorLinaje.a == 0)
        {
            misStats.colorLinaje = GetComponent<SpriteRenderer>().color;
        }
    }

    public void AsignarStatsLoad(DatosEntidad datosEntidadLoad)
    {
        this.misStats = new DatosGeneticos
        {
            idLinaje = datosEntidadLoad.idLinaje,
            generaciones = datosEntidadLoad.generaciones,
            velocidad = datosEntidadLoad.velocidad,
            radioVision = datosEntidadLoad.radioVision,
            energiaMax = datosEntidadLoad.energiaMax,
            consumo = datosEntidadLoad.consumo,
            tamano = datosEntidadLoad.tamano,
            vidaUtil = datosEntidadLoad.vidaUtil,
            rangoMutacion = datosEntidadLoad.rangoMutacion,
            tiempreEntreReproduccion = datosEntidadLoad.tiempreEntreReproduccion,
            colorLinaje = datosEntidadLoad.colorLinaje
        };

        this.nombreDePila = datosEntidadLoad.nombreDePila;
        this.energiaActual = datosEntidadLoad.energiaActual;
        this.edadActual = datosEntidadLoad.edadActual;
        this.cooldownRestante = datosEntidadLoad.cooldownRestante;
        this.transform.position = new Vector3(datosEntidadLoad.posX, datosEntidadLoad.posY, 0);
        this.transform.rotation = Quaternion.Euler(0, 0, datosEntidadLoad.quaternionZ);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = this.misStats.colorLinaje;
        }
        gameObject.name = this.nombreDePila;
        this.transform.localScale = Vector3.one * this.misStats.tamano;
    }
    void Update()
    {
        float dt = Time.deltaTime;
        energiaActual -= misStats.consumo * MultiplicadorActividad * dt;
        edadActual += dt;

        if (energiaActual <= 0 || edadActual > misStats.vidaUtil)
        {
            if (energiaActual <= 0 || CheckMuerteNatural()) Morir();
        }

        if (cooldownRestante > 0)
        {
            cooldownRestante -= dt;
        }
    }

    public void Alimentar(float cantidad)
    {
        energiaActual += cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0, misStats.energiaMax);
    }

    public void Reproducir()
    {
        // Condición de seguridad: Energía suficiente Y cooldown agotado
        float costoReproduccion = DatosGeneticos.CalcularCosteReproduccion(misStats.consumo, misStats.tiempreEntreReproduccion);

        if (cooldownRestante <= 0 && energiaActual >= costoReproduccion)
        {

            energiaActual -= costoReproduccion;

            // Bloqueamos la reproducción inmediatamente (Cooldown de seguridad)
            cooldownRestante = misStats.tiempreEntreReproduccion;

            GameObject hijo = BacteriasMuertas.Instance.GetBacteria(transform.position);
            if (hijo != null && hijo.TryGetComponent(out SistemaVida vidaHijo))
            {
                vidaHijo.InicializarHijo(misStats);
            }
        }
    }

    public void InicializarHijo(DatosGeneticos genesPadre)
    {
        // 1. Generamos el nuevo paquete de ADN
        this.misStats = MutarGenes(genesPadre);
        this.energiaActual = misStats.energiaMax * 0.5f;
        this.cooldownRestante = misStats.tiempreEntreReproduccion;
        transform.localScale = Vector3.one * misStats.tamano;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = misStats.colorLinaje;
        }
        // 2. Aplicamos cambios f�sicos y visuales
        string nombreBase = GestorLinajes.Instance.GetNombrePorId(misStats.idLinaje);
        gameObject.name = $"{nombreBase} (Gen {misStats.generaciones})";
        edadActual = 0;
    }
    private DatosGeneticos MutarGenes(DatosGeneticos padre)
    {
        float var = padre.rangoMutacion;

        // 1. Calculamos primero los valores mutados de forma local
        float nVelocidad = padre.velocidad * UnityEngine.Random.Range(1 - var, 1 + var);
        float nTamano = padre.tamano * UnityEngine.Random.Range(1 - var / 2, 1 + var / 2);
        float nRadio = padre.radioVision * UnityEngine.Random.Range(1 - var, 1 + var);
        // 2. Calculamos el consumo usando los NUEVOS valores locales
        // As� evitamos usar 'misStats' del padre por error
        float nConsumo = DatosGeneticos.CalcularGasto(nTamano, nVelocidad, nRadio);
        float nVidaUtil = (nTamano * nTamano * 100) / nConsumo;
        float nEnergiaMax = nTamano * 100;
        float nCooldown = (nEnergiaMax / nVidaUtil) * factorRitmo;
        // 3. Empaquetamos en el nuevo struct
        return new DatosGeneticos
        {
            idLinaje = padre.idLinaje,
            generaciones = padre.generaciones + 1,
            velocidad = nVelocidad,
            tamano = nTamano,
            radioVision = nRadio,
            consumo = nConsumo,
            vidaUtil = nVidaUtil,
            energiaMax = nEnergiaMax,
            tiempreEntreReproduccion = nCooldown,
            rangoMutacion = padre.rangoMutacion,
            colorLinaje = padre.colorLinaje
        };
    }



    private void OnTriggerEnter2D(Collider2D otroObjeto)
    {
        if (otroObjeto.CompareTag("Comida"))
        {
            Comida comida = otroObjeto.GetComponent<Comida>();
            float energia = comida != null ? comida.Energia : 40f;
            Alimentar(energia);
            comida?.Devolver();
        }
    }

    private bool CheckMuerteNatural()
    {
        // Calculamos qu� tan "viejo" es respecto a su l�mite (ej: 1.2 es un 20% extra)
        float factorExceso = edadActual / misStats.vidaUtil;

        // Probabilidad base que sube con el tiempo
        float probabilidad = (factorExceso - 1) * 0.1f; // Ajusta el 0.1f a tu gusto

        return UnityEngine.Random.value < (probabilidad * Time.deltaTime);
    }


    public void Morir()
    {
        CancelInvoke();
        SoltarNutrientes();
        gameObject.SetActive(false);
        BacteriasMuertas.Instance.bacteriasMuertas.Push(gameObject);
    }

    /// <summary>
    /// Releases nutrients proportional to this bacteria's tamano when it dies.
    /// </summary>
    private void SoltarNutrientes()
    {
        if (PoolComida.Instance == null) return;

        int cantidad = Mathf.Max(1, Mathf.RoundToInt(misStats.tamano * 2f));
        float tamanoChunk = misStats.tamano / cantidad;

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.2f, 0.2f),
                UnityEngine.Random.Range(-0.2f, 0.2f),
                0f);

            GameObject obj = PoolComida.Instance.GetComida(transform.position + offset, tamanoChunk);
            obj.GetComponent<Comida>()?.Inicializar(tamanoChunk);
        }
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

    public void Purga()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        BacteriasMuertas.Instance.bacteriasMuertas.Push(gameObject);
    }
}