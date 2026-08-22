using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ConversationBoxUI : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image characterImage;


    [Header("Text")]
    [SerializeField] private TextMeshProUGUI characterConversationText;


    [Header("Arrow")]
    [SerializeField] private GameObject nextArrow;

    [SerializeField] private float arrowDelay = 0.5f;


    [Header("변수")]
    [SerializeField] private float textSpeed = 0.1f;


    [Header("등장")]
    [SerializeField] private float showDuration = 0.3f;


    [Header("퇴장")]
    [SerializeField] private float hideDuration = 0.3f;

    [SerializeField] private float hideDistance = 50f;


    // =========================================================
    // UI
    // =========================================================

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;


    // =========================================================
    // Tween
    // =========================================================

    // 등장 Fade
    private Tween showTween;

    // 텍스트 출력
    private Tween textTween;

    // 화살표
    private Tween arrowTween;

    // 퇴장
    private Tween hideTween;


    // =========================================================
    // 상태
    // =========================================================

    // 현재 텍스트를 출력 중인가?
    private bool isTyping;

    // 텍스트 출력 완료 후 클릭을 기다리는 상태인가?
    private bool isWaitingForClick;

    // 현재 퇴장 중인가?
    private bool isHiding;


    // =========================================================
    // Event
    // =========================================================

    // 한 줄의 대화가 완전히 끝났을 때 호출
    public Action OnConversationComplete;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        rectTransform =
            GetComponent<RectTransform>();

        canvasGroup =
            GetComponent<CanvasGroup>();

        originalPosition =
            rectTransform.anchoredPosition;


        if (nextArrow != null)
            nextArrow.SetActive(false);
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;


        if (Input.GetMouseButtonDown(0))
        {
            OnClick();
        }
    }


    // =========================================================
    // 클릭
    // =========================================================

    private void OnClick()
    {
        // 퇴장 중이면 클릭 무시
        if (isHiding)
            return;


        // -----------------------------------------------------
        // 1. 대사 출력 중
        //
        // 클릭하면 대사를 전부 출력하고
        // 화살표를 바로 표시
        // -----------------------------------------------------

        if (isTyping)
        {
            CompleteText();

            return;
        }


        // -----------------------------------------------------
        // 2. 대사가 전부 출력되고
        // 화살표가 나온 상태
        //
        // 클릭하면 대화창 종료
        // -----------------------------------------------------

        if (isWaitingForClick)
        {
            Hide();

            return;
        }
    }


    // =========================================================
    // 일반 랜덤 대화
    // =========================================================

    public void Talking(
        PlayerCharacterData PCD,
        Situation situation)
    {
        if (PCD == null)
            return;


        CharacterInfo characterInfo =
            PCD.player_character_info;


        if (characterInfo == null)
            return;


        string conversation =
            characterInfo.GetDialogue(situation);


        if (string.IsNullOrEmpty(conversation))
            return;


        Talking(
            characterInfo,
            conversation
        );
    }


    // =========================================================
    // ConversationSO 대화
    // =========================================================

    public void Talking(
        CharacterInfo characterInfo,
        string conversation)
    {
        if (characterInfo == null)
            return;


        if (string.IsNullOrEmpty(conversation))
            return;


        // 캐릭터 전신 일러스트
        characterImage.sprite =
            characterInfo.character_full_art;


        // 대화 출력
        Talking(conversation);
    }


    // =========================================================
    // 대화 출력
    // =========================================================

    public void Talking(string conversation)
    {
        // -----------------------------------------------------
        // 기존 Tween 전부 정리
        // -----------------------------------------------------

        showTween?.Kill();
        textTween?.Kill();
        arrowTween?.Kill();
        hideTween?.Kill();


        // -----------------------------------------------------
        // 상태 초기화
        // -----------------------------------------------------

        isTyping = true;
        isWaitingForClick = false;
        isHiding = false;


        // -----------------------------------------------------
        // 화살표 숨기기
        // -----------------------------------------------------

        if (nextArrow != null)
            nextArrow.SetActive(false);


        // -----------------------------------------------------
        // 활성화
        // -----------------------------------------------------

        gameObject.SetActive(true);


        // -----------------------------------------------------
        // 위치 초기화
        // -----------------------------------------------------

        rectTransform.anchoredPosition =
            originalPosition;


        // -----------------------------------------------------
        // Fade 초기화
        // -----------------------------------------------------

        canvasGroup.alpha = 0f;


        // -----------------------------------------------------
        // 텍스트 설정
        // -----------------------------------------------------

        characterConversationText.text =
            conversation;


        characterConversationText.ForceMeshUpdate();


        int characterCount =
            characterConversationText.textInfo.characterCount;


        characterConversationText.maxVisibleCharacters =
            0;


        // -----------------------------------------------------
        // 글자가 없는 경우
        // -----------------------------------------------------

        if (characterCount <= 0)
        {
            CompleteText();

            return;
        }


        // =====================================================
        // 등장 Fade
        // =====================================================

        showTween =
            canvasGroup.DOFade(
                1f,
                showDuration
            );


        // =====================================================
        // 글자 출력
        // =====================================================

        float textDuration =
            characterCount * textSpeed;


        textTween =
            DOTween.To(
                () =>
                    characterConversationText.maxVisibleCharacters,

                value =>
                    characterConversationText.maxVisibleCharacters =
                        value,

                characterCount,

                textDuration
            )
            .OnComplete(() =>
            {
                // 글자 출력 완료
                isTyping = false;


                // 자연스럽게 끝난 경우
                // → 0.5초 후 화살표
                ShowArrowAfterDelay();
            });
    }


    // =========================================================
    // 텍스트 즉시 완료
    // =========================================================

    private void CompleteText()
    {
        // -----------------------------------------------------
        // 현재 텍스트 Tween 완전히 제거
        // -----------------------------------------------------

        textTween?.Kill();
        textTween = null;


        // -----------------------------------------------------
        // 텍스트 전체 표시
        // -----------------------------------------------------

        characterConversationText.maxVisibleCharacters =
            characterConversationText.textInfo.characterCount;


        // -----------------------------------------------------
        // 타이핑 종료
        // -----------------------------------------------------

        isTyping = false;


        // -----------------------------------------------------
        // 기존 화살표 예약 제거
        // -----------------------------------------------------

        arrowTween?.Kill();
        arrowTween = null;


        // -----------------------------------------------------
        // 클릭으로 완료한 경우
        // → 화살표 즉시 표시
        // -----------------------------------------------------

        ShowArrowAfterDelay(0f);
    }


    // =========================================================
    // 기본 화살표 표시
    // =========================================================
    //
    // 인자를 넣지 않으면 Inspector의 arrowDelay 사용
    //
    // 예:
    //
    // ShowArrowAfterDelay();
    // → 0.5초 후
    //
    // ShowArrowAfterDelay(0f);
    // → 즉시
    //
    // =========================================================

    private void ShowArrowAfterDelay()
    {
        ShowArrowAfterDelay(arrowDelay);
    }


    // =========================================================
    // 지정 시간 후 화살표 표시
    // =========================================================

    private void ShowArrowAfterDelay(float delay)
    {
        if (!gameObject.activeSelf)
            return;


        // -----------------------------------------------------
        // 화살표가 없다면
        // 바로 클릭 대기 상태
        // -----------------------------------------------------

        if (nextArrow == null)
        {
            isWaitingForClick = true;

            return;
        }


        // -----------------------------------------------------
        // 기존 화살표 숨기기
        // -----------------------------------------------------

        nextArrow.SetActive(false);


        // -----------------------------------------------------
        // 기존 예약 제거
        // -----------------------------------------------------

        arrowTween?.Kill();


        // -----------------------------------------------------
        // 지정 시간 후 화살표 표시
        // -----------------------------------------------------

        arrowTween =
            DOVirtual.DelayedCall(
                delay,
                () =>
                {
                    if (!gameObject.activeSelf)
                        return;


                    nextArrow.SetActive(true);


                    isWaitingForClick = true;
                }
            );
    }


    // =========================================================
    // 대화창 숨기기
    // =========================================================

    private void Hide()
    {
        if (!gameObject.activeSelf)
            return;


        // -----------------------------------------------------
        // 상태 변경
        // -----------------------------------------------------

        isTyping = false;
        isWaitingForClick = false;
        isHiding = true;


        // -----------------------------------------------------
        // Tween 정리
        // -----------------------------------------------------

        showTween?.Kill();
        textTween?.Kill();
        arrowTween?.Kill();
        hideTween?.Kill();


        // -----------------------------------------------------
        // 화살표 숨기기
        // -----------------------------------------------------

        if (nextArrow != null)
            nextArrow.SetActive(false);


        // =====================================================
        // 퇴장 Sequence
        // =====================================================

        Sequence sequence =
            DOTween.Sequence();


        // -----------------------------------------------------
        // 아래로 이동
        // -----------------------------------------------------

        sequence.Append(
            rectTransform.DOAnchorPosY(
                originalPosition.y - hideDistance,
                hideDuration
            )
        );


        // -----------------------------------------------------
        // Fade Out
        // -----------------------------------------------------

        sequence.Join(
            canvasGroup.DOFade(
                0f,
                hideDuration
            )
        );


        hideTween = sequence;


        // -----------------------------------------------------
        // 퇴장 완료
        // -----------------------------------------------------

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);


            // 위치 복구
            rectTransform.anchoredPosition =
                originalPosition;


            // 투명도 복구
            canvasGroup.alpha = 1f;


            isHiding = false;


            // -------------------------------------------------
            // 한 줄의 대화가 완전히 끝남
            // -------------------------------------------------

            OnConversationComplete?.Invoke();
        });
    }
}
