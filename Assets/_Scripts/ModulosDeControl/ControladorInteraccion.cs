using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControladorInteraccion : MonoBehaviour
{
    public enum ModoRaton { Moverse, Inspeccion, Creador, Herramientas }
    public enum TipoHerramienta { Ninguna, PincelComida, PincelSpawner }

    [Header("Estado Actual")]
    public ModoRaton modoActual = ModoRaton.Moverse;
    public int idEspecieSeleccionada = -1;

    [Header("Herramienta Actual")]
    public TipoHerramienta herramientaSeleccionada = TipoHerramienta.Ninguna;

    [Header("Referencias UI")]
    public PanelInspectorBacteria inspectorUI;
    public EvolutionTracker trackerEvolucion;
    public ControladorMenuPausa menuPrincipal;
    public PanellCreacion panelLaboratorio;

    public GameObject panelSelector;
    public BandejaEspeciesUI bandejaDinamica;

    [Header("Botones de Herramientas")]
    public Button botonMenu;
    public Button botonMover;
    public Button botonInspeccionar;
    public Button botonCrear;
    public Button botonHerramientas;

    [Header("Ajustes de Fisicas")]
    public float radioDeSeguridad = 1f;
    public float energiaPincelComida = 50f;

    [Header("Ajustes Pincel Spawner")]
    public GameObject prefabSpawnerComida;
    private float _spawnerMinEnergia = 30f;
    private float _spawnerMaxEnergia = 70f;
    private float _spawnerIntervalo = 0.2f;
    private float _spawnerRadio = 1.5f;

    /// <summary>Eventos para el TutorialManager.</summary>
    public static event System.Action OnCamaraCentrada;
    public static event System.Action OnBacteriaColocada;
    public static event System.Action OnComidaColocada;
    public static event System.Action OnSpawnerColocado;

    private void Start()
    {
        ActualizarVisualBotones();
        if (panelSelector != null) panelSelector.SetActive(false);
        if (panelLaboratorio != null) panelLaboratorio.CerrarPanel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // En tutorial el overlay cubre la pantalla: ignoramos el bloqueo del EventSystem
            if (!TutorialManager.TutorialActivo && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (modoActual == ModoRaton.Inspeccion)
            {
                IntentarInspeccionar(posicionRaton);
            }
            else if (modoActual == ModoRaton.Creador)
            {
                Collider2D[] colisiones = Physics2D.OverlapCircleAll(posicionRaton, radioDeSeguridad);
                bool espacioOcupado = false;

                foreach (Collider2D col in colisiones)
                {
                    if (col.GetComponent<SistemaVida>() != null)
                    {
                        espacioOcupado = true;
                        break;
                    }
                }

                if (espacioOcupado)
                {
                    Debug.Log("[Anti-Apilamiento] Espacio ocupado. Busca un hueco libre.");
                    return;
                }

                GenerarBacteria(posicionRaton);
            }
            else if (modoActual == ModoRaton.Herramientas)
            {
                if (herramientaSeleccionada == TipoHerramienta.PincelComida)
                {
                    GenerarComidaManual(posicionRaton);
                }
                else if (herramientaSeleccionada == TipoHerramienta.PincelSpawner)
                {
                    GenerarSpawnerManual(posicionRaton);
                }
            }
        }
    }

    private void GenerarComidaManual(Vector2 posicion)
    {
        if (PoolComida.Instance != null)
        {
            GameObject nuevaComida = PoolComida.Instance.GetComida(posicion, 0.5f);
            if (nuevaComida != null && nuevaComida.TryGetComponent(out Comida scriptComida))
            {
                scriptComida.EstablecerEnergiaManual(energiaPincelComida);
                OnComidaColocada?.Invoke();
            }
        }
    }

    private void GenerarSpawnerManual(Vector2 posicion)
    {
        if (prefabSpawnerComida != null)
        {
            GameObject spawnerObj = Instantiate(prefabSpawnerComida, posicion, Quaternion.identity);
            if (spawnerObj.TryGetComponent(out SpawnerComida scriptSpawner))
            {
                scriptSpawner.Inicializar(_spawnerMinEnergia, _spawnerMaxEnergia, _spawnerIntervalo, _spawnerRadio);
                OnSpawnerColocado?.Invoke();
            }
        }
    }

    private void IntentarInspeccionar(Vector2 posicion)
    {
        RaycastHit2D hit = Physics2D.Raycast(posicion, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Bacteria"))
        {
            SistemaVida bacteria = hit.collider.GetComponent<SistemaVida>();
            if (bacteria != null)
            {
                if (trackerEvolucion != null) trackerEvolucion.CambiarFamiliaSeleccionada(bacteria.misStats.idLinaje);
                if (inspectorUI != null) inspectorUI.Abrir(bacteria);
            }
        }
        else
        {
            if (inspectorUI != null) inspectorUI.Cerrar();
        }
    }

    private void GenerarBacteria(Vector2 posicion)
    {
        // Sin especie seleccionada no se puede crear — hay que diseñar una en el laboratorio primero
        if (idEspecieSeleccionada == -1)
        {
            Debug.LogWarning("[Laboratorio] Selecciona una especie en la bandeja antes de colocar bacterias.");
            return;
        }

        GameObject nuevaBacteria = BacteriasMuertas.Instance.GetBacteria(posicion);

        if (nuevaBacteria != null && nuevaBacteria.TryGetComponent(out SistemaVida vida))
        {
            if (GestorLinajes.Instance != null)
            {
                {
                    DatosGeneticos plantillaAdn = GestorLinajes.Instance.ObtenerPlantilla(idEspecieSeleccionada);
                    vida.misStats = plantillaAdn;
                    vida.EdadActual = 0f;
                    vida.CooldownRestante = plantillaAdn.tiempreEntreReproduccion;
                    vida.transform.localScale = Vector3.one * plantillaAdn.tamano;

                    SpriteRenderer sr = vida.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = plantillaAdn.colorLinaje;

                    string nombreReal = GestorLinajes.Instance.GetNombrePorId(idEspecieSeleccionada);
                    vida.gameObject.name = $"{nombreReal} (Clon Manual)";
                }
                vida.EnergiaActual = vida.misStats.energiaMax;
                OnBacteriaColocada?.Invoke();
            }
        }
    }

    public void SeleccionarPincelEspecie(int idLinaje)
    {
        idEspecieSeleccionada = idLinaje;
    }

    public void ConfigurarHerramientaComida(int energia)
    {
        energiaPincelComida = energia;
        herramientaSeleccionada = TipoHerramienta.PincelComida;
    }

    public void ConfigurarHerramientaSpawner(float minEnergia, float maxEnergia, float intervalo, float radio)
    {
        _spawnerMinEnergia = minEnergia;
        _spawnerMaxEnergia = maxEnergia;
        _spawnerIntervalo = intervalo;
        _spawnerRadio = radio;
        herramientaSeleccionada = TipoHerramienta.PincelSpawner;
    }

    public void ActivarMenu()
    {
        menuPrincipal.AlternarPausa();
        ActualizarVisualBotones();
    }

    public void ActivarModoMoverse()
    {
        modoActual = ModoRaton.Moverse;
        herramientaSeleccionada = TipoHerramienta.Ninguna;
        ActualizarVisualBotones();
    }

    public void ActivarModoInspeccion()
    {
        modoActual = ModoRaton.Inspeccion;
        herramientaSeleccionada = TipoHerramienta.Ninguna;
        ActualizarVisualBotones();
    }

    public void ActivarModoCreador()
    {
        if (modoActual == ModoRaton.Creador)
        {
            ActivarModoMoverse();
            return;
        }

        modoActual = ModoRaton.Creador;
        idEspecieSeleccionada = -1;
        herramientaSeleccionada = TipoHerramienta.Ninguna;
        ActualizarVisualBotones();

        if (bandejaDinamica != null) bandejaDinamica.RedibujarBandeja();
    }

    public void ActivarModoHerramientas()
    {
        if (modoActual == ModoRaton.Herramientas)
        {
            ActivarModoMoverse();
            return;
        }

        modoActual = ModoRaton.Herramientas;
        herramientaSeleccionada = TipoHerramienta.Ninguna;
        ActualizarVisualBotones();

        if (bandejaDinamica != null) bandejaDinamica.RedibujarBandeja();
    }

    public void AbrirLaboratorio()
    {
        if (panelLaboratorio != null) panelLaboratorio.AbrirPanel();
    }

    private void ActualizarVisualBotones()
    {
        PintarBotonActivo(botonMenu, false);
        PintarBotonActivo(botonMover, modoActual == ModoRaton.Moverse);
        PintarBotonActivo(botonInspeccionar, modoActual == ModoRaton.Inspeccion);
        PintarBotonActivo(botonCrear, modoActual == ModoRaton.Creador);
        PintarBotonActivo(botonHerramientas, modoActual == ModoRaton.Herramientas);

        if (panelSelector != null)
        {
            bool mostrarPanel = (modoActual == ModoRaton.Creador || modoActual == ModoRaton.Herramientas);
            panelSelector.SetActive(mostrarPanel);
        }
    }

    private void PintarBotonActivo(Button boton, bool estaActivo)
    {
        if (boton == null) return;
        ColorBlock colores = boton.colors;
        colores.normalColor = estaActivo ? new Color(0f, 0f, 0f, 0.4f) : new Color(0f, 0f, 0f, 0f);
        boton.colors = colores;
    }

    public void CentrarCamaraEnMapa()
    {
        if (Camera.main != null)
        {
            float z = Camera.main.transform.position.z;
            Camera.main.transform.position = new Vector3(0f, 0f, z);
            OnCamaraCentrada?.Invoke();
        }
    }
}
