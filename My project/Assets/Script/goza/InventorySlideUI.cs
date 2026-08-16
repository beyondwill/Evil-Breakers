using UnityEngine;
using DG.Tweening;

public class InventorySlideUI : MonoBehaviour
{
    public RectTransform inventoryPanel;
    public CanvasGroup canvasGroup;

    public float openX = 0;
    public float closeX = 500;

    public float duration = 0.3f;

    bool isOpen = false;


    void Start()
    {
        // 시작 상태
        inventoryPanel.anchoredPosition =
            new Vector2(closeX, inventoryPanel.anchoredPosition.y);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }


    public void ToggleInventory()
    {
        isOpen = !isOpen;


        float targetX = isOpen ? openX : closeX;
        float targetAlpha = isOpen ? 1 : 0;


        // 기존 애니메이션 제거
        inventoryPanel.DOKill();
        canvasGroup.DOKill();


        // 이동
        inventoryPanel.DOAnchorPosX(
            targetX,
            duration
        )
        .SetEase(Ease.OutCubic);


        // 투명도
        canvasGroup.DOFade(
            targetAlpha,
            duration
        );


        // 클릭 가능 여부
        if (isOpen)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            DOVirtual.DelayedCall(duration, () =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
        }
    }
}