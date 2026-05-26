using UnityEngine;

public class BandejaEspeciesUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform contenedorContent;
    [SerializeField] private GameObject prefabCartaEspecie;     
    [SerializeField] private GameObject prefabCartaHerramienta;
    [SerializeField] private ControladorInteraccion controladorInteraccion;

    private void OnEnable()
    {
        RedibujarBandeja();
    }

    public void RedibujarBandeja()
    {
        if (contenedorContent == null) return;

        // 1. Limpiamos las cartas antiguas
        foreach (Transform hijo in contenedorContent)
        {
            Destroy(hijo.gameObject);
        }

        // 2. CREAMOS SLOT: AÑADIR COMIDA (ID: -2) -> ¡AHORA ES EL PRIMERO!
        if (prefabCartaHerramienta != null)
        {
            GameObject cartaComida = Instantiate(prefabCartaHerramienta, contenedorContent);
            if (cartaComida.TryGetComponent(out CartaHerramientaUI uiComida))
            {
                uiComida.Inicializar(-2, "AÑADIR COMIDA", "50", Color.orange, controladorInteraccion);
            }
        }

        // 3. CREAMOS SLOT: LABORATORIO (ID: -1) -> VUELVE AL PREFAB ORIGINAL
        if (prefabCartaEspecie != null)
        {
            GameObject cartaLab = Instantiate(prefabCartaEspecie, contenedorContent);
            if (cartaLab.TryGetComponent(out CartaEspecieUI uiLab))
            {
                uiLab.Inicializar(-1, "+ CREAR NUEVA", new DatosGeneticos(), controladorInteraccion);
            }
        }

        // 4. DIBUJAMOS LAS ESPECIES REALES GUARDADAS
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