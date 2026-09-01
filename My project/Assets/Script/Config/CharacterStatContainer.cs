using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

#region Stat Value

[Serializable]
public class CharacterBaseStatValue
{
    public CharacterBaseStatType type;
    public float value;
}


[Serializable]
public class CharacterRuntimeStatValue
{
    public CharacterRuntimeStatType type;
    public float value;
}


[Serializable]
public class CharacterBuffValue
{
    public CharacterBuffType type;
    public float value;
}

#endregion


#region Stat Container

[System.Serializable]
public class CharacterStatContainer
{
    public List<CharacterBaseStatValue> baseStatList = new();
    public List<CharacterRuntimeStatValue> runtimeStatList = new();
    public List<CharacterBuffValue> buffList = new();


    // 기본 스탯 가져오기
    public float GetBaseStat(CharacterBaseStatType type)
    {
        //Debug.Log("찾는 타입 : " + type);

        //foreach (var stat in baseStatList)
        //{
        //    Debug.Log(
        //        "현재 타입 : " + stat.type +
        //        " 값 : " + stat.value
        //    );
        //}

        CharacterBaseStatValue result =
            baseStatList.Find(x => x.type == type);

        return result != null ? result.value : 0;
    }


    // 런타임 스탯 가져오기
    public float GetRuntimeStat(CharacterRuntimeStatType type)
    {
        CharacterRuntimeStatValue stat =
            runtimeStatList.Find(x => x.type == type);

        return stat != null ? stat.value : 0;
    }


    // 버프 가져오기
    public float GetBuff(CharacterBuffType type)
    {
        CharacterBuffValue buff =
            buffList.Find(x => x.type == type);

        return buff != null ? buff.value : 0;
    }

    // 기본 스텟 합치기
    public void MergeBaseStatList(List<CharacterBaseStatValue> otherList)
    {
        foreach (CharacterBaseStatValue stat in otherList)
        {
            AddBaseStat(stat.type, stat.value);
        }
    }

    // 기본 스탯 변경
    public void AddBaseStat(CharacterBaseStatType type, float value)
    {
        CharacterBaseStatValue stat =
            baseStatList.Find(x => x.type == type);

        if (stat == null)
        {
            baseStatList.Add(new CharacterBaseStatValue
            {
                type = type,
                value = value
            });
        }
        else
        {
            stat.value += value;
        }
    }


    // 런타임 변경
    public void AddRuntimeStat(CharacterRuntimeStatType type, float value)
    {
        CharacterRuntimeStatValue stat =
            runtimeStatList.Find(x => x.type == type);

        if (stat == null)
        {
            runtimeStatList.Add(new CharacterRuntimeStatValue
            {
                type = type,
                value = value
            });
        }
        else
        {
            stat.value += value;
        }
    }


    // 버프 변경
    public void AddBuff(CharacterBuffType type, float value)
    {
        CharacterBuffValue buff =
            buffList.Find(x => x.type == type);

        if (buff == null)
        {
            buffList.Add(new CharacterBuffValue
            {
                type = type,
                value = value
            });
        }
        else
        {
            buff.value += value;
        }
    }
}

#endregion