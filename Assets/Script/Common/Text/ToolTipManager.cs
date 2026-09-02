using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;
public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager Instance { get; private set;  }

    [SF] private RectTransform toolTipRect;
    [SF] private RectTransform backgroundRect;
    [SF] private TextMeshProUGUI nameText;
    [SF] private TextMeshProUGUI descriptionText;

    private int count = 0;
    private RectTransform before;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        HyperLink.OnLinkClick -= BuffEndDebuffClick;
        HyperLink.OnLinkClick += BuffEndDebuffClick;
    }

    private void OnDisable()
    {
        HyperLink.OnLinkClick -= BuffEndDebuffClick;
    }

    public void BuffEndDebuffClick(RectTransform transform, string id)
    {
        toolTipRect.gameObject.SetActive(true);
        // 버프 쌓기, 차후 여유 생기면 이걸로 변경
        if (before != null && before == transform && count > 0)
        {
        }
        // 버프창 갱신
        else
        {
            count = 0;

            StatusEffectData data = ResourceManager.Instance.EffectData[int.Parse(id)];
            string typeText = id[0] == '1' ? "버프" : "디버프";
            nameText.text = $"{data.Name}\n<size=14>{typeText}</size>";
            descriptionText.text = data.Description;

            // 동적 크기 강제 갱신
            nameText.ForceMeshUpdate();
            descriptionText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(toolTipRect);

            Vector3[] corners = new Vector3[4];

            transform.GetWorldCorners(corners);

            backgroundRect.position = corners[2];

            //쌓는 버전 추가시 카운트 증가 추가
            //count++;
        }
    }

    public void ToolTipClose()
    {
        toolTipRect.gameObject.SetActive(false);
    }
}
