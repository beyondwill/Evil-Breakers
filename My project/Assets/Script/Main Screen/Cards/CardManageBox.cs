using System.Collections.Generic;
using UnityEngine;

public class CardManageBox : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private GameObject card_content;
    [SerializeField] private GameObject card_prefab;


    // 임시
    public List<CardData> cards;



    // 카드 추가
    public void CardAdd(CardData card)
    {
        if (card == null)
        {
            Debug.LogError("추가하려는 카드가 null");
            return;
        }


        GameObject new_card =
            Instantiate(card_prefab, card_content.transform);


        CardView view =
            new_card.GetComponent<CardView>();


        if (view != null)
        {
            view.CardInit(card);
        }


        CardInteraction interaction =
            new_card.GetComponent<CardInteraction>();


        if (interaction != null)
        {
            interaction.Init(card, null);
        }
    }



    // 카드 제거
    public void CardRemove(int index)
    {

    }



    // 모든 카드 제거
    public void RemoveAllCards()
    {
        foreach (Transform child in card_content.transform)
        {
            Destroy(child.gameObject);
        }
    }



    // 단순 표시용
    public void ShowCards(List<CardData> cardList)
    {
        if (cardList == null)
            return;


        foreach (CardData cardInfo in cardList)
        {
            if (cardInfo == null)
            {
                Debug.LogError("덱 리스트에 null 카드 존재");
                continue;
            }


            GameObject card =
                Instantiate(card_prefab, card_content.transform);


            CardView view =
                card.GetComponent<CardView>();


            if (view != null)
            {
                view.CardInit(cardInfo);
            }
        }
    }



    // 상호작용 포함 표시용
    public void ShowCards(
        List<CardData> cardList,
        IDeckCardInteraction interaction)
    {
        if (cardList == null)
            return;



        for (int i = 0; i < cardList.Count; i++)
        {
            CardData cardInfo = cardList[i];


            if (cardInfo == null)
            {
                Debug.LogError("덱 리스트에 null 카드 존재");
                continue;
            }



            GameObject card =
                Instantiate(card_prefab, card_content.transform);



            CardView view =
                card.GetComponent<CardView>();


            if (view != null)
            {
                view.CardInit(cardInfo);
            }



            CardInteraction cardInteraction =
                card.GetComponent<CardInteraction>();


            if (cardInteraction != null)
            {
                cardInteraction.Init(
                    cardInfo,
                    interaction
                );


                // ★ 클릭한 카드 위치 저장
                cardInteraction.SetDeckIndex(i);
            }
        }
    }
}