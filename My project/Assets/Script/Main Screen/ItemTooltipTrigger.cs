using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private ItemData item;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 아이템 정보 설정
    public void SetItem(ItemData item)
    {
        this.item = item;
    }

    // 마우스를 아이콘 위에 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null)
            return;

        if (TooltipUI.Instance == null)
            return;

        TooltipUI.Instance.Show(
            item,
            rectTransform
        );
    }

    // 마우스가 아이콘에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance == null)
            return;

        TooltipUI.Instance.Hide();
    }
}