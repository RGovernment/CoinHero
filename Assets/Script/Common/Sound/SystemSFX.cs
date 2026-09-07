using UnityEngine;
using SF = UnityEngine.SerializeField;
using static Enums;
using static Constants;
using UnityEngine.EventSystems;

public class SystemSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SF] private SystemSoundType hoverType;
    [SF] private SystemSoundType clickType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySystemSFX(clickType);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySystemSFX(hoverType);
    }
}
