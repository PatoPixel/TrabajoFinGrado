using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Usamos estas interfaces para detectar clics y arrastres del ratón sobre la imagen
public class RuedaDeColor : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    [Header("Referencias")]
    [SerializeField] private Image imagenRueda;
    [SerializeField] private PanellCreacion laboratorioUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        ExtraerColor(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ExtraerColor(eventData);
    }

    private void ExtraerColor(PointerEventData eventData)
    {
        if (imagenRueda == null || imagenRueda.sprite == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 posicionLocal;

        // 1. Traducimos el clic de la pantalla a coordenadas locales del cuadro de la imagen
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out posicionLocal))
        {
            // 2. Normalizamos la posición (de 0.0 a 1.0)
            Rect rect = rectTransform.rect;
            float px = Mathf.Clamp01((posicionLocal.x - rect.x) / rect.width);
            float py = Mathf.Clamp01((posicionLocal.y - rect.y) / rect.height);

            // 3. Traducimos eso a los píxeles reales de la textura
            Texture2D textura = imagenRueda.sprite.texture;
            int texX = Mathf.RoundToInt(px * textura.width);
            int texY = Mathf.RoundToInt(py * textura.height);

            // 4. Extraemos el color de ese píxel
            Color colorExtraido = textura.GetPixel(texX, texY);

            // Solo enviamos el color si no hemos pinchado en una zona transparente (fuera del círculo)
            if (colorExtraido.a > 0.1f)
            {
                if (laboratorioUI != null)
                {
                    laboratorioUI.SeleccionarColorDinamico(colorExtraido);
                }
            }
        }
    }
}