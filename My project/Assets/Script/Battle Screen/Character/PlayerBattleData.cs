using System.Collections.Generic;


[System.Serializable]
public class PlayerBattleData
{
    // 연결된 캐릭터
    public CharacterVariable character;

    // 카드
    public List<CardVariable> deck = new();
    public List<CardVariable> hand = new();
    public List<CardVariable> graveyard = new();

    // 전투 자원
    public int energy;

    public void DrawCard(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count <= 0)
                return;


            CardVariable card = deck[0];

            deck.RemoveAt(0);

            hand.Add(card);
        }
    }
}