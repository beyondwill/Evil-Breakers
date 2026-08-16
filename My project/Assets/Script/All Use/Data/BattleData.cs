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

    public MapData map_data = new();

    public int time = 100;

    public BattleResultVariable battle_result_variables;

    // 시간 변경 이벤트
    // 첫 번째 값 : 변경 후 시간
    // 두 번째 값 : 실제 변경량
    public event Action<int, int> OnTimeChanged;


    public BattleData()
    {
    }


    public BattleData(
        List<PlayerCharacterData> character_data_list,
        MissionInfo mission)
    {
        characters_in_battle_data_list = new();

        foreach (var c in character_data_list)
        {
            characters_in_battle_data_list.Add(
                new PlayerCharacterData(c));
        }

        enemyCharacterList = new();
        slots = new();
        leftRewards = new();

        battle_result_variables =
            new BattleResultVariable();

        time = 100;
    }


    // ==========================================
    // 시간 설정
    // ==========================================

    public void SetTime(int value, bool showChangeText = true)
    {
        if (time == value)
            return;

        int previousTime = time;

        time = value;

        int changeAmount = time - previousTime;

        // 텍스트 표시 여부까지 전달
        OnTimeChanged?.Invoke(
            time,
            showChangeText ? changeAmount : 0
        );
    }


    // ==========================================
    // 시간 증가
    // ==========================================

    public void AddTime(int value)
    {
        SetTime(time + value);
    }


    // ==========================================
    // 시간 감소
    // ==========================================

    public void ReduceTime(int value)
    {
        SetTime(time - value);
    }


    // ==========================================
    // 아이템 추가
    // ==========================================

    public int AddItem(
        ItemData item,
        int amount = 1)
    {
        if (item == null || amount <= 0)
            return amount;


        // ==========================================
        // 기존 아이템 스택에 추가
        // ==========================================

        foreach (var slot in slots)
        {
            if (slot == null ||
                slot.IsEmpty ||
                slot.item != item ||
                slot.amount >= item.maxStack)
                continue;


            int add = Mathf.Min(
                item.maxStack - slot.amount,
                amount);


            slot.amount += add;
            amount -= add;


            if (amount <= 0)
                return 0;
        }


        // ==========================================
        // 빈 슬롯에 추가
        // ==========================================

        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            if (!slot.IsEmpty)
                continue;


            int add = Mathf.Min(
                item.maxStack,
                amount);


            slot.Set(item, add);
            amount -= add;


            if (amount <= 0)
                return 0;
        }


        return amount;
    }


    // ==========================================
    // 아이템 추가 가능 여부
    // ==========================================

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
                return true;
        }


        return false;
    }


    // ==========================================
    // 아이템 제거
    // ==========================================

    public void RemoveItem(int index)
    {
        if (index < 0 ||
            index >= slots.Count)
            return;


        slots.RemoveAt(index);

        slots.Add(new InventoryItem());
    }
}