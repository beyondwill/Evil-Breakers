
// 유물 구분
using System.Collections.Generic;
using UnityEngine;

public enum RelicSort
{
    Common,             // 일반 등급
    Rare,               // 희귀 등급
    Epic                // 특급 등급
}

// 소지 범위
public enum OwnershipScope
{
    Individual,         // 개인
    PlayerAll           // 파티
}

// 조건 타입
public enum ConditionStatType
{
    HP,
    Turn,
    Round,
    CardsPlayedThisTurn
}

// 비교 타입
public enum CompareType
{
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    Equal
}

[System.Serializable]
// 발동 조건
public class PassiveCondition
{
    public ConditionStatType statType;
    public CompareType compare;
    public float value;
}

// 트리거 타입
public enum TriggerType
{
    Always,                 // 항상(모든 경우 조사)
    OnEquip,                // 장착 시
    BattleStart,            // 전투 시작 시
    TurnStart,              // 턴 시작 시
    TurnEnd,                // 턴 종료 시
    RoundStart,             // 라운드 시작 시
    RoundEnd,               // 라운드 종료 시
    PlayCard,               // 플레이어가 카드를 낼 때 마다
    EnemyCard               // 적이 카드를 낼 때 마다
}

// 패시브 스킬 이벤트
[System.Serializable]
public class PassiveEvent
{
    [Header("Trigger")]
    public TriggerType triggerType;

    [Header("Condition")]
    public List<PassiveCondition> conditionList;            // 발동 조건

    [Header("Action")]
    public List<EffectValue> effectValueList;               // 효과 발동
}
