using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ShopUI shopUI;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }



    private void Start()
    {
        RefreshUI();
    }



    public void RefreshUI()
    {
        shopUI.Show(
            DataManager.Instance.GetAllData.main_data.shopData.shop_items
        );

        shopUI.ShowInventory();
    }





    // ==========================
    // 구매
    // ==========================

    public void BuyItem(int index)
    {
        var shopItems =
            DataManager.Instance.GetAllData.main_data.shopData.shop_items;



        if (index < 0 || index >= shopItems.Count)
            return;



        InventoryItem item =
            shopItems[index];



        if (item == null || item.item == null)
            return;



        // 재고 없음
        if (item.amount <= 0)
            return;



        int price =
            item.item.sellPrice;



        // 돈 부족
        if (DataManager.Instance.GetAllData.main_data.money < price)
            return;




        // 인벤토리 추가
        DataManager.Instance.GetAllData.main_data.AddInventoryItem(
                item.item,
                1
            );


        // 돈 차감
        DataManager.Instance.GetAllData.main_data.money -= price;




        // 상점 재고 감소
        item.amount--;



        // 여기서 삭제 금지
        // if(item.amount <= 0)
        //     shopItems.RemoveAt(index);



        RefreshUI();


        DataManager.Instance.SaveData();
    }







    // ==========================
    // 판매
    // ==========================

    public void SellItem(int index)
    {
        var inventory =
            DataManager.Instance.GetAllData.main_data.inventoryItemList;



        if (index < 0 || index >= inventory.Count)
            return;



        InventoryItem item =
            inventory[index];



        if (item == null || item.IsEmpty)
            return;



        int price =
            item.item.sellPrice;



        // 돈 증가
        DataManager.Instance.GetAllData.main_data.money += price;



        // 개수 감소
        item.amount--;



        // 플레이어 인벤토리는 0이면 제거
        if (item.amount <= 0)
        {
            item.Clear();
        }



        RefreshUI();


        DataManager.Instance.SaveData();
    }
}