using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }


    // =========================================================
    // 일반 상점 UI
    // =========================================================

    [Header("Normal Shop")]

    [SerializeField] private ShopUI shopUI;


    // =========================================================
    // 카드 상점 UI
    // =========================================================

    [Header("Card Shop")]

    [SerializeField] private CardManageBox shopCardBox;

    [SerializeField] private CardManageBox storageCardBox;


    // =========================================================
    // 카드 상호작용
    // =========================================================

    private ShopCardInteraction shopCardInteraction;

    private ShopCardInteraction storageCardInteraction;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;


        // -----------------------------------------------------
        // 상점 카드
        // -----------------------------------------------------

        shopCardInteraction =
            new ShopCardInteraction(
                this,
                true
            );


        // -----------------------------------------------------
        // 창고 카드
        // -----------------------------------------------------

        storageCardInteraction =
            new ShopCardInteraction(
                this,
                false
            );
    }


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        RefreshUI();
    }


    // =========================================================
    // 전체 UI 갱신
    // =========================================================

    public void RefreshUI()
    {
        if (DataManager.Instance == null)
            return;


        var mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        if (mainData == null)
            return;


        // =====================================================
        // 일반 상점
        // =====================================================

        if (shopUI != null)
        {
            shopUI.Show(
                mainData
                    .shopData
                    .shop_items
            );


            shopUI.ShowInventory();
        }


        // =====================================================
        // 카드 상점
        // =====================================================

        RefreshCardUI();
    }


    // =========================================================
    // 카드 UI 갱신
    // =========================================================

    private void RefreshCardUI()
    {
        if (DataManager.Instance == null)
            return;


        var mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        if (mainData == null)
            return;


        // -----------------------------------------------------
        // 상점 카드
        // -----------------------------------------------------

        if (shopCardBox != null)
        {
            shopCardBox.ShowCards(
                mainData
                    .shopData
                    .card_items,

                shopCardInteraction
            );
        }


        // -----------------------------------------------------
        // 창고 카드
        // -----------------------------------------------------

        if (storageCardBox != null)
        {
            storageCardBox.ShowCards(
                mainData
                    .storage_cards_list,

                storageCardInteraction
            );
        }
    }


    // =========================================================
    // 카드 클릭
    // =========================================================

    public void CardClick(
        CardData card,
        bool isShop)
    {
        if (card == null)
            return;


        if (isShop)
        {
            BuyCard(card);
        }
        else
        {
            SellCard(card);
        }
    }


    // =========================================================
    // 카드 구매
    // =========================================================

    private void BuyCard(CardData card)
    {
        var mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        var shopCards =
            mainData
                .shopData
                .card_items;


        var storageCards =
            mainData
                .storage_cards_list;


        // -----------------------------------------------------
        // 상점에 실제 존재하는 카드인지 확인
        // -----------------------------------------------------

        if (!shopCards.Contains(card))
            return;


        // -----------------------------------------------------
        // 가격
        // -----------------------------------------------------

        int price =
            card.buy_card_cost;


        // -----------------------------------------------------
        // 돈 부족
        // -----------------------------------------------------

        if (mainData.money < price)
            return;


        // -----------------------------------------------------
        // 돈 차감
        // -----------------------------------------------------

        mainData.money -= price;


        // -----------------------------------------------------
        // 상점 → 창고
        // -----------------------------------------------------

        shopCards.Remove(card);


        storageCards.Add(card);


        // -----------------------------------------------------
        // UI 갱신
        // -----------------------------------------------------

        RefreshUI();


        // -----------------------------------------------------
        // 저장
        // -----------------------------------------------------

        DataManager.Instance.SaveData();
    }


    // =========================================================
    // 카드 판매
    // =========================================================

    private void SellCard(CardData card)
    {
        var mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        var shopCards =
            mainData
                .shopData
                .card_items;


        var storageCards =
            mainData
                .storage_cards_list;


        // -----------------------------------------------------
        // 창고에 실제 존재하는 카드인지 확인
        // -----------------------------------------------------

        if (!storageCards.Contains(card))
            return;


        // -----------------------------------------------------
        // 판매 가격
        // -----------------------------------------------------

        int price =
            card.sell_card_cost;


        // -----------------------------------------------------
        // 돈 증가
        // -----------------------------------------------------

        mainData.money += price;


        // -----------------------------------------------------
        // 창고 → 상점
        // -----------------------------------------------------

        storageCards.Remove(card);


        shopCards.Add(card);


        // -----------------------------------------------------
        // UI 갱신
        // -----------------------------------------------------

        RefreshUI();


        // -----------------------------------------------------
        // 저장
        // -----------------------------------------------------

        DataManager.Instance.SaveData();
    }


    // =========================================================
    // 일반 아이템 구매
    // =========================================================

    public void BuyItem(int index)
    {
        var shopItems =
            DataManager.Instance
                .GetAllData
                .main_data
                .shopData
                .shop_items;


        if (index < 0 ||
            index >= shopItems.Count)
            return;


        InventoryItem item =
            shopItems[index];


        if (item == null ||
            item.item == null)
            return;


        // -----------------------------------------------------
        // 재고 없음
        // -----------------------------------------------------

        if (item.amount <= 0)
            return;


        int price =
            item.item.sellPrice;


        // -----------------------------------------------------
        // 돈 부족
        // -----------------------------------------------------

        if (DataManager.Instance
                .GetAllData
                .main_data
                .money < price)
            return;


        // -----------------------------------------------------
        // 인벤토리 추가
        // -----------------------------------------------------

        DataManager.Instance
            .GetAllData
            .main_data
            .AddInventoryItem(
                item.item,
                1
            );


        // -----------------------------------------------------
        // 돈 차감
        // -----------------------------------------------------

        DataManager.Instance
            .GetAllData
            .main_data
            .money -= price;


        // -----------------------------------------------------
        // 재고 감소
        // -----------------------------------------------------

        item.amount--;


        // -----------------------------------------------------
        // UI 갱신
        // =====================================================

        RefreshUI();


        // -----------------------------------------------------
        // 저장
        // -----------------------------------------------------

        DataManager.Instance.SaveData();
    }


    // =========================================================
    // 일반 아이템 판매
    // =========================================================

    public void SellItem(int index)
    {
        var inventory =
            DataManager.Instance
                .GetAllData
                .main_data
                .inventoryItemList;


        if (index < 0 ||
            index >= inventory.Count)
            return;


        InventoryItem item =
            inventory[index];


        if (item == null ||
            item.IsEmpty)
            return;


        int price =
            item.item.sellPrice;


        // -----------------------------------------------------
        // 돈 증가
        // -----------------------------------------------------

        DataManager.Instance
            .GetAllData
            .main_data
            .money += price;


        // -----------------------------------------------------
        // 개수 감소
        // -----------------------------------------------------

        item.amount--;


        // -----------------------------------------------------
        // 0이면 제거
        // -----------------------------------------------------

        if (item.amount <= 0)
        {
            item.Clear();
        }


        // -----------------------------------------------------
        // UI 갱신
        // -----------------------------------------------------

        RefreshUI();


        // -----------------------------------------------------
        // 저장
        // -----------------------------------------------------

        DataManager.Instance.SaveData();
    }
}