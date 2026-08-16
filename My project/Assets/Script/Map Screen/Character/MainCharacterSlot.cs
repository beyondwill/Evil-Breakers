using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainCharacterSlot :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("외부 요소")]
    [SerializeField] private IconButton character_icon;
    [SerializeField] private Transform stressHLG;
    [SerializeField] private TalkingBoxUI talkingBoxUI;
    [SerializeField] private Slider slider;


    [Header("Text")]
    [SerializeField] private TextMeshProUGUI character_name;
    [SerializeField] private TextMeshProUGUI character_attack;
    [SerializeField] private TextMeshProUGUI character_defend;
    [SerializeField] private TextMeshProUGUI character_level;

    [SerializeField] private TextMeshProUGUI level_additional;
    [SerializeField] private TextMeshProUGUI exp_additional;
    [SerializeField] private TextMeshProUGUI stress_additional;


    [Header("Prefab")]
    [SerializeField] private StressUI stressUI;


    [Header("Slide")]
    [SerializeField] private float slideDistance = 20f;
    [SerializeField] private float slideDuration = 0.15f;


    // =========================================================
    // 변수
    // =========================================================

    private RectTransform rectTransform;

    private CanvasGroup canvasGroup;

    private CharacterSlotDragManager dragManager;


    // =========================================================
    // ★ Slide 기준 위치
    // =========================================================

    // Layout Group이 잡아준 원래 X 위치
    private float slideOriginalX;

    // 현재 슬라이드 상태
    private bool isSlid;


    // =========================================================
    // 캐릭터 데이터
    // =========================================================

    public PlayerCharacterData CharacterData
    {
        get;
        private set;
    }


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();


        canvasGroup =
            GetComponent<CanvasGroup>();


        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }


        dragManager =
            GetComponentInParent<
                CharacterSlotDragManager
            >();
    }


    // =========================================================
    // Start
    // =========================================================

    private IEnumerator Start()
    {
        yield return null;


        if (dragManager == null)
        {
            dragManager =
                GetComponentInParent<
                    CharacterSlotDragManager
                >();
        }


        // =====================================================
        // ★ 시작할 때 Layout이 정해준 위치 저장
        // =====================================================

        slideOriginalX =
            rectTransform.anchoredPosition.x;

        isSlid = false;
    }


    // =========================================================
    // 마우스 진입
    // =========================================================

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        // -----------------------------------------------------
        // 드래그 중에는 슬라이드하지 않음
        // -----------------------------------------------------

        if (
            dragManager != null &&
            dragManager.IsDragging
        )
        {
            return;
        }


        rectTransform.DOKill();


        // =====================================================
        // ★ 아직 슬라이드 상태가 아니라면
        // 현재 Layout 위치를 기준 위치로 저장
        // =====================================================

        if (!isSlid)
        {
            slideOriginalX =
                rectTransform.anchoredPosition.x;

            isSlid = true;
        }


        // =====================================================
        // ★ 원래 위치에서 slideDistance만큼만 왼쪽 이동
        // =====================================================

        rectTransform
            .DOAnchorPosX(
                slideOriginalX - slideDistance,
                slideDuration
            )
            .SetEase(
                Ease.OutQuad
            );
    }


    // =========================================================
    // 마우스 이탈
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData)
    {
        // -----------------------------------------------------
        // 드래그 중에는 슬라이드 복구하지 않음
        // -----------------------------------------------------

        if (
            dragManager != null &&
            dragManager.IsDragging
        )
        {
            return;
        }


        rectTransform.DOKill();


        // =====================================================
        // ★ 중요
        //
        // 0으로 보내면 안 됨.
        //
        // Layout Group이 잡아준 원래 X 위치로 돌아감.
        // =====================================================

        rectTransform
            .DOAnchorPosX(
                slideOriginalX,
                slideDuration
            )
            .SetEase(
                Ease.OutQuad
            )
            .OnComplete(() =>
            {
                isSlid = false;
            });
    }


    // =========================================================
    // 드래그 시작
    // =========================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (CharacterData == null)
            return;


        if (dragManager == null)
        {
            dragManager =
                GetComponentInParent<
                    CharacterSlotDragManager
                >();
        }


        if (dragManager == null)
            return;


        // =====================================================
        // ★ 드래그 시작 전에 슬라이드 Tween 제거
        // =====================================================

        rectTransform.DOKill();


        // =====================================================
        // ★ 현재 위치를 Layout 기준으로 다시 저장하지 않음
        //
        // 드래그에서는 DragManager가 위치를 관리함.
        // =====================================================

        isSlid = false;


        dragManager.BeginDrag(
            this,
            eventData
        );
    }


    // =========================================================
    // 드래그
    // =========================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (dragManager == null)
            return;


        dragManager.Drag(
            eventData
        );
    }


    // =========================================================
    // 드래그 종료
    // =========================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (dragManager == null)
            return;


        dragManager.EndDrag(
            eventData
        );


        // =====================================================
        // ★ Layout이 새로운 위치를 잡은 뒤
        // 그 위치를 새로운 슬라이드 기준으로 사용
        // =====================================================

        StartCoroutine(
            RefreshSlidePosition()
        );
    }


    // =========================================================
    // 드래그 종료 후 Slide 위치 갱신
    // =========================================================

    private IEnumerator RefreshSlidePosition()
    {
        // Layout Group이 위치를 계산할 시간을 줌
        yield return null;


        Canvas.ForceUpdateCanvases();


        slideOriginalX =
            rectTransform.anchoredPosition.x;


        isSlid = false;
    }


    // =========================================================
    // 드래그 중 원본 숨기기
    // =========================================================

    public void SetDragging(
        bool value)
    {
        if (canvasGroup == null)
            return;


        if (value)
        {
            canvasGroup.alpha =
                0f;

            canvasGroup.blocksRaycasts =
                false;

            canvasGroup.interactable =
                false;
        }
        else
        {
            canvasGroup.alpha =
                1f;

            canvasGroup.blocksRaycasts =
                true;

            canvasGroup.interactable =
                true;
        }
    }


    // =========================================================
    // RectTransform
    // =========================================================

    public RectTransform GetRectTransform()
    {
        return rectTransform;
    }


    // =========================================================
    // 캐릭터 슬롯 보여주기
    // =========================================================

    public void ShowCharacterSlot(
        PlayerCharacterData PCD)
    {
        CharacterData =
            PCD;


        Debug.Log("생성");


        // -----------------------------------------------------
        // 기존 스트레스 UI 삭제
        // -----------------------------------------------------

        for (
            int i = stressHLG.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                stressHLG
                    .GetChild(i)
                    .gameObject
            );
        }


        // -----------------------------------------------------
        // 스트레스
        // -----------------------------------------------------

        int fill_stress =
            Mathf.Clamp(
                PCD.current_stress / 10,
                0,
                10
            );


        // -----------------------------------------------------
        // 채워진 스트레스
        // -----------------------------------------------------

        for (
            int i = 0;
            i < fill_stress;
            i++
        )
        {
            StressUI SU =
                Instantiate(
                    stressUI,
                    stressHLG
                );


            SU.StressInit(true);
        }


        // -----------------------------------------------------
        // 비어있는 스트레스
        // -----------------------------------------------------

        for (
            int i = fill_stress;
            i < 10;
            i++
        )
        {
            StressUI SU =
                Instantiate(
                    stressUI,
                    stressHLG
                );


            SU.StressInit(false);
        }


        // -----------------------------------------------------
        // 캐릭터 정보
        // -----------------------------------------------------

        character_level.text =
            PCD.player_character_level.ToString();


        character_attack.text =
            PCD.current_weapon_level.ToString();


        character_defend.text =
            PCD.current_armor_level.ToString();


        character_name.text =
            PCD.player_character_info.character_name;


        character_icon.SetImage(
            PCD.player_character_info.character_icon
        );


        // -----------------------------------------------------
        // 대화
        // -----------------------------------------------------

        talkingBoxUI.Init(
            PCD.player_character_info
        );


        // -----------------------------------------------------
        // 경험치
        // -----------------------------------------------------

        int current_level =
            PCD.player_character_level;


        List<int> expList =
            GameRuleManager.Instance
                .Rule
                .expPerLevelList;


        // -----------------------------------------------------
        // 최대 레벨
        // -----------------------------------------------------

        if (
            current_level >=
            expList.Count - 1
        )
        {
            slider.minValue =
                0;

            slider.maxValue =
                1;

            slider.value =
                1;


            level_additional.text =
                GameRuleManager.Instance
                    .Rule
                    .NamePerLevelList[
                        current_level
                    ]
                + " (레벨: "
                + current_level
                + ")";


            exp_additional.text =
                "의지 경험치: MAX";


            stress_additional.text =
                "스트레스: "
                + PCD.current_stress
                + "/200";


            return;
        }


        // -----------------------------------------------------
        // 현재 레벨 시작 경험치
        // -----------------------------------------------------

        int current_level_exp =
            expList[current_level];


        // -----------------------------------------------------
        // 다음 레벨 시작 경험치
        // -----------------------------------------------------

        int next_level_exp =
            expList[current_level + 1];


        // -----------------------------------------------------
        // 현재 레벨에서 쌓인 경험치
        // -----------------------------------------------------

        int current_exp =
            PCD.current_exp
            - current_level_exp;


        // -----------------------------------------------------
        // 현재 레벨에서 필요한 경험치
        // -----------------------------------------------------

        int required_exp =
            next_level_exp
            - current_level_exp;


        // -----------------------------------------------------
        // Slider
        // -----------------------------------------------------

        slider.minValue =
            0;


        slider.maxValue =
            required_exp;


        slider.value =
            Mathf.Clamp(
                current_exp,
                0,
                required_exp
            );


        // -----------------------------------------------------
        // Additional
        // -----------------------------------------------------

        level_additional.text =
            GameRuleManager.Instance
                .Rule
                .NamePerLevelList[
                    current_level
                ]
            + " (레벨: "
            + current_level
            + ")";


        exp_additional.text =
            "의지 경험치: "
            + PCD.current_exp
            + "/"
            + GameRuleManager.Instance
                .Rule
                .expPerLevelList[
                    current_level + 1
                ];


        stress_additional.text =
            "스트레스: "
            + PCD.current_stress
            + "/200";
    }
}