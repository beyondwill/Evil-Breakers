using TMPro;
using UnityEngine;
using DG.Tweening;

public class TalkingBoxUI : MonoBehaviour
{
    [Header("외부 요소")]
    private CharacterInfo characterInfo;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI characterConversation;

    [Header("변수")]
    [SerializeField] private float textSpeed = 0.1f;
    [SerializeField] private float stayTime = 1f;
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

    // 캐릭터 정보 등록
    public void Init(CharacterInfo characterInfo)
    {
        this.characterInfo = characterInfo;
        TalkingManager.Instance.AddTalkingBox(this);
    }

    // 특정 상황의 대사를 출력
    public void Talking(Situation situation)
    {
        if (characterInfo == null)
            return;

        string conversation =
            characterInfo.GetDialogue(situation);

        Talking(conversation);
    }

    // 지정한 대화문 출력
    public void Talking(string conversation)
    {
        talkingSequence?.Kill();

        gameObject.SetActive(true);

        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 1f;

        characterConversation.text = conversation;

        characterConversation.ForceMeshUpdate();

        int characterCount =
            characterConversation.textInfo.characterCount;

        characterConversation.maxVisibleCharacters = 0;

        talkingSequence = DOTween.Sequence();

        // 글자 출력
        talkingSequence.Append(
            DOTween.To(
                () => characterConversation.maxVisibleCharacters,
                x => characterConversation.maxVisibleCharacters = x,
                characterCount,
                characterCount * textSpeed
            )
        );

        // 대기
        talkingSequence.AppendInterval(stayTime);

        // 아래로 이동
        talkingSequence.Append(
            rectTransform.DOAnchorPosY(
                originalPosition.y - hideDistance,
                hideDuration
            )
        );

        // 페이드 아웃
        talkingSequence.Join(
            canvasGroup.DOFade(
                0f,
                hideDuration
            )
        );

        talkingSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);

            rectTransform.anchoredPosition = originalPosition;
            canvasGroup.alpha = 1f;
        });
    }
}