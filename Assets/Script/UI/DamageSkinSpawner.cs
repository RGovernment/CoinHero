using DamageNumbersPro;
using System.Text;
using UnityEngine;
using static Constants;

public class DamageSkinSpawner : MonoBehaviour
{
    public static DamageSkinSpawner Instance;

    public DamageNumber skin;
    public DamageNumber heal;
    public DamageNumber text;
    public RectTransform targetCanvas;
    private readonly StringBuilder sb = new(64);
    private Camera mainCam;
    public void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
    }

    public void DamageSkinSpawn(Vector3 pos, int damage)
    {
        string damageStr = damage.ToString();
        sb.Clear();
        for (int i = 0; i < damageStr.Length; i++)
        {
            sb.Append($"<sprite=");
            sb.Append(damageStr[i]); 
            sb.Append(" color=#");
            sb.Append(ATTACK_COLOR);
            sb.Append(">");
        }

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas,
            screenPoint,
            mainCam,
            out Vector2 localPoint
        );

        skin.SpawnGUI(targetCanvas, localPoint, sb.ToString());
    }
    public void HealSkinSpawn(Vector3 pos, int damage)
    {
        string damageStr = damage.ToString();
        Debug.Log(damageStr);
        sb.Clear();
        for (int i = 0; i < damageStr.Length; i++)
        {
            sb.Append($"<sprite=");
            sb.Append(damageStr[i]);
            sb.Append(" color=#");
            sb.Append(HEAL_COLOR);
            sb.Append(">");
        }

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas,
            screenPoint,
            mainCam,
            out Vector2 localPoint
        );

        heal.SpawnGUI(targetCanvas, localPoint, sb.ToString());
    }

    public void TextSpawn(Vector3 pos, string str)
    {

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetCanvas,
            screenPoint,
            mainCam,
            out Vector2 localPoint
        );

        text.SpawnGUI(targetCanvas, localPoint, str);
    }


}
