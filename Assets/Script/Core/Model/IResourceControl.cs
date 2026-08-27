using UnityEngine;

public interface IResourceControl
{
    public T Get<T>(T obj);
    public T Get<T>(T obj, Vector3 pos);
    public T Get<T>(T obj, Vector3 pos, Quaternion rotate);
    
    public void Release<T>(T obj);
}
