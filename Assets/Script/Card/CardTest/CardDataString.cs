using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardWrite))]
public class CardDataString : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CardWrite component = (CardWrite)target;

        if (GUILayout.Button("Card 데이터 출력")) component.JsonWrite();
        if (GUILayout.Button("effect 데이터 출력")) component.JsonWrite();
    }
}
