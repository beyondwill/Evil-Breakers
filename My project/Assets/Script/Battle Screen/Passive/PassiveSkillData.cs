using System.Collections.Generic;
using UnityEngine;


// =========================================================
// 패시브 발동 트리거
// =========================================================

public enum PassiveTriggerType
{
    BattleStart,        // 전투 시작
    TurnStart,          // 턴 시작
    TurnEnd,            // 턴 종료
    RoundStart,         // 라운드 시작
    RoundEnd,           // 라운드 종료
    PlayCard,           // 카드를 사용했을 때
    EnemyCard,          // 적이 카드를 사용했을 때
    Damaged,            // 피해를 받았을 때
    ManaSpent,          // 마나를 소모했을 때
    Death               // 사망했을 때
}


// =========================================================
// 패시브 조건에서 검사할 값
// =========================================================

public enum PassiveConditionType
{
    HP,                     // 현재 HP
    Turn,                   // 현재 턴
    Round,                  // 현재 라운드

    DamageTaken,            // 이번에 받은 피해량
    ManaSpent,              // 이번에 소모한 마나
    CardsPlayedThisTurn     // 이번 턴 사용한 카드 수
}


// =========================================================
// 비교 방법
// =========================================================

public enum PassiveCompareType
{
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    Equal
}


// =========================================================
// 패시브 조건
// =========================================================

[System.Serializable]
public class PassiveCondition
{
    [Header("Condition")]

    // 무엇을 검사할 것인가?
    public PassiveConditionType conditionType;

    // 어떻게 비교할 것인가?
    public PassiveCompareType compareType;

    // 기준값
    public float value;
}


// =========================================================
// 패시브 스킬 데이터
// =========================================================

[CreateAssetMenu(
    fileName = "New Passive Skill",
    menuName = "Character/Passive Skill"
)]
public class PassiveSkillData : DataEntity
{
    // =====================================================
    // 기본 정보
    // =====================================================

    [Header("Info")]

    public string skill_name;

    [TextArea]
    public string skill_description;

    public Sprite skill_image;


    // =====================================================
    // 발동 조건
    // =====================================================

    [Header("Trigger")]

    public PassiveTriggerType triggerType;


    // =====================================================
    // 조건
    // =====================================================

    [Header("Conditions")]

    // 여러 개라면 전부 만족해야 발동
    public List<PassiveCondition> conditionList = new();


    // =====================================================
    // 효과
    // =====================================================

    [Header("Effects")]

    // 카드와 동일한 효과 시스템 사용
    public List<CardEffectEntry> effects = new();
}