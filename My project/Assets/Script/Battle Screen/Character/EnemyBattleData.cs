using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyBattleData
{
    public CharacterVariable character;


    // 사용할 카드 목록
    public List<CardVariable> cardList = new();


    // 이번 턴 예정 카드
    public CardVariable nextCard;


    public void ChooseRandomCard()
    {
        if (cardList.Count == 0)
        {
            nextCard = null;
            return;
        }

        nextCard =
            cardList[Random.Range(0, cardList.Count)];
    }
}