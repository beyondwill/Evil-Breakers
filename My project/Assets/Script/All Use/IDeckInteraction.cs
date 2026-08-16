public interface IDeckCardInteraction
{
    void LeftClick(CardInteraction card);

    void RightClick(CardData card);

    void BeginDrag(CardData card);

    void Drag(CardData card);

    void EndDrag(CardData card);
}