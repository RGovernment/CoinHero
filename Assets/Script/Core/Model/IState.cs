using UnityEngine;

public interface IState
{
    public void Start();
    public void Continue();
    public void End();
}
