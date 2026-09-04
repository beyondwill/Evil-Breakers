using System.Collections.Generic;
using UnityEngine;

// =========================================================
// 팀 타입
// =========================================================

public enum TeamType
{
    Ally,
    Enemy
}

// =========================================================
// 타겟 인덱스
// =========================================================

public struct TargetIndex
{
    public TeamType team;
    public int index;
}

// =========================================================
// 카드 타입
// =========================================================

public enum CardType
{
    None,
    Attack,
    Skill,
    Power,
    Curse,
    Status
}

// =========================================================
// 카드 대상
// =========================================================

public enum CardTarget
{
    None,
    Ally,
    Enemy,
    Any
}

// =========================================================
// 카드 희귀도
// =========================================================

public enum CardRarity
{
    Basic,
    Common,
    Rare,
    Epic,
    Legendary
}

// =========================================================
// 카드 기간
// =========================================================

public enum CardPeriod
{
    None,
    Repeat
}

// =========================================================
// 카드 효과
// =========================================================

[System.Serializable]
public class CardEffectEntry
{
    public float time;

    public CardVisual visual;

    public CardEffect effect;

    public List<int> valueList;

    public List<float> floatValueList;

    public DataEntity dataEntity;

    public CardCondition condition;

    public bool AccuracyReset = false;
}

// =========================================================
// 카드 데이터
// =========================================================

[CreateAssetMenu(
    fileName = "New Card",
    menuName = "Card/Card Data"
)]
public class CardData : DataEntity
{
    [Header("Info")]

    public CharacterClass characterClass;

    public CardType cardType;

    public CardTarget cardTarget;

    public CardRarity cardRarity;

    public CardPeriod cardPeriod;


    public Sprite card_image;

    public string card_name;

    public int card_cost;

    public int buy_card_cost;

    public int sell_card_cost;

    public bool useAccuracy = false;

    public float accuracy = 0f;


    [TextArea]
    public string card_description;


    // =====================================================
    // Condition
    // =====================================================

    [Header("Card Conditions")]

    // 모든 조건을 만족해야 카드를 낼 수 있음
    public List<CardCondition> useConditions = new();


    // 이 조건을 만족하면 카드가 황금색으로 표시됨
    public CardCondition specialcardCondition;


    // =====================================================
    // Timeline
    // =====================================================

    [Header("Timeline")]

    public List<CardEffectEntry> effects = new();
}