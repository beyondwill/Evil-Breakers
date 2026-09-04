public class DeckCardInteraction : IDeckCardInteraction
{
    private DeckManager deckManager;
    private bool isPlayer;


    public DeckCardInteraction(
        DeckManager deckManager,
        bool isPlayer)
    {
        this.deckManager = deckManager;
        this.isPlayer = isPlayer;
    }


    // =========================================================
    // 좌클릭
    // =========================================================

    public void LeftClick(CardInteraction card)
    {
        if (card == null)
            return;


        deckManager.MoveCard(
            card.DeckCard,
            isPlayer
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