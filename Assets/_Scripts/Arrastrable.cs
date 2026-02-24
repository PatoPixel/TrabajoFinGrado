using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrastrarVentanaDesdeCabecera : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform ventanaRect; // La ventana completa (Padre)
    private Transform lienzoFlotante;
    private Canvas canvasPrincipal;

    void Start()
    {
        // 1. Buscamos a nuestro PADRE (La Ventana real)
        if (transform.parent != null)
        {
            ventanaRect = transform.parent.GetComponent<RectTransform>();
        }

        // 2. Buscamos el Canvas principal para saber la escala
        canvasPrincipal = GetComponentInParent<Canvas>();

        // 3. Buscamos la Zona Flotante (o el Canvas si no existe)
        GameObject zona = GameObject.Find("ZonaFlotante");
        if (zona != null)
        {
            lienzoFlotante = zona.transform;
        }
        else
        {
            lienzoFlotante = canvasPrincipal.transform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ventanaRect == null) return;

        // Guardamos el tamaño actual de la VENTANA (no del botón)
        Vector2 tamañoVentana = ventanaRect.rect.size;

        // CAMBIAMOS DE PADRE A LA VENTANA (El botón sigue siendo hijo de la ventana)
        ventanaRect.SetParent(lienzoFlotante);

        // Restauramos el tamaño de la ventana
        ventanaRect.sizeDelta = tamañoVentana;

        //  AQUÍ ESTÁ EL TRUCO:
        // Decimos que la VENTANA sea la última hermana del Canvas (para verse encima de otras ventanas)
        // NO el botón. El botón se queda quieto en su sitio dentro de la ventana.
        ventanaRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ventanaRect == null || canvasPrincipal == null) return;

        // Movemos la VENTANA entera
        ventanaRect.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;
    }
}