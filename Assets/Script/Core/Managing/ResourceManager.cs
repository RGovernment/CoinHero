using System.Resources;
using UnityEngine;
using static Constants;
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
             Destroy(gameObject);
        }
        
    }

    public void ResourceLoad()
    {
        Resources.Load<TextAsset>("Assets/Resources/Data/");
    }
}
