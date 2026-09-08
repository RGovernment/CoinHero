using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;
public class IngameOptionScript : MonoBehaviour
{
    [SF] private Button optionBtn;

    private void Start()
    {
        optionBtn.onClick.AddListener(() => OptionManager.Instance.OptionPanelOpen());
    }

    private void OnDestroy()
    {
        optionBtn.onClick.RemoveAllListeners();
    }
}
