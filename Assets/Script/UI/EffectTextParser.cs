using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static Constants;
using static Enums;

public static class EffectTextParser
{
    private static readonly Regex TagRegex = new(@"\[(.*?)\]", RegexCompiledOption);

    private const RegexOptions RegexCompiledOption = RegexOptions.Compiled;

    /// <summary>
    /// 정규식 카드 텍스트 변환
    /// </summary>
    public static string ParseDescription(this string rawDescription, Card card)
    {
        if (string.IsNullOrEmpty(rawDescription)) return string.Empty;

        return TagRegex.Replace(rawDescription, match =>
        {
            string key = match.Value;

            if (key == $"[{VALUE}{PLUS}{COIN}{MULTIPLY}{COIN_POINT}]")
                return (card.Value + (card.Coin * card.CoinPoint)).ToString();

            if (key == $"[{COIN}{MULTIPLY}{COIN_POINT}]")
                return (card.Coin * card.CoinPoint).ToString();

            if (key == $"[{COIN_POINT}]")
                return card.CoinPoint.ToString();

            if (key == $"[{COIN}]")
                return card.Coin.ToString();

            if (key == $"[{VALUE}]")
                return card.Value.ToString();

            return match.Value;
        });
    }
}
