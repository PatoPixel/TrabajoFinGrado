using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
- Permite arrastrar una ventana UI desde su cabecera 
y ponerla flotando en la pantalla
*/

public class ArrastrarVentanaDesdeCabecera : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform ventanaRect; // RectTransform de la ventana
    private Transform lienzoFlotante; // Parent para el arrastre
    private Canvas canvasPrincipal;

    void Start()
    {
        // Encontrar el RectTransform de la ventana padre
        if (transform.parent != null)
        {
            ventanaRect = transform.parent.GetComponent<RectTransform>();
        }

        // Obtener el Canvas para corregir el movimiento
        canvasPrincipal = GetComponentInParent<Canvas>();

        // Buscar el contenedor de arrastre o usar el Canvas
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

        // Guardar el tamaño actual antes de mover
        Vector2 tamanoVentana = ventanaRect.rect.size;

        // Cambiar el padre para arrastrar libremente
        ventanaRect.SetParent(lienzoFlotante);

        // Mantener el tamaño de la ventana
        ventanaRect.sizeDelta = tamanoVentana;

        // Poner la ventana por delante
        ventanaRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ventanaRect == null || canvasPrincipal == null) return;

        // Actualizar la posición según el arrastre
        ventanaRect.anchoredPosition += eventData.delta / canvasPrincipal.scaleFactor;
    }
}