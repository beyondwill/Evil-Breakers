using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlotDragManager :
    MonoBehaviour
{
    [Header("Drag")]
    [SerializeField] private float dragAlpha = 0.7f;


    // =========================================================
    // 변수
    // =========================================================

    private Canvas canvas;

    private RectTransform canvasRect;


    private MainCharacterSlot draggingSlot;


    private RectTransform dragVisual;

    private CanvasGroup dragVisualCanvasGroup;


    private GameObject placeholder;

    private RectTransform placeholderRect;

    private LayoutElement placeholderLayout;


    private LayoutElement draggingLayoutElement;


    private int originalIndex;

    private bool isDragging;


    // =========================================================
    // 드래그 시작 위치
    // =========================================================

    private Vector3 dragStartWorldPosition;


    // 마우스를 잡은 위치와 캐릭터 위치 사이의 거리
    private Vector3 dragOffset;


    // =========================================================
    // Property
    // =========================================================

    public bool IsDragging
    {
        get
        {
            return isDragging;
        }
    }


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        canvas =
            GetComponentInParent<Canvas>();


        if (canvas != null)
        {
            canvasRect =
                canvas.GetComponent<RectTransform>();
        }
    }


    // =========================================================
    // 드래그 시작
    // =========================================================

    public void BeginDrag(
        MainCharacterSlot slot,
        PointerEventData eventData)
    {
        if (isDragging)
            return;


        if (slot == null)
            return;


        if (canvas == null)
            return;


        RectTransform slotRect =
            slot.GetRectTransform();


        if (slotRect == null)
            return;


        // =====================================================
        // 드래그 상태 시작
        // =====================================================

        isDragging = true;

        draggingSlot = slot;


        // =====================================================
        // ★ 매우 중요
        //
        // Placeholder가 생성되기 전에
        // 원본 슬롯의 실제 위치를 저장한다.
        // =====================================================

        dragStartWorldPosition =
            slotRect.position;


        // =====================================================
        // 원래 순서
        // =====================================================

        originalIndex =
            slot.transform.GetSiblingIndex();


        // =====================================================
        // ★ 마우스를 잡은 위치와 슬롯 중심의 차이 저장
        //
        // 이 값을 저장해두면
        // 마우스를 캐릭터의 어느 위치에서 잡든
        // 그 지점을 그대로 유지하면서 움직인다.
        // =====================================================

        Vector3 mouseWorldPosition;

        bool mousePositionSuccess =
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out mouseWorldPosition
                );


        if (mousePositionSuccess)
        {
            dragOffset =
                dragStartWorldPosition
                - mouseWorldPosition;
        }
        else
        {
            dragOffset =
                Vector3.zero;
        }


        // =====================================================
        // Placeholder 생성
        // =====================================================

        CreatePlaceholder(
            slot
        );


        // =====================================================
        // 실제 슬롯 Layout 제외
        // =====================================================

        draggingLayoutElement =
            slot.GetComponent<LayoutElement>();


        if (draggingLayoutElement == null)
        {
            draggingLayoutElement =
                slot.gameObject.AddComponent<
                    LayoutElement
                >();
        }


        draggingLayoutElement.ignoreLayout =
            true;


        // =====================================================
        // 원본 슬롯 숨기기
        // =====================================================

        slot.SetDragging(true);


        // =====================================================
        // Layout 갱신
        //
        // 여기서 다른 캐릭터들이 움직여도
        // dragStartWorldPosition은 이미 저장되어 있음.
        // =====================================================

        Canvas.ForceUpdateCanvases();


        RectTransform parentRect =
            transform as RectTransform;


        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parentRect
            );
        }


        Canvas.ForceUpdateCanvases();


        // =====================================================
        // Drag Visual 생성
        // =====================================================

        CreateDragVisual(
            slot
        );


        // =====================================================
        // ★ Drag Visual을
        // Placeholder 때문에 바뀌기 전의
        // 원래 위치로 강제 이동
        // =====================================================

        if (dragVisual != null)
        {
            dragVisual.position =
                dragStartWorldPosition;
        }


        // =====================================================
        // Raycast
        // =====================================================

        if (dragVisualCanvasGroup != null)
        {
            dragVisualCanvasGroup.blocksRaycasts =
                false;

            dragVisualCanvasGroup.interactable =
                false;

            dragVisualCanvasGroup.alpha =
                dragAlpha;
        }


        // =====================================================
        // 상태 초기화 후 현재 마우스 위치 적용
        // =====================================================

        UpdateDragVisualPosition(
            eventData
        );
    }


    // =========================================================
    // Placeholder 생성
    // =========================================================

    private void CreatePlaceholder(
    MainCharacterSlot slot)
    {
        // =====================================================
        // Placeholder 생성
        // =====================================================

        placeholder =
            new GameObject(
                "CharacterSlot_Placeholder",
                typeof(RectTransform),
                typeof(LayoutElement)
            );


        placeholderRect =
            placeholder.GetComponent<RectTransform>();


        placeholderLayout =
            placeholder.GetComponent<LayoutElement>();


        // =====================================================
        // 부모
        // =====================================================

        placeholder.transform.SetParent(
            transform,
            false
        );


        // =====================================================
        // 원래 위치
        // =====================================================

        placeholder.transform.SetSiblingIndex(
            originalIndex
        );


        // =====================================================
        // ★ Placeholder 크기
        // =====================================================

        const float PLACEHOLDER_WIDTH = 300f;
        const float PLACEHOLDER_HEIGHT = 300f;


        // =====================================================
        // LayoutElement
        // =====================================================

        placeholderLayout.ignoreLayout =
            false;


        placeholderLayout.minWidth =
            PLACEHOLDER_WIDTH;

        placeholderLayout.preferredWidth =
            PLACEHOLDER_WIDTH;

        placeholderLayout.flexibleWidth =
            0f;


        placeholderLayout.minHeight =
            PLACEHOLDER_HEIGHT;

        placeholderLayout.preferredHeight =
            PLACEHOLDER_HEIGHT;

        placeholderLayout.flexibleHeight =
            0f;


        // =====================================================
        // ★ RectTransform도 직접 300으로 설정
        // =====================================================

        placeholderRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            PLACEHOLDER_WIDTH
        );


        placeholderRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            PLACEHOLDER_HEIGHT
        );


        // =====================================================
        // Anchor
        // =====================================================

        placeholderRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );


        placeholderRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );


        placeholderRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        placeholderRect.localScale =
            Vector3.one;


        // =====================================================
        // Layout 갱신
        // =====================================================

        Canvas.ForceUpdateCanvases();


        RectTransform parentRect =
            transform as RectTransform;


        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parentRect
            );
        }


        Canvas.ForceUpdateCanvases();


        // =====================================================
        // ★ 마지막으로 실제 Rect 크기도 다시 확인
        // =====================================================

        placeholderRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            PLACEHOLDER_WIDTH
        );


        placeholderRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            PLACEHOLDER_HEIGHT
        );
    }


    // =========================================================
    // Drag Visual 생성
    // =========================================================

    private void CreateDragVisual(
        MainCharacterSlot slot)
    {
        // =====================================================
        // 복제
        // =====================================================

        GameObject clone =
            Instantiate(
                slot.gameObject,
                canvas.transform
            );


        clone.name =
            slot.gameObject.name
            + "_DragVisual";


        // =====================================================
        // 복제된 MainCharacterSlot 비활성화
        // =====================================================

        MainCharacterSlot cloneSlot =
            clone.GetComponent<
                MainCharacterSlot
            >();


        if (cloneSlot != null)
        {
            cloneSlot.enabled =
                false;
        }


        // =====================================================
        // RectTransform
        // =====================================================

        dragVisual =
            clone.GetComponent<
                RectTransform
            >();


        RectTransform originalRect =
            slot.GetRectTransform();


        // =====================================================
        // 위치
        //
        // ★ 여기서 originalRect.position을 사용하면 안 됨.
        //
        // Placeholder 생성으로 Layout이 이미 움직였기 때문.
        //
        // 따라서 BeginDrag에서 저장해둔
        // dragStartWorldPosition을 사용한다.
        // =====================================================

        dragVisual.position =
            dragStartWorldPosition;


        dragVisual.rotation =
            originalRect.rotation;


        // =====================================================
        // 실제 월드 크기 유지
        // =====================================================

        Vector3 canvasScale =
            canvasRect.lossyScale;


        Vector3 originalScale =
            originalRect.lossyScale;


        dragVisual.localScale =
            new Vector3(
                SafeDivide(
                    originalScale.x,
                    canvasScale.x
                ),

                SafeDivide(
                    originalScale.y,
                    canvasScale.y
                ),

                SafeDivide(
                    originalScale.z,
                    canvasScale.z
                )
            );


        // =====================================================
        // CanvasGroup
        // =====================================================

        dragVisualCanvasGroup =
            clone.GetComponent<
                CanvasGroup
            >();


        if (dragVisualCanvasGroup == null)
        {
            dragVisualCanvasGroup =
                clone.AddComponent<
                    CanvasGroup
                >();
        }


        dragVisualCanvasGroup.alpha =
            dragAlpha;


        dragVisualCanvasGroup.blocksRaycasts =
            false;


        dragVisualCanvasGroup.interactable =
            false;
    }


    // =========================================================
    // 안전한 나눗셈
    // =========================================================

    private float SafeDivide(
        float a,
        float b)
    {
        if (Mathf.Approximately(
            b,
            0f))
        {
            return 1f;
        }


        return a / b;
    }


    // =========================================================
    // 드래그
    // =========================================================

    public void Drag(
        PointerEventData eventData)
    {
        if (!isDragging)
            return;


        if (draggingSlot == null)
            return;


        // =====================================================
        // Drag Visual 이동
        // =====================================================

        UpdateDragVisualPosition(
            eventData
        );


        // =====================================================
        // Placeholder 위치 계산
        // =====================================================

        UpdatePlaceholderPosition(
            eventData.position
        );
    }


    // =========================================================
    // Drag Visual 위치
    // =========================================================

    private void UpdateDragVisualPosition(
        PointerEventData eventData)
    {
        if (dragVisual == null)
            return;


        if (canvasRect == null)
            return;


        Vector3 worldPosition;


        bool success =
            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out worldPosition
                );


        if (!success)
            return;


        // =====================================================
        // ★ 마우스 위치 + 잡았던 위치 보정
        // =====================================================

        dragVisual.position =
            worldPosition
            + dragOffset;
    }


    // =========================================================
    // Placeholder 위치 계산
    // =========================================================

    private void UpdatePlaceholderPosition(
        Vector2 screenPosition)
    {
        if (placeholder == null)
            return;


        // =====================================================
        // ★ 현재 Placeholder 영역 안에 마우스가 있으면
        // ★ 절대로 Placeholder를 이동시키지 않는다.
        //
        // 이게 핵심
        // =====================================================

        if (IsMouseOverPlaceholder(screenPosition))
        {
            return;
        }


        MainCharacterSlot[] slots =
            GetComponentsInChildren<
                MainCharacterSlot
            >();


        int currentPlaceholderIndex =
            placeholder.transform.GetSiblingIndex();


        int targetIndex =
            currentPlaceholderIndex;


        // =====================================================
        // 슬롯 검사
        // =====================================================

        foreach (
            MainCharacterSlot slot
            in slots
        )
        {
            if (slot == null)
                continue;


            if (slot == draggingSlot)
                continue;


            RectTransform slotRect =
                slot.GetRectTransform();


            if (slotRect == null)
                continue;


            Vector3[] corners =
                new Vector3[4];


            slotRect.GetWorldCorners(
                corners
            );


            Vector2 bottom =
                RectTransformUtility.WorldToScreenPoint(
                    canvas.worldCamera,
                    corners[0]
                );


            Vector2 top =
                RectTransformUtility.WorldToScreenPoint(
                    canvas.worldCamera,
                    corners[1]
                );


            float centerY =
                (bottom.y + top.y) * 0.5f;


            int siblingIndex =
                slot.transform.GetSiblingIndex();


            // =================================================
            // 슬롯 위쪽
            // =================================================

            if (screenPosition.y > centerY)
            {
                targetIndex =
                    siblingIndex;

                break;
            }


            // =================================================
            // 슬롯 아래쪽
            // =================================================

            targetIndex =
                siblingIndex + 1;
        }


        // =====================================================
        // 범위 보정
        // =====================================================

        int maxIndex =
            transform.childCount - 1;


        targetIndex =
            Mathf.Clamp(
                targetIndex,
                0,
                maxIndex
            );


        // =====================================================
        // ★ 현재 위치와 같으면 아무것도 하지 않음
        // =====================================================

        if (targetIndex ==
            currentPlaceholderIndex)
        {
            return;
        }


        // =====================================================
        // Placeholder 이동
        // =====================================================

        placeholder.transform.SetSiblingIndex(
            targetIndex
        );


        // =====================================================
        // Layout 갱신 예약
        // =====================================================

        RectTransform parentRect =
            transform as RectTransform;


        if (parentRect != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(
                parentRect
            );
        }
    }


    // =========================================================
    // 드래그 종료
    // =========================================================

    public void EndDrag(
        PointerEventData eventData)
    {
        if (!isDragging)
            return;


        // =====================================================
        // 최종 위치
        // =====================================================

        int targetIndex =
            originalIndex;


        if (placeholder != null)
        {
            targetIndex =
                placeholder.transform.GetSiblingIndex();
        }


        // =====================================================
        // 실제 슬롯 Layout 복구
        // =====================================================

        if (draggingLayoutElement != null)
        {
            draggingLayoutElement.ignoreLayout =
                false;
        }


        // =====================================================
        // 실제 슬롯 위치 변경
        // =====================================================

        if (draggingSlot != null)
        {
            draggingSlot.transform.SetSiblingIndex(
                targetIndex
            );


            draggingSlot.SetDragging(
                false
            );
        }


        // =====================================================
        // Placeholder 삭제
        // =====================================================

        if (placeholder != null)
        {
            Destroy(
                placeholder
            );


            placeholder =
                null;


            placeholderRect =
                null;


            placeholderLayout =
                null;
        }


        // =====================================================
        // Drag Visual 삭제
        // =====================================================

        if (dragVisual != null)
        {
            Destroy(
                dragVisual.gameObject
            );


            dragVisual =
                null;


            dragVisualCanvasGroup =
                null;
        }


        // =====================================================
        // Layout 강제 갱신
        // =====================================================

        Canvas.ForceUpdateCanvases();


        RectTransform parentRect =
            transform as RectTransform;


        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parentRect
            );
        }


        Canvas.ForceUpdateCanvases();


        // =====================================================
        // 상태 초기화
        // =====================================================

        draggingSlot =
            null;


        draggingLayoutElement =
            null;


        dragOffset =
            Vector3.zero;


        dragStartWorldPosition =
            Vector3.zero;


        originalIndex =
            -1;


        isDragging =
            false;
    }

    // =========================================================
    // 마우스가 Placeholder 위에 있는지 확인
    // =========================================================

    private bool IsMouseOverPlaceholder(
        Vector2 screenPosition)
    {
        if (placeholderRect == null)
            return false;


        Vector3[] corners =
            new Vector3[4];


        placeholderRect.GetWorldCorners(
            corners
        );


        Vector2 bottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                corners[0]
            );


        Vector2 topRight =
            RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                corners[2]
            );


        Rect rect =
            Rect.MinMaxRect(
                bottomLeft.x,
                bottomLeft.y,
                topRight.x,
                topRight.y
            );


        return rect.Contains(
            screenPosition
        );
    }
}