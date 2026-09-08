using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Constants;
using SF = UnityEngine.SerializeField;
public class EnemyHandManager : MonoBehaviour
{
    [SF] private BehindCardData cardData;
    [SF] private Transform holder;

    public List<Card> CardSelect(List<Card> baseCards)
    {
        List<Card> baseCard = new();

        foreach (var item in baseCards)
        {
            Card newCard = new();
            newCard = newCard.Init(item);
            baseCard.Add(newCard);
            Debug.Log(newCard.ToString());
        }
        baseCard.Shuffle();

        return baseCard.Take(ENEMY_HAND_MAX_COUNT).ToList();
    }

    public BehindCardData CardCreate(Card data)
    {
        BehindCardData bDdata = Instantiate(cardData, holder);

        bDdata.Type = data.Type;

        return bDdata;
    }
}
