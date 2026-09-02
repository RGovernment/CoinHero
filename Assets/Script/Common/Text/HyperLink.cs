using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HyperLink : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textMeshPro;

    public static event Action<RectTransform, string> OnLinkClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        Camera cameraData = Camera.main;
        Canvas m_Canvas = gameObject.GetComponentInParent<Canvas>();
        if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay) cameraData = null;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Mouse.current.position.value, cameraData);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();

            RectTransform clickedRect =
                eventData.pointerPressRaycast.gameObject.GetComponent<RectTransform>();

            OnLinkClick?.Invoke(clickedRect, linkId);
        }
    }
}
