using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MainData
{
    public List<PlayerCharacterData> player_character_data_list = new();

    public int money = 0;
    public int day = 0;

    public List<CardData> storage_cards_list = new();
    public List<InventoryItem> inventoryItemList = new();

    public List<EquipmentInfo> equipmentInfoList = new();
    public List<MapData> mapDataList = new();

    public ShopData shopData = new();



    // 메인 화면 인벤토리 추가
    // maxStack 무시
    // 슬롯 제한 없음
    public void AddInventoryItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return;



        // 같은 아이템 찾기
        foreach (var slot in inventoryItemList)
        {
            if (slot == null)
                continue;


            if (slot.IsEmpty)
                continue;


            if (slot.item != item)
                continue;


            // 메인 인벤토리는 스택 제한 없음
            slot.amount += amount;

            return;
        }



        // 없으면 새 슬롯 생성
        InventoryItem newItem = new InventoryItem();

        newItem.Set(item, amount);

        inventoryItemList.Add(newItem);
    }





    // 아이템 제거
    public bool RemoveInventoryItem(int index, int amount = 1)
    {
        if (index < 0 || index >= inventoryItemList.Count)
            return false;



        InventoryItem item = inventoryItemList[index];


        if (item == null || item.IsEmpty)
            return false;



        item.amount -= amount;



        if (item.amount <= 0)
        {
            item.Clear();
            inventoryItemList.RemoveAt(index);
        }


        return true;
    }





    // 아이템 판매/존재 확인용
    public InventoryItem GetInventoryItem(int index)
    {
        if (index < 0 || index >= inventoryItemList.Count)
            return null;


        return inventoryItemList[index];
    }
}