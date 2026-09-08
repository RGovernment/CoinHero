using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;
public class EnemyHandManager : MonoBehaviour
{
    [SF] private BehindCardData cardData;
    [SF] private Transform holder;

    public List<Card> CardSelect(List<Card> card)
    {
        List<Card> result = new(card);
        result.Shuffle();
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
