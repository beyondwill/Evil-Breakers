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
    None,
    Light,
    Shadow,
    Water,
    Fire,
    Earth
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
    // =====================================================
    // 기본 정보
    // =====================================================

    public string character_name;

    public int character_id;

    public Element element;

    public CharacterClass characterClass;


    [TextArea]
    public string character_story;


    public Sprite character_full_art;

    public Sprite character_icon;

    public Color icon_background_color = Color.black;


    // =====================================================
    // 레벨 스탯
    // =====================================================

    public List<CharacterLevelStat> levelStatList = new();


    // =====================================================
    // 패시브
    // =====================================================

    public List<PassiveSkillData> passiveSkillList = new();


    // =====================================================
    // 대화
    // =====================================================

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


    // =====================================================
    // 대화 가져오기
    // =====================================================

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