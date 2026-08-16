using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("Rect")]
    [SerializeField] private RectTransform rect;

    [Header("UI")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Stat")]
    [SerializeField] private StatConfig statConfig;

    [Header("Setting")]
    [SerializeField] private Vector2 offset = new Vector2(10f, 0f);
    [SerializeField] private bool showPrice = false;

    private Canvas canvas;


    private void Awake()
    {
        Instance = this;

        canvas = GetComponentInParent<Canvas>();


        // =====================================================
        // Tooltip이 마우스 이벤트를 막지 않도록 설정
        // =====================================================

        CanvasGroup canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;


        // 자식 UI도 Raycast 차단 방지

        if (background != null)
            background.raycastTarget = false;

        if (nameText != null)
            nameText.raycastTarget = false;

        if (typeText != null)
            typeText.raycastTarget = false;

        if (descriptionText != null)
            descriptionText.raycastTarget = false;

        if (priceText != null)
            priceText.raycastTarget = false;


        gameObject.SetActive(false);
    }


    // =========================================================
    // SHOW
    // =========================================================

    public void Show(
        ItemData item,
        RectTransform target)
    {
        if (item == null || target == null)
            return;

        if (rect == null)
            return;

        if (canvas == null)
            return;


        // =====================================================
        // 이름
        // =====================================================

        if (nameText != null)
        {
            nameText.text =
                item.itemName;
        }


        // =====================================================
        // 타입
        // =====================================================

        if (typeText != null)
        {
            typeText.text =
                $"분류 : {GetTypeName(item.itemType)}";
        }


        // =====================================================
        // 설명
        // =====================================================

        if (descriptionText != null)
        {
            descriptionText.text =
                GetDescription(item);
        }


        // =====================================================
        // 가격
        // =====================================================

        if (priceText != null)
        {
            priceText.gameObject.SetActive(
                showPrice);

            if (showPrice)
            {
                priceText.text =
                    item.sellPrice.ToString();
            }
        }


        // =====================================================
        // 활성화
        // =====================================================

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }


        // =====================================================
        // 레이아웃 갱신
        // =====================================================

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rect);

        Canvas.ForceUpdateCanvases();


        // =====================================================
        // 위치
        // =====================================================

        SetPosition(target);
    }


    // =========================================================
    // 설명 + 장비 스탯
    // =========================================================

    private string GetDescription(ItemData item)
    {
        if (item == null)
            return "";


        StringBuilder builder =
            new StringBuilder();


        // -----------------------------------------------------
        // 기본 설명
        // -----------------------------------------------------

        if (!string.IsNullOrEmpty(item.description))
        {
            builder.Append(
                item.description);
        }


        // -----------------------------------------------------
        // 장비인 경우 스탯 추가
        // -----------------------------------------------------

        EquipmentInfo equipment =
            item as EquipmentInfo;


        if (equipment == null)
            return builder.ToString();


        if (equipment.baseStatList == null)
            return builder.ToString();


        foreach (CharacterBaseStatValue stat
                 in equipment.baseStatList)
        {
            if (stat == null)
                continue;


            // 값이 0이면 표시하지 않음
            if (stat.value == 0f)
                continue;


            string statName =
                GetStatName(stat.type);


            // 설명과 스탯 사이 줄바꿈
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }


            // =================================================
            // 최종 출력
            //
            // 공격력: 10
            // 방어력: 5
            // =================================================

            builder.Append(
                statName);

            builder.Append(
                ": ");

            builder.Append(
                FormatStatValue(stat.value));
        }


        return builder.ToString();
    }


    // =========================================================
    // 스탯 이름 가져오기
    // =========================================================

    private string GetStatName(
        CharacterBaseStatType type)
    {
        // StatConfig가 없으면 enum 이름 사용
        if (statConfig == null)
        {
            return type.ToString();
        }


        CharacterBaseStatSort stat =
            statConfig.FindBaseStat(type);


        // 해당 스탯 설정이 없으면 enum 이름 사용
        if (stat == null)
        {
            return type.ToString();
        }


        return stat.statName;
    }


    // =========================================================
    // 스탯 값 표시
    // =========================================================

    private string FormatStatValue(float value)
    {
        // 10.0 → 10
        // 10.5 → 10.5

        if (Mathf.Approximately(
            value,
            Mathf.Round(value)))
        {
            return Mathf.RoundToInt(value)
                .ToString();
        }


        return value.ToString("0.##");
    }


    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        if (!gameObject.activeSelf)
            return;


        gameObject.SetActive(false);
    }


    // =========================================================
    // POSITION
    // =========================================================

    private void SetPosition(
        RectTransform target)
    {
        if (target == null)
            return;

        if (rect == null)
            return;

        if (canvas == null)
            return;


        // =====================================================
        // 1. 아이콘의 오른쪽 중앙
        // =====================================================

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(corners);


        /*
        
        3 ---------------- 2
        |                  |
        |       ICON       |
        |                  |
        0 ---------------- 1
        
        */


        Vector3 rightCenter =
            (corners[1] + corners[2]) * 0.5f;


        // =====================================================
        // 2. World → Screen
        // =====================================================

        Camera camera =
            GetCanvasCamera();


        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                camera,
                rightCenter
            );


        // =====================================================
        // 3. Screen → Tooltip 부모 World
        // =====================================================

        RectTransform parentRect =
            rect.parent as RectTransform;


        if (parentRect == null)
            return;


        Vector3 worldPosition;


        RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                parentRect,
                screenPosition,
                camera,
                out worldPosition
            );


        // =====================================================
        // 4. Tooltip 위치
        // =====================================================

        rect.position =
            worldPosition;


        // =====================================================
        // 5. Offset
        // =====================================================

        Vector3 offsetWorld =
            GetOffsetWorld(offset);


        rect.position +=
            offsetWorld;


        // =====================================================
        // 6. 화면 밖 보정
        // =====================================================

        ClampToScreen(target);
    }


    // =========================================================
    // Offset 변환
    // =========================================================

    private Vector3 GetOffsetWorld(
        Vector2 offset)
    {
        RectTransform parentRect =
            rect.parent as RectTransform;


        if (parentRect == null)
            return Vector3.zero;


        Vector3 right =
            parentRect.TransformVector(
                new Vector3(
                    offset.x,
                    0f,
                    0f)
            );


        Vector3 up =
            parentRect.TransformVector(
                new Vector3(
                    0f,
                    offset.y,
                    0f)
            );


        return right + up;
    }


    // =========================================================
    // 화면 밖 보정
    // =========================================================

    private void ClampToScreen(
        RectTransform target)
    {
        if (rect == null)
            return;


        Canvas.ForceUpdateCanvases();


        Camera camera =
            GetCanvasCamera();


        Vector3[] corners =
            new Vector3[4];


        rect.GetWorldCorners(corners);


        Vector2 bottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                camera,
                corners[0]
            );


        Vector2 topRight =
            RectTransformUtility.WorldToScreenPoint(
                camera,
                corners[2]
            );


        float left =
            bottomLeft.x;

        float right =
            topRight.x;

        float bottom =
            bottomLeft.y;

        float top =
            topRight.y;


        float screenWidth =
            Screen.width;

        float screenHeight =
            Screen.height;


        Vector3 position =
            rect.position;


        // =====================================================
        // 오른쪽 화면 밖
        // =====================================================

        if (right > screenWidth)
        {
            Vector3[] targetCorners =
                new Vector3[4];


            target.GetWorldCorners(
                targetCorners);


            // 아이콘 왼쪽 중앙
            Vector3 leftCenter =
                (targetCorners[0] +
                 targetCorners[3]) * 0.5f;


            Vector2 targetScreen =
                RectTransformUtility
                    .WorldToScreenPoint(
                        camera,
                        leftCenter
                    );


            RectTransform parentRect =
                rect.parent as RectTransform;


            if (parentRect != null)
            {
                Vector3 worldPosition;


                RectTransformUtility
                    .ScreenPointToWorldPointInRectangle(
                        parentRect,
                        targetScreen,
                        camera,
                        out worldPosition
                    );


                rect.position =
                    worldPosition -
                    GetOffsetWorld(offset);
            }


            // 다시 갱신
            Canvas.ForceUpdateCanvases();


            rect.GetWorldCorners(
                corners);


            bottomLeft =
                RectTransformUtility
                    .WorldToScreenPoint(
                        camera,
                        corners[0]
                    );


            topRight =
                RectTransformUtility
                    .WorldToScreenPoint(
                        camera,
                        corners[2]
                    );


            left =
                bottomLeft.x;

            right =
                topRight.x;

            bottom =
                bottomLeft.y;

            top =
                topRight.y;
        }


        // =====================================================
        // 왼쪽 화면 밖
        // =====================================================

        if (left < 0f)
        {
            float difference =
                -left;


            position +=
                GetOffsetWorld(
                    new Vector2(
                        difference,
                        0f)
                );
        }


        // =====================================================
        // 오른쪽 화면 밖
        // =====================================================

        if (right > screenWidth)
        {
            float difference =
                right - screenWidth;


            position -=
                GetOffsetWorld(
                    new Vector2(
                        difference,
                        0f)
                );
        }


        // =====================================================
        // 아래 화면 밖
        // =====================================================

        if (bottom < 0f)
        {
            float difference =
                -bottom;


            position +=
                GetOffsetWorld(
                    new Vector2(
                        0f,
                        difference)
                );
        }


        // =====================================================
        // 위 화면 밖
        // =====================================================

        if (top > screenHeight)
        {
            float difference =
                top - screenHeight;


            position -=
                GetOffsetWorld(
                    new Vector2(
                        0f,
                        difference)
                );
        }


        rect.position =
            position;
    }


    // =========================================================
    // Canvas Camera
    // =========================================================

    private Camera GetCanvasCamera()
    {
        if (canvas == null)
            return null;


        if (canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }


        return canvas.worldCamera;
    }


    // =========================================================
    // ItemType → 한글
    // =========================================================

    private string GetTypeName(
        ItemType type)
    {
        switch (type)
        {
            case ItemType.Normal:
                return "일반";

            case ItemType.Equipment:
                return "장비";

            case ItemType.Relic:
                return "유물";

            case ItemType.Consumable:
                return "소모품";

            case ItemType.Quest:
                return "퀘스트";

            default:
                return "";
        }
    }
}