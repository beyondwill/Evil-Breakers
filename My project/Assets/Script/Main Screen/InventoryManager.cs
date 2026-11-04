
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum InventoryCategory
{
    All,
    Weapon,
    Armor,
    Accessory,
    Relic
}

public class InventoryManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private IconButton iconPrefab;
    [SerializeField] private Transform itemIconContent;


    [Header("Item Information")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStoryText;


    [Header("Icon Settings")]
    [SerializeField] private Color iconColor = Color.white;


    // 생성된 아이콘
    private List<IconButton> iconList =
        new List<IconButton>();


    // 현재 선택된 아이콘
    private IconButton selectedButton;


    // 현재 선택된 아이템
    private InventoryItem selectedItem;


    // 현재 카테고리
    private InventoryCategory currentCategory =
        InventoryCategory.All;


    #region Inventory

    // 인벤토리 갱신
    public void RefreshInventory()
    {
        ClearIcons();

        List<InventoryItem> inventory =
            DataManager.Instance.GetMainData.inventoryItemList;


        for (int i = 0; i < inventory.Count; i++)
        {
            InventoryItem item = inventory[i];

            // 빈 슬롯 검사
            if (item == null || item.IsEmpty)
                continue;

            // 카테고리 검사
            if (!IsCategoryMatch(item.item))
                continue;

            // 아이콘 생성
            CreateIcon(item, i);
        }

        ClearInformation();
    }


    // 아이콘 생성
    private void CreateIcon(
        InventoryItem item,
        int index)
    {
        IconButton icon = Instantiate(
            iconPrefab,
            itemIconContent
        );

        icon.ButtonInit(
            index,
            iconColor,
            item.item.icon
        );

        icon.SetAmount(item.amount);

        // 클릭 이벤트
        icon.ActionAdd(() =>
        {
            SelectIcon(icon, item);
        });

        iconList.Add(icon);
    }


    // 기존 아이콘 삭제
    private void ClearIcons()
    {
        selectedButton = null;
        selectedItem = null;

        for (int i = 0; i < iconList.Count; i++)
        {
            if (iconList[i] != null)
                Destroy(iconList[i].gameObject);
        }

        iconList.Clear();
    }

    #endregion


    #region Category

    // 카테고리 변경
    public void SetCategory(InventoryCategory category)
    {
        currentCategory = category;

        RefreshInventory();
    }


    // 카테고리 필터
    private bool IsCategoryMatch(ItemData item)
    {
        if (item == null)
            return false;

        switch (currentCategory)
        {
            case InventoryCategory.All:
                return true;

            case InventoryCategory.Weapon:
                return item.itemType == ItemType.Equipment
                    && item.equipmentType == EquipmentType.Weapon;

            case InventoryCategory.Armor:
                return item.itemType == ItemType.Equipment
                    && item.equipmentType == EquipmentType.Armor;

            case InventoryCategory.Accessory:
                return item.itemType == ItemType.Equipment
                    && item.equipmentType == EquipmentType.Accessory;

            case InventoryCategory.Relic:
                return item.itemType == ItemType.Relic;
        }

        return false;
    }


    // 상단 버튼 연결용 함수

    public void ShowAll()
    {
        SetCategory(InventoryCategory.All);
    }

    public void ShowWeapon()
    {
        SetCategory(InventoryCategory.Weapon);
    }

    public void ShowArmor()
    {
        SetCategory(InventoryCategory.Armor);
    }

    public void ShowAccessory()
    {
        SetCategory(InventoryCategory.Accessory);
    }

    public void ShowRelic()
    {
        SetCategory(InventoryCategory.Relic);
    }

    #endregion


    #region Selection

    // 아이콘 선택
    private void SelectIcon(
        IconButton button,
        InventoryItem item)
    {
        // 이전 선택 해제
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }

        // 새로운 선택
        selectedButton = button;
        selectedItem = item;

        // 테두리 활성화
        selectedButton.SetSelected(true);

        // 아이템 정보 출력
        ShowInformation(item.item);
    }


    // 아이템 정보 표시
    private void ShowInformation(ItemData item)
    {
        if (item == null)
            return;

        // 이미지
        if (itemImage != null)
        {
            itemImage.sprite = item.icon;
            itemImage.enabled = item.icon != null;
        }

        // 이름
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        // 설명
        if (itemDescriptionText != null)
            itemDescriptionText.text = item.description;
    }


    // 정보 초기화
    private void ClearInformation()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = "";

        if (itemDescriptionText != null)
            itemDescriptionText.text = "";

        if (itemStoryText != null)
            itemStoryText.text = "";
    }

    #endregion


    #region Getter

    public InventoryItem GetSelectedItem()
    {
        return selectedItem;
    }


    public ItemData GetSelectedItemData()
    {
        return selectedItem != null
            ? selectedItem.item
            : null;
    }

    #endregion
}
