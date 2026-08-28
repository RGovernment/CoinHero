using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Enums;
using SF = UnityEngine.SerializeField;

public class BehindCardData : MonoBehaviour
{
    [SF] private Image typeIcon;
    /// <summary>
    /// (int)CardType.x로 호출해 사용
    /// </summary>
    public Sprite[] typeIconSprite;
    public CardType Type;

    public void SetTypeIcon()
    {
        if (ResourceManager.Instance.EffectData == null) return;

        int typeInt = (int)Type;

        typeIcon.sprite = typeIconSprite[typeInt];
    }
}
