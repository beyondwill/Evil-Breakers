using System;
using System.Collections.Generic;
using UnityEngine;

public enum Situation
{
    MainNormal,                     // 메인 화면 평상시
    MainHurt,                       // 메인 화면 부상
    MainStress,                     // 메인 화면 스트레스

    PartyFormation,                 // 파티 구성시
    BattleMapNormal,                // 전투 맵 평상시
    BattleMapHurt,                  // 전투 맵 다칠시
    BattleMapStress,                // 전투 맵 스트레스

    CantStartCard,                  // 카드 낼 수 없음
    InvalidTarget,                  // 잘못된 대상
    BattleStart                     // 전투 개시
}


// 속성
public enum Element
{
    None,
    Light,
    Shadow,
    Water,
    Fire,
    Earth
}


public enum CharacterClass
{
    None,
    Character1,
    Character2,
    Character3,
    Character4
}


// =========================================================
// 캐릭터 레벨 스탯
// =========================================================

[Serializable]
public class CharacterLevelStat
{
    // 해당 스탯이 적용되는 레벨
    public int level;

    // 해당 레벨의 스탯
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

public abstract class CharacterInfo : DataEntity
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

    // 레벨별 스탯
    public List<CharacterLevelStat> levelStatList = new();


    // =====================================================
    // 기타
    // =====================================================

    public List<PassiveEvent> passiveEventList;
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


        // 현재 레벨의 데이터 찾기
        CharacterLevelStat levelStat =
            levelStatList.Find(
                x => x != null &&
                     x.level == level);


        if (levelStat == null ||
            levelStat.statList == null)
        {
            return 0;
        }


        // 해당 레벨의 해당 스탯 찾기
        CharacterBaseStatValue stat =
            levelStat.statList.Find(
                x => x.type == type);


        if (stat == null)
            return 0;


        return stat.value;
    }


    // =====================================================
    // 상황별 대사 가져오기
    // =====================================================

    public string GetDialogue(
        Situation situation)
    {
        if (dialogues == null)
            return null;


        CharacterDialogue dialogue =
            dialogues.Find(
                x => x.situation == situation);


        if (dialogue == null ||
            dialogue.dialogue == null ||
            dialogue.dialogue.Count == 0)
        {
            return null;
        }


        return dialogue.dialogue[
            UnityEngine.Random.Range(
                0,
                dialogue.dialogue.Count)
        ];
    }
}