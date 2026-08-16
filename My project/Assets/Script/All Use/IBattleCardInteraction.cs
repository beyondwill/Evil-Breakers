public interface IBattleCardInteraction
{
    void PointerEnter(CardInteraction card);
    void PointerExit(CardInteraction card);

    void LeftClick(CardInteraction card);
    void RightClick(CardInteraction card);

    void BeginDrag(CardInteraction card);
    void Drag(CardInteraction card);
    void EndDrag(CardInteraction card);
}