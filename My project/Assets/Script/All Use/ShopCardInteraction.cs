public class ShopCardInteraction : IDeckCardInteraction
{
    private ShopManager shopManager;
    private bool isShop;


    public ShopCardInteraction(
        ShopManager shopManager,
        bool isShop)
    {
        this.shopManager = shopManager;
        this.isShop = isShop;
    }


    // =========================================================
    // 좌클릭
    // =========================================================

    public void LeftClick(CardInteraction card)
    {
        if (card == null)
            return;


        shopManager.CardClick(
            card.DeckCard,
            isShop
        );
    }


    // =========================================================
    // 우클릭
    // =========================================================

    public void RightClick(CardData card)
    {

    }


    // =========================================================
    // 드래그
    // =========================================================

    public void BeginDrag(CardData card)
    {

    }


    public void Drag(CardData card)
    {

    }


    public void EndDrag(CardData card)
    {

    }
}