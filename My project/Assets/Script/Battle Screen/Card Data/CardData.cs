using System.Collections.Generic;
using UnityEngine;

// 팀 타입
public enum TeamType
{
    Ally,
    Enemy
}

// 타겟 인덱스
public struct TargetIndex
{
    public TeamType team;
    public int index;
}

// 카드 타입
public enum CardType
{
    None,
    Attack,
    Skill,
    Power,
    Curse,
    Status
}

// 카드로 지정할 수 있는 대상
public enum CardTarget
{
    None,                   // 없음
    Ally,                   // 아군
    Enemy,                  // 적
    Any                     // 모두
}

// 카드 희귀도
public enum CardRarity
{
    Basic,                  // 기본 제공 카드
    Common,                 // 일반 카드
    Rare,                   // 희귀 카드
    Epic,                   // 특급 카드
    Legendary               // 전설 카드
}

// 카드 기간
public enum CardPeriod
{
    None,
    Repeat
}

[System.Serializable]
public class CardEffectEntry
{
    // 카드 사용 시작 후 실행 시점
    public float time;

    // 연출
    public CardVisual visual;

    // 실행할 효과
    public CardEffect effect;

    // 효과 수치
    public List<int> valueList;
    public List<float> floatValueList;

    public DataEntity dataEntity;
    public CardCondition condition;
    public bool AccuracyReset = false;          // 명중률 초기화 여부
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card/Card Data")]
public class CardData : DataEntity
{
    [Header("Info")]
    public CharacterClass characterClass;               // 캐릭터 직업
    public CardType cardType;                           // 카드 타입
    public CardTarget cardTarget;                       // 카드 대상
    public CardRarity cardRarity;                       // 카드 희귀도
    public CardPeriod cardPeriod;                       // 카드 기간

    public Sprite card_image;                           // 카드 이미지
    public string card_name;                            // 카드 이름
    public int card_cost;                               // 카드 비용
    public int buy_card_cost;                           // 구매 카드 비용
    public int sell_card_cost;                          // 판매 카드 비용
    public bool useAccuracy = false;                    // 명중률 사용 여부
    public float accuracy = 0f;                         // 명중률

    [TextArea]
    public string card_description;

    [Header("Condtiion")]
    public CardCondition specialcardCondition;          // 특수 이펙트 발동 카드 조건(카드 테두리 황금색 변경) 

    [Header("Timeline")]
    public List<CardEffectEntry> effects = new();       // 카드 이펙트
}