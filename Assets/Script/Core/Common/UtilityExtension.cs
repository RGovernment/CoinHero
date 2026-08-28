using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static Enums;
using static Utility;

public static class UtilityExtension
{
    /// <summary>
    /// JObject를 원하는 타입으로 변환하여 반환하는 제네릭 메서드
    /// </summary>
    /// <typeparam name="T">변환할 타입</typeparam>
    /// <param name="path">불러올 JObject의 이름</param>
    /// <returns>변환된 데이터</returns>
    public static T GetValue<T>(this JToken obj, T defaultValue = default)
    {
        try
        {
            if (obj == null || obj.Type == JTokenType.Null)
                return defaultValue;

            return obj.Value<T>();
        }
        catch
        {
            Debug.LogWarning($"JToken 변환 실패: {obj} → {typeof(T).Name}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Fisher-Yates Shuffle, List용
    /// </summary>
    /// <typeparam name="T">모든 변수 타입</typeparam>
    /// <param name="values"></param>
    /// <returns>List</returns>
    public static List<T> Shuffle<T>(this List<T> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }

    /// <summary>
    /// Fisher-Yates Shuffle, Array용
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <returns>Array</returns>
    public static T[] Shuffle<T>(this T[] values)
    {
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }

    /// <summary>
    /// 함수가 기준치보다 짧을 경우, 함수 인자를 반복해 기준치까지 늘이는 함수
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="targetLength"></param>
    /// <returns></returns>
    public static List<T> ListExtend<T>(this List<T> source, int targetLength)
    {
        List<T> result = new(source);

        if (source.Count == 0) return null;

        int index = 0;

        while (result.Count < targetLength)
        {
            result.Add(source[index]);
            index = (index + 1) % source.Count;
        }

        return result;
    }

    /// <summary>
    /// 클릭 대기용 1회용 버튼 이벤트 부여 함수
    /// </summary>
    /// <param name="button"></param>
    /// <param name="token"></param>
    /// <returns></returns>

    public static UniTask OnClickAsync(this Button button, CancellationToken token = default)
    {
        var tcs = new UniTaskCompletionSource();

        void OnClick()
        {
            button.onClick.RemoveListener(OnClick);
            tcs.TrySetResult();
        }

        button.onClick.AddListener(OnClick);
        return tcs.Task;
    }
}