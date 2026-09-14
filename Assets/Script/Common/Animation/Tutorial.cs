using UnityEngine;
using UnityEngine.Playables;
using SF = UnityEngine.SerializeField;

public class Tutorial : MonoBehaviour
{
    [SF] private PlayableDirector timeline;

    private bool stopAble = false;

    public void TimelineStop()
    {
        timeline.playableGraph.GetRootPlayable(0).SetSpeed(0);
        stopAble = true;
    }

    public void TimelineStart()
    {
        if (stopAble)
        {
            timeline.playableGraph.GetRootPlayable(0).SetSpeed(1);
            stopAble = false;
        }
    }

    public void TutorialEnd()
    {
        gameObject.SetActive(false);
        GameManager.Instance.state.IsTutorialCompleted = true;
    }
}
