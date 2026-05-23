using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControladorInteraccion : MonoBehaviour
{
    public enum ModoRaton { Moverse, Inspeccion, Creador }

    [Header("Estado Actual")]
    public ModoRaton modoActual = ModoRaton.Moverse;
    public int idEspecieSeleccionada = -1;

    [Header("Referencias UI")]
    public PanelInspectorBacteria inspectorUI;
    public EvolutionTracker trackerEvolucion;
    public ControladorMenuPausa menuPrincipal;
    public PanellCreacion panelLaboratorio;
    public GameObject panelSelectorEspecies;

    [Header("Botones de Herramientas")]
    public Button botonMenu;
    public Button botonMover;
    public Button botonInspeccionar;
    public Button botonCrear;

    private void Start()
    {
        ActualizarVisualBotones();

        if (panelSelectorEspecies != null) panelSelectorEspecies.SetActive(false);

        if (panelLaboratorio != null) panelLaboratorio.CerrarPanel();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 posicionRaton = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (modoActual == ModoRaton.Inspeccion)
            {
                IntentarInspeccionar(posicionRaton);
            }
            else if (modoActual == ModoRaton.Creador)
            {
                GenerarBacteria(posicionRaton);
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
                if (trackerEvolucion != null)
                {
                    trackerEvolucion.CambiarFamiliaSeleccionada(bacteria.misStats.idLinaje);
                }
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
        // 1. Sacamos una carcasa vacía del Pool de bacterias muertas
        GameObject nuevaBacteria = BacteriasMuertas.Instance.GetBacteria(posicion);

        if (nuevaBacteria != null && nuevaBacteria.TryGetComponent(out SistemaVida vida))
        {
            if (GestorLinajes.Instance != null)
            {
                if (idEspecieSeleccionada == -1)
                {
                    // --- MODO A: CEPA BASE POR DEFECTO (Si no hay nada seleccionado) ---
                    vida.misStats.idLinaje = 0;
                    vida.AsignarStatsBase();
                    vida.misStats.idLinaje = GestorLinajes.Instance.ObtenerNuevoId();
                    vida.gameObject.name = $"{GestorLinajes.Instance.GetNombrePorId(vida.misStats.idLinaje)} (Gen 1)";

                    Debug.Log($"¡Modo Dios: Cepa Base inyectada en {posicion}!");
                }
                else
                {
                    // --- MODO B: CLONAR ESPECIE DEL LABORATORIO ---
                    // extraemos el ADN exacto que guardamos en el laboratorio
                    DatosGeneticos plantillaAdn = GestorLinajes.Instance.ObtenerPlantilla(idEspecieSeleccionada);

                    // Se lo inyectamos directamente a la nueva bacteria
                    vida.misStats = plantillaAdn;

                    // Restauramos los valores dinámicos para que no nazca vieja o cansada
                    vida.EdadActual = 0f;
                    vida.CooldownRestante = plantillaAdn.tiempreEntreReproduccion;

                    // Aplicamos los cambios físicos al transform de Unity
                    vida.transform.localScale = Vector3.one * plantillaAdn.tamano;

                    // Pintamos su Sprite del color exacto de su linaje
                    SpriteRenderer sr = vida.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = plantillaAdn.colorLinaje;
                    }

                    // La bautizamos con su nombre real guardado en el gestor
                    string nombreReal = GestorLinajes.Instance.GetNombrePorId(idEspecieSeleccionada);
                    vida.gameObject.name = $"{nombreReal} (Clon Manual)";

                    Debug.Log($"¡Modo Dios: Inyectado clon de {nombreReal} (ID #{idEspecieSeleccionada}) en {posicion}!");
                }

                // 2. Encendemos la bacteria con la energía al máximo según su adn
                vida.EnergiaActual = vida.misStats.energiaMax;
            }
            else
            {
                Debug.LogError("No se puede inicializar la bacteria porque falta el GestorLinajes en la escena.");
            }
        }
    }
    public void SeleccionarPincelEspecie(int idLinaje)
    {
        idEspecieSeleccionada = idLinaje;
        Debug.Log($" Pincel cambiado al linaje: {idLinaje}");
    }

    // --- CÁMARA ---
    public void CentrarCamara()
    {
        if (Camera.main != null)
        {
            Transform camTransform = Camera.main.transform;
            camTransform.position = new Vector3(0f, 0f, camTransform.position.z);
        }
    }

    // --- Metodos publicos para los botones ---
    public void ActivarMenu()
    {
        menuPrincipal.AlternarPausa();
        ActualizarVisualBotones();
    }

    public void ActivarModoMoverse()
    {
        modoActual = ModoRaton.Moverse;
        ActualizarVisualBotones();
    }

    public void ActivarModoInspeccion()
    {
        modoActual = ModoRaton.Inspeccion;
        ActualizarVisualBotones();
    }

    public void ActivarModoCreador()
    {
        modoActual = ModoRaton.Creador;
        idEspecieSeleccionada = -1; // Por defecto empezamos con el pincel en "Nueva Especie"
        ActualizarVisualBotones();
    }

    public void AbrirLaboratorio()
    {
        if (panelLaboratorio != null)
        {
            panelLaboratorio.AbrirPanel();
        }
    }

    // --- LÓGICA VISUAL ---
    private void ActualizarVisualBotones()
    {
        PintarBotonActivo(botonMenu, false);
        PintarBotonActivo(botonMover, modoActual == ModoRaton.Moverse);
        PintarBotonActivo(botonInspeccionar, modoActual == ModoRaton.Inspeccion);
        PintarBotonActivo(botonCrear, modoActual == ModoRaton.Creador);

        // Encendemos o apagamos el Scroll View de cartas
        if (panelSelectorEspecies != null)
        {
            panelSelectorEspecies.SetActive(modoActual == ModoRaton.Creador);
        }
    }

    private void PintarBotonActivo(Button boton, bool estaActivo)
    {
        if (boton == null) return;
        ColorBlock colores = boton.colors;
        colores.normalColor = estaActivo ? new Color(0f, 0f, 0f, 0.4f) : new Color(0f, 0f, 0f, 0f);
        boton.colors = colores;
    }
}