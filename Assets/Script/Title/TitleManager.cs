using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

public class TitleManager : MonoBehaviour
{
    [SF] private Button startBtn;
    [SF] private Button closeBtn;

    public void StartGame()
    {
        // 임시로 즉시 이동
        SceneManager.LoadScene(2);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
