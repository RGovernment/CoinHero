using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;

public class SelectZone : MonoBehaviour
{
    [SF] private Transform Canvas;
    [SF] private List<GameObject> EmptySlot;

    [SF] private Button CancelBtn;
    [SF] private Button TurnEndBtn;

    private Queue<Card> cardList;
    private int nowSelectCount = 0;

    public event Action OnSelectCard;
    public event Action OnCancelCard;
    public event Action OnSelectCardComplete;

    private int GetSelectZoneCount() 
    {
        int count = 0;
        foreach (var item in EmptySlot)
        {
            if (item.activeSelf) count++;
        }
        return count;
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        cardList = new();
        ResetCardZone();
    }

    public void ResetCardZone()
    {
        foreach (var slot in EmptySlot)
        {
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.transform.GetChild(i).gameObject);
            }
        }
        cardList.Clear();
        nowSelectCount = 0;
        CancelBtn.gameObject.SetActive(false);

        OnCancelCard?.Invoke();
    }

    public void TurnEnd()
    {
        OnSelectCardComplete?.Invoke();
    }

    public void CardZoneOpen(int zone = 3)
    {
        zone = zone < 5 ? zone : 5;

        foreach (var item in EmptySlot)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < zone; i++)
        {
            EmptySlot[i].SetActive(true);
        }

        gameObject.SetActive(true);
        TurnEndBtn.gameObject.SetActive(true);
    }

    public void SetCardToEnemyZone(BehindCardData data)
    {
        data.transform.SetParent(EmptySlot[nowSelectCount].transform);
        data.transform.localPosition = Vector3.zero;
        data.transform.localScale = Vector3.one;
        data.gameObject.SetActive(true);

        nowSelectCount++;
    }

    public async UniTaskVoid SetCardToZone(CardData data)
    {
        if (nowSelectCount >= GetSelectZoneCount()) return;
        
        Transform emptySlot = EmptySlot[nowSelectCount].transform;
        data.gameObject.SetActive(false);
        CardData slotObj = Instantiate(data, Canvas);
        slotObj.Init(data.cardData);
        slotObj.transform
            .SetPositionAndRotation(data.transform.position, data.transform.rotation);
        slotObj.gameObject.tag = "Slot";
        await UniTask.DelayFrame(1);
        slotObj.gameObject.SetActive(true);
        cardList.Enqueue(slotObj.cardData);

        Sequence seq = DOTween.Sequence();

        Vector3 scale = EmptySlot[nowSelectCount].transform.localScale;
        slotObj.gameObject.SetActive(true);
        nowSelectCount++;
        await seq
            .Join(slotObj.transform.DOMove(emptySlot.position, 0.15f))
            .Append(slotObj.transform.DORotate(Vector3.zero, 0.05f))
            .Join(slotObj.transform.DOScale(Vector3.one * 0.5f, 0.03f))
            .Append(slotObj.transform.DOScale(scale, 0.05f))
            .ToUniTask();

        slotObj.transform.SetParent(emptySlot);

        if (!CancelBtn.gameObject.activeSelf)
            CancelBtn.gameObject.SetActive(true);

        OnSelectCard?.Invoke();
    }

    public Queue<Card> GetCardList()
    {
        return new(cardList);
    }

    public void BtnClose()
    {
        TurnEndBtn.gameObject.SetActive(false);
        CancelBtn.gameObject.SetActive(false);
    }
}
