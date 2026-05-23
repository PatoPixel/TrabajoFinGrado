using UnityEngine;

public class BandejaEspeciesUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform contenedorContent; // El objeto 'Content' del Scroll View
    [SerializeField] private GameObject prefabCarta;       // El prefab de tu carta
    [SerializeField] private ControladorInteraccion controladorInteraccion;

    // Se ejecuta automáticamente cada vez que el panel se activa (al darle al botón de la jeringuilla)
    private void OnEnable()
    {
        RedibujarBandeja();
    }

    public void RedibujarBandeja()
    {
        if (contenedorContent == null || prefabCarta == null) return;

        // 1. Limpiamos las cartas antiguas
        foreach (Transform hijo in contenedorContent)
        {
            Destroy(hijo.gameObject);
        }

        // 2. CREAMOS EL SLOT ESPECIAL (Botón de + CREAR NUEVA)
        GameObject cartaLab = Instantiate(prefabCarta, contenedorContent);
        if (cartaLab.TryGetComponent(out CartaEspecieUI uiLab))
        {
            uiLab.Inicializar(-1, "+ CREAR NUEVA", new DatosGeneticos(), controladorInteraccion);
        }

        // 3. DIBUJAMOS LAS ESPECIES REALES GUARDADAS EN EL GESTOR
        if (GestorLinajes.Instance != null)
        {
            foreach (var kvp in GestorLinajes.Instance.plantillasLinajes)
            {
                int idLinaje = kvp.Key;
                DatosGeneticos stats = kvp.Value;
                string nombreReal = GestorLinajes.Instance.GetNombrePorId(idLinaje);

                GameObject nuevaCarta = Instantiate(prefabCarta, contenedorContent);
                if (nuevaCarta.TryGetComponent(out CartaEspecieUI uiCarta))
                {
                    uiCarta.Inicializar(idLinaje, nombreReal, stats, controladorInteraccion);
                }
            }
        }
    }
}