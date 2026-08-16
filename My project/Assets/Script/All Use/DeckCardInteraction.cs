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


    public void LeftClick(CardInteraction card)
    {
        deckManager.MoveCard(
            card.DeckIndex,
            isPlayer
        );
    }


    public void RightClick(CardData card)
    {

    }


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