using UnityEngine;

[System.Serializable]
public class CardVariable
{
    public CardData original_card_info;     // 원본 카드 정보
    public int current_card_cost;           // 현재 카드 비용
    public bool can_use;                    // 카드를 낼 수 있는가?

    public CardVariable(CardData c)
    {
        original_card_info = c;
        Debug.Log(c.name);
        current_card_cost = c.card_cost;
    }

    public int DamageCalculation()
    {
        return 100;
    }

    public int DrawCalculation()
    {
        return 1;
    }

    public int ManaCalculation()
    {
        return 3;
    }

    public int HealCalculation()
    {
        return 100;
    }
}