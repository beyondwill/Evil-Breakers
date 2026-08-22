using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleData
{
    public HexNode.NodeType nodeType;
    public HexNode.ZoneType zoneType;

    public List<PlayerCharacterData> characters_in_battle_data_list = new();
    public List<EnemyCharacterInfo> enemyCharacterList = new();
    public List<InventoryItem> slots = new();
    public List<InventoryItem> leftRewards = new();

    public BattleResultVariable battle_result_variables;

    public MapData map_data = new();

    // =========================================================
    // 시간
    // =========================================================

    [SerializeField]
    private int time = 100;

    public event Action<int, int> OnTimeChanged;


    public int GetTime()
    {
        return time;
    }


    public void SetTime(
        int value,
        bool showChangeText = true)
    {
        if (time == value)
            return;

        int previousTime = time;

        time = value;

        int changeAmount =
            time - previousTime;

        OnTimeChanged?.Invoke(
            time,
            showChangeText
                ? changeAmount
                : 0
        );
    }


    public void AddTime(int value)
    {
        SetTime(time + value);
    }


    public void ReduceTime(int value)
    {
        SetTime(time - value);
    }


    // =========================================================
    // 공포도
    // =========================================================

    [SerializeField]
    private int horror = 0;

    public event Action<int, int> OnHorrorChanged;

    public int GetHorror()
    {
        return horror;
    }


    public void SetHorror(
        int value,
        bool showChangeText = true)
    {
        if (horror == value)
            return;

        int previousHorror = horror;

        horror = value;

        int changeAmount =
            horror - previousHorror;

        OnHorrorChanged?.Invoke(
            horror,
            showChangeText
                ? changeAmount
                : 0
        );
    }


    public void AddHorror(int value)
    {
        SetHorror(
            Mathf.Clamp(
                horror + value,
                0,
                100
            )
        );
    }


    public void ReduceHorror(int value)
    {
        SetHorror(horror - value);
    }


    // =========================================================
    // 생성자
    // =========================================================

    public BattleData()
    {
        time = 100;
        horror = 0;

        characters_in_battle_data_list = new();
        enemyCharacterList = new();
        slots = new();
        leftRewards = new();

        map_data = new();
    }


    public BattleData(
        List<PlayerCharacterData> character_data_list,
        MissionInfo mission)
    {
        characters_in_battle_data_list = new();

        foreach (var c in character_data_list)
        {
            characters_in_battle_data_list.Add(
                new PlayerCharacterData(c)
            );
        }

        enemyCharacterList = new();
        slots = new();
        leftRewards = new();

        battle_result_variables =
            new BattleResultVariable();

        time = 100;
        horror = 0;
    }


    // =========================================================
    // 아이템 추가
    // =========================================================

    public int AddItem(
        ItemData item,
        int amount = 1)
    {
        if (item == null || amount <= 0)
            return amount;


        foreach (var slot in slots)
        {
            if (slot == null ||
                slot.IsEmpty ||
                slot.item != item ||
                slot.amount >= item.maxStack)
            {
                continue;
            }

            int add = Mathf.Min(
                item.maxStack - slot.amount,
                amount
            );

            slot.amount += add;
            amount -= add;

            if (amount <= 0)
                return 0;
        }


        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            if (!slot.IsEmpty)
                continue;

            int add = Mathf.Min(
                item.maxStack,
                amount
            );

            slot.Set(item, add);

            amount -= add;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }


    // =========================================================
    // 아이템 추가 가능 여부
    // =========================================================

    public bool CanAddItem(ItemData item)
    {
        if (item == null)
            return false;

        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            if (slot.IsEmpty)
                return true;

            if (slot.item == item &&
                slot.amount < item.maxStack)
            {
                return true;
            }
        }

        return false;
    }


    // =========================================================
    // 아이템 제거
    // =========================================================

    public void RemoveItem(int index)
    {
        if (index < 0 ||
            index >= slots.Count)
        {
            return;
        }

        slots.RemoveAt(index);

        slots.Add(
            new InventoryItem()
        );
    }
}
