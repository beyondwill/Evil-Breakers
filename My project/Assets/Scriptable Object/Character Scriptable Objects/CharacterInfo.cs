using System;
using System.Collections.Generic;
using UnityEngine;


// =========================================================
// 상황
// =========================================================

public enum Situation
{
    MainNormal,
    MainHurt,
    MainStress,
    PartyFormation,
    BattleMapNormal,
    BattleMapHurt,
    BattleMapStress,
    CantStartCard,
    InvalidTarget,
    BattleStart,
}


// =========================================================
// 속성
// =========================================================

public enum Element
{
    None,               // 없음
    QIGONG,             // 기공
    BELIEF,             // 신앙
    SPIRITPOWER,        // 영력
    SUPERPOWER,         // 초능력
    PHYSIC,             // 물리
    SPACE,              // 우주
    CHAOS               // 혼돈
}


// =========================================================
// 캐릭터 직업
// =========================================================

public enum CharacterClass
{
    None,
    Character1,
    Character2,
    Character3,
    Character4,
    Neutral
}


// =========================================================
// 캐릭터 레벨 스탯
// =========================================================

[Serializable]
public class CharacterLevelStat
{
    public int level;

    public List<CharacterBaseStatValue> statList = new();
}


// =========================================================
// 캐릭터 대화
// =========================================================

[Serializable]
public class CharacterDialogue
{
    public Situation situation;

    public List<string> dialogue = new();
}


// =========================================================
// 캐릭터 정보
// =========================================================

[CreateAssetMenu(
    fileName = "Character",
    menuName = "Character/Character"
)]
public class CharacterInfo : DataEntity
{
    [Header("캐릭터 기본 정보")]
    public bool attack_impossible;    // 해당 캐릭터가 공격 가능한가
    public string character_name;
    public int character_id;
    public Element element;
    public CharacterClass characterClass;

    [Header("UI")]
    [TextArea]
    public string character_story;
    public Sprite character_full_art;
    public Sprite character_icon;
    public Color icon_background_color = Color.black;

    public List<CharacterLevelStat> levelStatList = new();

    public List<CharacterBaseStatValue> characterBaseStatValues = new();
    public List<CharacterBaseStatValue> characterUpgradeBaseStatValues = new();

    // 패시브
    public List<PassiveSkillData> passiveSkillList = new();

    // 대화문
    public List<CharacterDialogue> dialogues = new();


    // =====================================================
    // 스탯 가져오기
    // =====================================================

    public float GetStatValue(
        CharacterBaseStatType type,
        int level = 1)
    {
        if (levelStatList == null)
            return 0;


        CharacterLevelStat levelStat =
            levelStatList.Find(
                x =>
                    x != null &&
                    x.level == level
            );


        if (levelStat == null ||
            levelStat.statList == null)
        {
            return 0;
        }


        CharacterBaseStatValue stat =
            levelStat.statList.Find(
                x => x.type == type
            );


        if (stat == null)
            return 0;


        return stat.value;
    }

    // 스탯 밸류 가져오기
    public float GetStatValue(CharacterBaseStatType type, bool gozarani, int level = 0)
    {
        float value = 0f;
        var stat = characterBaseStatValues.Find(x => x.type == type);
        if (stat != null) { value = stat.value; }
        var levelUpStat = characterUpgradeBaseStatValues.Find(x => x.type == type);
        if (levelUpStat != null) { value += stat.value * level; }

        return value;
    }


    // 대화문 가져오기
    public string GetDialogue(
        Situation situation)
    {
        if (dialogues == null)
            return null;


        CharacterDialogue dialogue =
            dialogues.Find(
                x =>
                    x.situation == situation
            );


        if (dialogue == null ||
            dialogue.dialogue == null ||
            dialogue.dialogue.Count == 0)
        {
            return null;
        }


        return dialogue.dialogue[
            UnityEngine.Random.Range(
                0,
                dialogue.dialogue.Count
            )
        ];
    }
}