using System;
using System.Collections.Generic;
using UnityEngine;

#region Stat

// 불변 스탯
public enum CharacterBaseStatType
{
    Attack,             // 공격력
    MaxHealth,          // 최대 생명력
    MaxEnergy,          // 최대 에너지
    DrawCardCount,      // 카드 뽑기 수
    AttackOrder,        // 공격 순서
    Accuracy,           // 정확도
    RESPoison,          // 독 저항
    RESBlood,           // 적혈 저항
    RESStun,            // 기절 저항
    RESConfusion,       // 혼란 저항
    RESCurse,           // 저주 저항
    RESInfection,       // 감염 저항
    DEF,                // 방어력
    Dodge               // 회피율
}


// 런타임 스탯
public enum CharacterRuntimeStatType
{
    CurrentHealth,      // 현재 생명력
    CurrentEnergy,      // 현재 에너지
    CurrentBlock        // 현재 방어도
}


// 버프 타입
public enum CharacterBuffType
{
    Strength,           // 힘 (주는 피해량 증가)
    Dexterity,          // 민첩 (방어 증가)
    Draw,               // 카드 뽑기
    Evasion,            // 회피력
    Stun,               // 기절
    Acceleration,       // 가속 (공격 속도)
    Toughness           // 강인함
}

#endregion


#region Name Config


// 기본 스탯 이름
[Serializable]
public class CharacterBaseStatSort
{
    public CharacterBaseStatType type;
    public string statName;
}


// 런타임 스탯 이름
[Serializable]
public class CharacterRuntimeStatSort
{
    public CharacterRuntimeStatType type;
    public string statName;
}


// 버프 이름
[Serializable]
public class CharacterBuffSort
{
    public CharacterBuffType type;
    public string buffName;
    public Sprite buffIcon;
    public bool isDebuff;
}

#endregion


#region Stat Config


[CreateAssetMenu(menuName = "Config/StatConfig")]
public class StatConfig : ScriptableObject
{
    // 기본 스탯 이름
    public List<CharacterBaseStatSort> baseStatSortList = new();


    // 런타임 스탯 이름
    public List<CharacterRuntimeStatSort> runtimeStatSortList = new();


    // 버프 이름
    public List<CharacterBuffSort> buffList = new();



    #region Find


    // 기본 스탯 찾기
    public CharacterBaseStatSort FindBaseStat(CharacterBaseStatType type)
    {
        return baseStatSortList.Find(x => x.type == type);
    }


    // 런타임 스탯 찾기
    public CharacterRuntimeStatSort FindRuntimeStat(CharacterRuntimeStatType type)
    {
        return runtimeStatSortList.Find(x => x.type == type);
    }


    // 버프 찾기
    public CharacterBuffSort FindBuff(CharacterBuffType type)
    {
        return buffList.Find(x => x.type == type);
    }


    #endregion



    #region Validate


    private void OnValidate()
    {
        ValidateBaseStat();
        ValidateRuntimeStat();
        ValidateBuff();
    }



    // 기본 스탯 검증
    private void ValidateBaseStat()
    {
        baseStatSortList.RemoveAll(x =>
            !Enum.IsDefined(typeof(CharacterBaseStatType), x.type));


        foreach (CharacterBaseStatType type in Enum.GetValues(typeof(CharacterBaseStatType)))
        {
            if (baseStatSortList.Exists(x => x.type == type))
                continue;


            baseStatSortList.Add(new CharacterBaseStatSort
            {
                type = type,
                statName = type.ToString()
            });
        }


        baseStatSortList.Sort((a, b) =>
            a.type.CompareTo(b.type));
    }



    // 런타임 스탯 검증
    private void ValidateRuntimeStat()
    {
        runtimeStatSortList.RemoveAll(x =>
            !Enum.IsDefined(typeof(CharacterRuntimeStatType), x.type));


        foreach (CharacterRuntimeStatType type in Enum.GetValues(typeof(CharacterRuntimeStatType)))
        {
            if (runtimeStatSortList.Exists(x => x.type == type))
                continue;


            runtimeStatSortList.Add(new CharacterRuntimeStatSort
            {
                type = type,
                statName = type.ToString()
            });
        }


        runtimeStatSortList.Sort((a, b) =>
            a.type.CompareTo(b.type));
    }



    // 버프 검증
    private void ValidateBuff()
    {
        buffList.RemoveAll(x =>
            !Enum.IsDefined(typeof(CharacterBuffType), x.type));


        foreach (CharacterBuffType type in Enum.GetValues(typeof(CharacterBuffType)))
        {
            if (buffList.Exists(x => x.type == type))
                continue;


            buffList.Add(new CharacterBuffSort
            {
                type = type,
                buffName = type.ToString()
            });
        }


        buffList.Sort((a, b) =>
            a.type.CompareTo(b.type));
    }


    #endregion
}

#endregion