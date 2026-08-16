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

    [Header("변수")]
    [SerializeField] private float textSpeed = 0.1f;
    [SerializeField] private float stayTime = 1f;

    [Header("등장")]
    [SerializeField] private float showDuration = 0.3f;

    [Header("퇴장")]
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private float hideDistance = 50f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;
    private Sequence talkingSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        originalPosition = rectTransform.anchoredPosition;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        // 대화창이 활성화된 상태에서 클릭하면 종료
        if (gameObject.activeSelf &&
            Input.GetMouseButtonDown(0))
        {
            Hide();
        }
    }

    // 특정 상황의 대사를 출력
    public void Talking(PlayerCharacterData PCD, Situation situation)
    {
        if (PCD == null)
            return;

        string conversation =
            PCD.player_character_info.GetDialogue(situation);

        characterImage.sprite = PCD.player_character_info.character_icon;

        Talking(conversation);
    }

    // 지정한 대화문 출력
    public void Talking(string conversation)
    {
        // 기존 대화 중단
        talkingSequence?.Kill();

        gameObject.SetActive(true);

        // 원래 위치로 초기화
        rectTransform.anchoredPosition = originalPosition;

        // 처음에는 투명하게
        canvasGroup.alpha = 0f;

        characterConversationText.text = conversation;

        characterConversationText.ForceMeshUpdate();

        int characterCount =
            characterConversationText.textInfo.characterCount;

        characterConversationText.maxVisibleCharacters = 0;

        talkingSequence = DOTween.Sequence();

        // 등장 Fade In
        talkingSequence.Append(
            canvasGroup.DOFade(
                1f,
                showDuration
            )
        );

        // 글자 출력
        talkingSequence.Append(
            DOTween.To(
                () => characterConversationText.maxVisibleCharacters,
                x => characterConversationText.maxVisibleCharacters = x,
                characterCount,
                characterCount * textSpeed
            )
        );

        // 대기
        talkingSequence.AppendInterval(stayTime);

        // 자동으로 숨기기
        talkingSequence.AppendCallback(Hide);
    }

    // 아래로 이동하면서 FadeOut
    private void Hide()
    {
        // 이미 사라지는 중이면 중복 실행 방지
        talkingSequence?.Kill();

        talkingSequence = DOTween.Sequence();

        // 아래로 이동
        talkingSequence.Append(
            rectTransform.DOAnchorPosY(
                originalPosition.y - hideDistance,
                hideDuration
            )
        );

        // 동시에 FadeOut
        talkingSequence.Join(
            canvasGroup.DOFade(
                0f,
                hideDuration
            )
        );

        // 완전히 사라지면 비활성화
        talkingSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);

            rectTransform.anchoredPosition = originalPosition;
            canvasGroup.alpha = 1f;
        });
    }
}