using UnityEngine;

/*
- Controla la bandeja que muestra las cartas de especies o herramientas disponibles, dependiendo del modo actual.
- Se encarga de redibujar la bandeja cada vez que se activa o cambia el modo, instanciando las cartas correspondientes.
- Notifica a otras partes del sistema cuando se selecciona una carta, para que puedan reaccionar en consecuencia.
*/

public class BandejaEspeciesUI : MonoBehaviour
{
  
    public static event System.Action<MonoBehaviour> OnCartaSeleccionada;
    public static void NotificarSeleccion(MonoBehaviour carta) => OnCartaSeleccionada?.Invoke(carta);

    [Header("Contenedor")]
    [SerializeField] private Transform contenedorContent;

    [Header("Prefabs de Bacterias")]
    [SerializeField] private GameObject prefabCartaEspecie;

    [Header("Prefabs de Herramientas Separados")]
    [SerializeField] private GameObject prefabCartaComida;
    [SerializeField] private GameObject prefabCartaSpawner;

    [SerializeField] private ControladorInteraccion controladorInteraccion;

    private void OnEnable()
    {
        RedibujarBandeja();
    }

    public void RedibujarBandeja()
    {
        if (contenedorContent == null || controladorInteraccion == null) return;

        foreach (Transform hijo in contenedorContent)
        {
            Destroy(hijo.gameObject);
        }

        if (controladorInteraccion.modoActual == ControladorInteraccion.ModoRaton.Herramientas)
        {
            if (prefabCartaComida != null)
            {
                GameObject cartaComida = Instantiate(prefabCartaComida, contenedorContent);
                if (cartaComida.TryGetComponent(out CartaComidaUI uiComida))
                {
                    uiComida.Inicializar("ANNADIR COMIDA", "50", controladorInteraccion);
                }
            }

            if (prefabCartaSpawner != null)
            {
                GameObject cartaSpawner = Instantiate(prefabCartaSpawner, contenedorContent);
                if (cartaSpawner.TryGetComponent(out CartaSpawnerUI uiSpawner))
                {
                    uiSpawner.Inicializar("PLANTAR SPAWNER", "10", "30", "70", "0.2", controladorInteraccion);
                }
            }
        }
        else if (controladorInteraccion.modoActual == ControladorInteraccion.ModoRaton.Creador)
        {
            if (prefabCartaEspecie != null)
            {
                GameObject cartaLab = Instantiate(prefabCartaEspecie, contenedorContent);
                if (cartaLab.TryGetComponent(out CartaEspecieUI uiLab))
                {
                    uiLab.Inicializar(-1, "+ CREAR NUEVA", new DatosGeneticos(), controladorInteraccion);
                }
            }

            if (GestorLinajes.Instance != null && prefabCartaEspecie != null)
            {
                foreach (var kvp in GestorLinajes.Instance.plantillasLinajes)
                {
                    int idLinaje = kvp.Key;
                    DatosGeneticos stats = kvp.Value;
                    string nombreReal = GestorLinajes.Instance.GetNombrePorId(idLinaje);

                    GameObject nuevaCarta = Instantiate(prefabCartaEspecie, contenedorContent);
                    if (nuevaCarta.TryGetComponent(out CartaEspecieUI uiCarta))
                    {
                        uiCarta.Inicializar(idLinaje, nombreReal, stats, controladorInteraccion);
                    }
                }
            }
        }
    }
}