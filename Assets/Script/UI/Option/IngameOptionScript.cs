using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;
public class IngameOptionScript : MonoBehaviour
{
    [SF] private Button optionBtn;
    [SF] private TextMeshProUGUI playerGoldText;

    private void Start()
    {
        optionBtn.onClick.AddListener(() => OptionManager.Instance.OptionPanelOpen());
        GameManager.Instance.OnGoldChanged += GoldTextChange;
        playerGoldText.text = GameManager.Instance.state.gold.ToString();
    }

    private void GoldTextChange(int value)
    {
        playerGoldText.text = value.ToString();
    }

    private void OnDestroy()
    {
        optionBtn.onClick.RemoveAllListeners();
        GameManager.Instance.OnGoldChanged -= GoldTextChange;
    }
}
