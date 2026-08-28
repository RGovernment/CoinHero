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

        // 차후 우선순위 지정, 지금 당장은 그냥 순서대로 3개 뽑기
        int count = 0;
        while (result.Count < 3)
        {
            result.Add(card[count]);
            count++;
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
