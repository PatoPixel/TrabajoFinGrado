using UnityEngine;

public class ControladorMenuPausa : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelPausa;
    public GameObject panelListaPartidas;

    public static bool juegoPausado = false;
    private float velocidadAnterior = 1f;

    private void Awake()
    {
        // El estado de pausa siempre se resetea al cargar una nueva escena
        // (es static y podría venir contaminado de la escena anterior)
        juegoPausado = false;

        // En modo tutorial el TutorialManager controla el tiempo — no tocamos timeScale
        if (TutorialManager.TutorialActivo) return;

        Time.timeScale = 1f;
        velocidadAnterior = 1f;
    }

    private void Start()
    {
        // 2. Ocultamos todo directamente por seguridad
        if (panelListaPartidas != null) panelListaPartidas.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false);

        Debug.Log("MenuPausa Sistema Iniciado. Paneles apagados.");
    }

    void Update()
    {
        // Durante el tutorial el Escape está bloqueado
        if (TutorialManager.TutorialActivo) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log(" MenuPausa Tecla ESC pulsada.");

            if (juegoPausado && panelListaPartidas.activeSelf)
            {
                CerrarSubMenu();
                panelPausa.SetActive(true);
            }
            else
            {
                AlternarPausa();
            }
        }
    }

    private void CerrarSubMenu()
    {
        if (panelListaPartidas != null) panelListaPartidas.SetActive(false);
    }

    public void AlternarPausa()
    {
        // MAGIA DE SENIOR: Ya no confiamos en variables, miramos si el panel existe visualmente
        bool elPanelEstaEncendido = panelPausa.activeSelf;

        if (elPanelEstaEncendido)
        {
            ReanudarJuego();
        }
        else
        {
            PausarJuego();
        }
    }

    private void PausarJuego()
    {
        Debug.Log(" MenuPausa Pausando juego y ENCENDIENDO panel...");

        // 1. Encendemos visualmente
        panelPausa.SetActive(true);

        // 2. Guardamos la velocidad
        if (Time.timeScale > 0f)
        {
            velocidadAnterior = Time.timeScale;
        }

        // 3. Congelamos
        Time.timeScale = 0f;
        juegoPausado = true;
    }

    public void ReanudarJuego()
    {
        Debug.Log(" MenuPausa Reanudando juego y APAGANDO panel...");

        // 1. Apagamos visualmente
        panelPausa.SetActive(false);

        // 2. Descongelamos
        Time.timeScale = velocidadAnterior > 0f ? velocidadAnterior : 1f;
        juegoPausado = false;
    }
}