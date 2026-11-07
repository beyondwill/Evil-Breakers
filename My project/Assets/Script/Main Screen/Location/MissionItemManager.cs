using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionItemManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform purchase_item_content;
    [SerializeField] private Transform carry_item_content;

    [SerializeField] private IconButton item_icon_prefab;


    [Header("Settings")]
    [SerializeField] private Color icon_color = Color.white;


    // 구매할 아이템
    private List<InventoryItem> purchaseItemList =
        new List<InventoryItem>();


    // 챙길 아이템
    private List<InventoryItem> carryItemList =
        new List<InventoryItem>();


    #region Init

    private void OnEnable()
    {
        InitializeItemList();

        RefreshItemUI();
    }


    // 아이템 리스트 초기화
    private void InitializeItemList()
    {
        purchaseItemList.Clear();
        carryItemList.Clear();


        // 플레이어 인벤토리 가져오기
        List<InventoryItem> inventoryList =
            DataManager.Instance
                .GetMainData
                .inventoryItemList;


        if (inventoryList == null)
            return;


        foreach (InventoryItem data in inventoryList)
        {
            if (data == null ||
                data.IsEmpty ||
                data.amount <= 0)
                continue;


            // 소모품만 가져오기
            if (data.item is not ConsumptionItemInfo)
                continue;


            AddItemToList(
                purchaseItemList,
                data.item,
                data.amount,
                true
            );
        }
    }


    // 아이템을 리스트에 추가
    private void AddItemToList(
        List<InventoryItem> itemList,
        ItemData itemData,
        int amount,
        bool unlimitedStack = false)
    {
        if (itemData == null || amount <= 0)
            return;


        // 최대 중첩 제한이 없는 경우
        if (unlimitedStack)
        {
            InventoryItem targetItem = itemList.Find(
                x => x != null
                && !x.IsEmpty
                && x.item == itemData
            );

            if (targetItem != null)
            {
                targetItem.amount += amount;
            }
            else
            {
                itemList.Add(
                    new InventoryItem(itemData, amount)
                );
            }

            return;
        }


        // 최대 중첩 제한이 있는 경우
        int maxStack = Mathf.Max(1, itemData.maxStack);


        // 기존 동일 아이템 슬롯에 먼저 합산
        foreach (InventoryItem item in itemList)
        {
            if (item == null ||
                item.IsEmpty ||
                item.item != itemData)
                continue;


            int availableAmount = maxStack - item.amount;

            if (availableAmount <= 0)
                continue;


            int addAmount = Mathf.Min(
                amount,
                availableAmount
            );

            item.amount += addAmount;
            amount -= addAmount;


            if (amount <= 0)
                return;
        }


        // 남은 수량은 새 슬롯에 추가
        while (amount > 0)
        {
            int addAmount = Mathf.Min(
                amount,
                maxStack
            );

            itemList.Add(
                new InventoryItem(itemData, addAmount)
            );

            amount -= addAmount;
        }
    }

    #endregion


    #region UI

    private void RefreshItemUI()
    {
        CreatePurchaseItemList();
        CreateCarryItemList();
    }


    // 구매할 아이템 목록
    private void CreatePurchaseItemList()
    {
        ClearContent(purchase_item_content);

        for (int i = 0; i < purchaseItemList.Count; i++)
        {
            InventoryItem item = purchaseItemList[i];

            if (item == null || item.IsEmpty)
                continue;


            CreateItemIcon(
                item,
                purchase_item_content,
                () => MoveItem(
                    purchaseItemList,
                    carryItemList,
                    item
                )
            );
        }
    }


    // 챙길 아이템 목록
    private void CreateCarryItemList()
    {
        ClearContent(carry_item_content);

        for (int i = 0; i < carryItemList.Count; i++)
        {
            InventoryItem item = carryItemList[i];

            if (item == null || item.IsEmpty)
                continue;


            CreateItemIcon(
                item,
                carry_item_content,
                () => MoveItem(
                    carryItemList,
                    purchaseItemList,
                    item
                )
            );
        }
    }


    // 아이콘 생성
    private void CreateItemIcon(
        InventoryItem item,
        Transform content,
        Action clickAction)
    {
        IconButton icon = Instantiate(
            item_icon_prefab,
            content
        );

        icon.ButtonInit(
            0,
            icon_color,
            item.item.icon
        );

        icon.SetAmount(item.amount);

        icon.SetItemData(item.item);

        icon.ActionAdd(clickAction);
    }


    // UI 자식 삭제
    private void ClearContent(Transform content)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion


    #region Item

    // 아이템 한 개 이동
    private void MoveItem(
        List<InventoryItem> from,
        List<InventoryItem> to,
        InventoryItem item)
    {
        if (item == null || item.amount <= 0)
            return;


        // 이동할 아이템 정보 저장
        ItemData itemData = item.item;


        // 기존 목록에서 한 개 감소
        item.amount--;


        // 구매 목록으로 이동하는 경우 최대 중첩 제한 무시
        bool unlimitedStack = to == purchaseItemList;


        // 받을 목록에서 동일한 아이템 검색
        InventoryItem targetItem = to.Find(
            x => x != null
            && !x.IsEmpty
            && x.item == itemData
            && (
                unlimitedStack ||
                x.amount < Mathf.Max(1, itemData.maxStack)
            )
        );


        if (targetItem != null)
        {
            // 기존 슬롯에 한 개 추가
            targetItem.amount++;
        }
        else
        {
            // 새로운 슬롯 생성
            to.Add(
                new InventoryItem(itemData, 1)
            );
        }


        // 수량이 0이면 기존 슬롯 제거
        if (item.amount <= 0)
        {
            from.Remove(item);
        }


        // UI 갱신
        RefreshItemUI();
    }

    #endregion


    #region Reset

    // 아이템 선택 초기화
    public void ResetItemList()
    {
        InitializeItemList();

        RefreshItemUI();
    }

    #endregion


    #region Getter

    // 구매할 아이템 목록
    public List<InventoryItem> GetPurchaseItemList()
    {
        return purchaseItemList;
    }


    // 챙길 아이템 목록
    public List<InventoryItem> GetCarryItemList()
    {
        return carryItemList;
    }

    #endregion
}