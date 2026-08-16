using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDrag :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private int originalSiblingIndex;


    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvas =
            GetComponentInParent<Canvas>();

        canvasGroup =
            GetComponent<CanvasGroup>();


        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }
    }


    // =====================================================
    // 드래그 시작
    // =====================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        originalParent =
            transform.parent;

        originalSiblingIndex =
            transform.GetSiblingIndex();


        // Vertical Layout Group에서 잠시 꺼냄
        transform.SetParent(
            canvas.transform);


        // 다른 슬롯이 Raycast를 받을 수 있게
        canvasGroup.blocksRaycasts =
            false;

        canvasGroup.alpha =
            0.7f;
    }


    // =====================================================
    // 드래그 중
    // =====================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        rectTransform.position =
            eventData.position;
    }


    // =====================================================
    // 드래그 종료
    // =====================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        // 일단 원래 부모로 복귀
        transform.SetParent(
            originalParent);

        transform.SetSiblingIndex(
            originalSiblingIndex);


        canvasGroup.blocksRaycasts =
            true;

        canvasGroup.alpha =
            1f;
    }
}