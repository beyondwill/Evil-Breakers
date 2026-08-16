using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Transform shopGrid;
    [SerializeField] private Transform inventoryGrid;

    [Header("Prefab")]
    [SerializeField] private IconButton iconPrefab;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI money;


    private readonly List<IconButton> shopButtons = new();
    private readonly List<IconButton> inventoryButtons = new();



    // ==========================
    // 상점 아이템 표시
    // ==========================

    public void Show(List<InventoryItem> items)
    {
        money.text = DataManager.Instance.GetAllData.main_data.money.ToString();
        Clear(shopButtons);


        for (int i = 0; i < items.Count; i++)
        {
            int index = i;

            InventoryItem inventoryItem = items[i];


            if (inventoryItem == null || inventoryItem.item == null)
                continue;



            IconButton button =
                Instantiate(iconPrefab, shopGrid);



            button.ButtonInit(
                index,
                Color.black,
                inventoryItem.item.icon,
                true
            );


            // 개수 표시
            button.SetAmount(
                inventoryItem.amount
            );



            // 구매
            button.ActionAdd(() =>
            {
                ShopManager.Instance.BuyItem(index);
            });



            // 툴팁
            button.PointerEnterAdd(() =>
            {
                TooltipUI.Instance.Show(
                    inventoryItem.item,
                    button.transform as RectTransform
                );
            });



            button.PointerExitAdd(() =>
            {
                TooltipUI.Instance.Hide();
            });



            shopButtons.Add(button);
        }
    }





    // ==========================
    // 플레이어 인벤토리 표시
    // ==========================

    public void ShowInventory()
    {
        Clear(inventoryButtons);


        List<InventoryItem> items =
            DataManager.Instance.GetAllData.main_data.inventoryItemList;



        for (int i = 0; i < items.Count; i++)
        {
            int index = i;

            InventoryItem inventoryItem = items[i];


            if (inventoryItem == null || inventoryItem.item == null)
                continue;



            IconButton button =
                Instantiate(iconPrefab, inventoryGrid);



            button.ButtonInit(
                index,
                Color.black,
                inventoryItem.item.icon,
                true
            );



            // 개수
            button.SetAmount(
                inventoryItem.amount
            );



            // 판매
            button.ActionAdd(() =>
            {
                ShopManager.Instance.SellItem(index);
            });



            // 툴팁
            button.PointerEnterAdd(() =>
            {
                TooltipUI.Instance.Show(
                    inventoryItem.item,
                    button.transform as RectTransform
                );
            });



            button.PointerExitAdd(() =>
            {
                TooltipUI.Instance.Hide();
            });



            inventoryButtons.Add(button);
        }
    }





    // ==========================
    // 전체 갱신
    // ==========================

    public void Refresh()
    {
        Show(
            DataManager.Instance.GetAllData.main_data.shopData.shop_items
        );

        ShowInventory();
    }





    private void Clear(List<IconButton> list)
    {
        foreach (IconButton button in list)
        {
            if (button == null)
                continue;


            button.ActionRemove();
            button.PointerEnterRemove();
            button.PointerExitRemove();


            Destroy(button.gameObject);
        }


        list.Clear();
    }
}