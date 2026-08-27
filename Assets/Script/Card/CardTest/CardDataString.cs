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
        GUIStyle customHelpBoxStyle = new(EditorStyles.helpBox)
        {
            fontSize = 12,
            richText = true
        };

        GUIContent content = EditorGUIUtility.TrTextContentWithIcon(
            "[Description 작성 가이드]\n" +
            "동적 숫자가 필요할 경우 해당 텍스트로 대체해 작성\n" +
            "기본 위력 관련 : [Value]\n" +
            "지속 시간 관련 : [Duration]\n" +
            "코인 개수 관련 : [Coin]\n" +
            "코인 위력 관련 : [CoinPoint]\n" +
            "각 값에 계산이 필요할 경우 두 값 사이에 삽입 \n" +
            "- Plus,Minus,Multiply\n" +
            "예시 : [ValuePlusCoinMultiplyCoinPoint],[CoinMultiplyCoinPoint]",MessageType.Info);

        GUILayout.Label(content, customHelpBoxStyle);


        if (GUILayout.Button("Card 데이터 출력")) component.JsonWrite();
        if (GUILayout.Button("effect 데이터 출력")) component.JsonWrite();
    }
}
