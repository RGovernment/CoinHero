using UnityEngine;

public interface IState
{
    public void OnStart();
    public void OnStay();
    public void OnEnd();
}
