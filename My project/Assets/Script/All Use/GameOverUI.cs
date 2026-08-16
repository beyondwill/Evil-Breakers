using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup buttonCanvasGroup;

    [SerializeField] private TextMeshProUGUI gameOverTitle;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Button returnButton;


    [Header("강조할 오브젝트")]
    [SerializeField] private Transform highlightTarget;

    // 강조 대상에 런타임으로 추가한 Canvas
    private Canvas highlightCanvas;


    [Header("Animation")]
    [SerializeField] private float blackFadeDuration = 3.0f;
    [SerializeField] private float titleDelay = 1.0f;

    // GAME OVER 등장 시간
    [SerializeField] private float titleAnimationDuration = 0.8f;

    // GAME OVER 이후 이미지 + 버튼 등장까지
    [SerializeField] private float imageButtonDelay = 0.2f;

    // 이미지와 버튼 등장 시간
    [SerializeField] private float imageFadeDuration = 0.5f;
    [SerializeField] private float buttonFadeDuration = 0.5f;


    [Header("Audio Clip")]
    [SerializeField] private AudioClip defeat;
    [SerializeField] private AudioClip no;


    private RectTransform titleRect;


    private void Awake()
    {
        titleRect = gameOverTitle.GetComponent<RectTransform>();
    }


    // ==========================================
    // 게임 오버 시작
    // ==========================================

    public void GameOverInit()
    {
        // ========================================
        // 강조 대상 최상단으로 올리기
        // ========================================

        BringHighlightToFront();


        // ========================================
        // 기존 DOTween 제거
        // ========================================

        canvasGroup.DOKill();
        titleCanvasGroup.DOKill();
        buttonCanvasGroup.DOKill();
        gameOverImage.DOKill();
        titleRect.DOKill();


        // ========================================
        // 초기 상태
        // ========================================

        // 전체 GameOver UI 완전 투명
        canvasGroup.alpha = 0f;

        // GAME OVER 완전 투명
        titleCanvasGroup.alpha = 0f;

        // 이미지 완전 투명
        SetImageAlpha(0f);

        // 버튼 완전 투명
        buttonCanvasGroup.alpha = 0f;

        // 버튼 클릭 방지
        buttonCanvasGroup.interactable = false;
        buttonCanvasGroup.blocksRaycasts = false;


        // ========================================
        // GAME OVER 가로 크기 0
        // ========================================

        Vector3 scale = titleRect.localScale;
        scale.x = 0f;
        titleRect.localScale = scale;


        // ========================================
        // 패배 사운드
        // ========================================

        AudioManager.Instance.FadeOutBGM(0.1f);

        DOVirtual.DelayedCall(0.11f, () =>
        {
            AudioManager.Instance.PlaySoundOnce(
                AudioSort.BGM,
                defeat
            );
        });


        // ========================================
        // 1. 화면이 3초 동안 검게 물듦
        // ========================================

        canvasGroup
            .DOFade(1f, blackFadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // ========================================
                // 2. 검은 화면이 된 후 잠깐 대기
                // ========================================

                DOVirtual.DelayedCall(titleDelay, () =>
                {
                    ShowGameOverTitle();
                });
            });
    }


    // ==========================================
    // 강조 대상 최상단으로
    // ==========================================

    private void BringHighlightToFront()
    {
        if (highlightTarget == null)
            return;


        // 이미 Canvas가 있다면 사용
        highlightCanvas =
            highlightTarget.GetComponent<Canvas>();


        // Canvas가 없다면 런타임 생성
        if (highlightCanvas == null)
        {
            highlightCanvas =
                highlightTarget.gameObject.AddComponent<Canvas>();
        }


        // 부모는 그대로 유지
        // 해당 오브젝트만 별도의 Sorting 적용
        highlightCanvas.overrideSorting = true;

        // GameOverUI보다 높은 순서
        highlightCanvas.sortingOrder = 999;


        // Graphic Raycast가 필요한 오브젝트가 아니라면
        // 추가 Canvas의 GraphicRaycaster는 필요 없음
    }


    // ==========================================
    // 강조 Canvas 제거
    // ==========================================

    private void RemoveHighlightCanvas()
    {
        if (highlightCanvas == null)
            return;


        // 우리가 런타임에 만든 Canvas인지 확인하려면
        // 여기서는 제거
        Destroy(highlightCanvas);

        highlightCanvas = null;
    }


    // ==========================================
    // GAME OVER 등장
    // ==========================================

    private void ShowGameOverTitle()
    {
        // GAME OVER 등장 사운드
        AudioManager.Instance.PlaySoundOnce(
            AudioSort.BGM,
            no
        );


        titleRect.DOKill();
        titleCanvasGroup.DOKill();


        // ========================================
        // GAME OVER 최종 크기
        // ========================================

        Vector3 targetScale = titleRect.localScale;
        targetScale.x = 1f;


        // ========================================
        // GAME OVER 등장
        //
        // 투명 → 불투명
        // 가로 0 → 1
        // ========================================

        Sequence titleSequence = DOTween.Sequence();


        // 투명 → 불투명
        titleSequence.Join(
            titleCanvasGroup
                .DOFade(1f, titleAnimationDuration)
                .SetEase(Ease.OutQuad)
        );


        // 가로 0 → 1
        titleSequence.Join(
            titleRect
                .DOScale(targetScale, titleAnimationDuration)
                .SetEase(Ease.OutBack)
        );


        // ========================================
        // GAME OVER 등장 완료
        // ========================================

        titleSequence.AppendInterval(imageButtonDelay);


        titleSequence.AppendCallback(() =>
        {
            ShowImageAndButton();
        });
    }


    // ==========================================
    // 이미지 + 버튼 등장
    // ==========================================

    private void ShowImageAndButton()
    {
        Sequence sequence = DOTween.Sequence();


        // ========================================
        // 이미지
        // 투명 → 불투명
        // ========================================

        sequence.Join(
            gameOverImage
                .DOFade(1f, imageFadeDuration)
                .SetEase(Ease.OutQuad)
        );


        // ========================================
        // 버튼
        // 투명 → 불투명
        // ========================================

        sequence.Join(
            buttonCanvasGroup
                .DOFade(1f, buttonFadeDuration)
                .SetEase(Ease.OutQuad)
        );


        sequence.OnComplete(() =>
        {
            // 버튼 활성화
            buttonCanvasGroup.interactable = true;
            buttonCanvasGroup.blocksRaycasts = true;
        });
    }


    // ==========================================
    // Image Alpha
    // ==========================================

    private void SetImageAlpha(float alpha)
    {
        Color color = gameOverImage.color;
        color.a = alpha;
        gameOverImage.color = color;
    }


    // ==========================================
    // 메인으로 돌아가기
    // ==========================================

    public void ReturnToMain()
    {
        // 강조 대상에 추가했던 Canvas 제거
        RemoveHighlightCanvas();


        Destroy(DataManager.Instance.gameObject);

        SceneManager.LoadScene("Main Scene");
    }


    // ==========================================
    // 오브젝트가 제거될 때
    // ==========================================

    private void OnDestroy()
    {
        // DOTween 정리
        canvasGroup?.DOKill();
        titleCanvasGroup?.DOKill();
        buttonCanvasGroup?.DOKill();
        gameOverImage?.DOKill();
        titleRect?.DOKill();
    }
}