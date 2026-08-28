using UnityEngine;
using UnityEngine.UI;

public class EmptyButton : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
