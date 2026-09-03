using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;
public class EnemyHandManager : MonoBehaviour
{
    [SF] private BehindCardData cardData;
    [SF] private Transform holder;

    public List<Card> CardSelect(List<Card> card)
    {
        List<Card> result = new();

        // 차후 우선순위 지정, 지금 당장은 그냥 순서대로 최대 3개 뽑기
        int pickCount = Mathf.Min(3, card.Count);
        for (int i = 0; i < pickCount; i++)
        {
            result.Add(card[i]);
        }

        return result;
    }

    public BehindCardData CardCreate(Card data)
    {
        BehindCardData bDdata = Instantiate(cardData, holder);

        bDdata.Type = data.Type;

        return bDdata;
    }
}
