using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CardMoving))]
public class CardInteraction : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private CardVariable battleCard;
    private CardData deckCard;

    // 덱 카드 위치
    private int deckIndex;

    private IBattleCardInteraction battleInteraction;
    private IDeckCardInteraction deckInteraction;

    private CardMoving moving;
    private RectTransform rect;

    private void Awake()
    {
        moving = GetComponent<CardMoving>();
        rect = GetComponent<RectTransform>();
    }


    // ================================
    // 전투 카드 초기화
    // ================================
    public void Init(
        CardVariable card,
        IBattleCardInteraction interaction)
    {
        battleCard = card;
        deckCard = null;

        battleInteraction = interaction;
        deckInteraction = null;
    }


    // ================================
    // 덱 편집 카드 초기화
    // ================================
    public void Init(
        CardData card,
        IDeckCardInteraction interaction)
    {
        deckCard = card;
        battleCard = null;

        deckInteraction = interaction;
        battleInteraction = null;
    }


    // ================================
    // 덱 인덱스
    // ================================
    public void SetDeckIndex(int index)
    {
        deckIndex = index;
    }


    public CardVariable BattleCard => battleCard;

    public CardData DeckCard => deckCard;

    public int DeckIndex => deckIndex;

    public CardMoving Moving => moving;

    public RectTransform Rect => rect;


    // ================================
    // 카드 효과 발동 중인지
    // ================================
    private bool IsCardEffectRunning
    {
        get
        {
            return CardManager.Instance != null &&
                   CardManager.Instance.IsCardEffectRunning;
        }
    }


    // ================================
    // 클릭
    // ================================
    public void OnPointerClick(
        PointerEventData eventData)
    {
        // 전투 카드만 차단
        if (battleInteraction != null &&
            IsCardEffectRunning)
        {
            return;
        }


        if (battleInteraction != null)
        {
            if (eventData.button ==
                PointerEventData.InputButton.Left)
            {
                battleInteraction.LeftClick(this);
            }
            else if (eventData.button ==
                     PointerEventData.InputButton.Right)
            {
                battleInteraction.RightClick(this);
            }

            return;
        }


        if (deckInteraction != null)
        {
            if (eventData.button ==
                PointerEventData.InputButton.Left)
            {
                deckInteraction.LeftClick(this);
            }
            else if (eventData.button ==
                     PointerEventData.InputButton.Right)
            {
                deckInteraction.RightClick(deckCard);
            }
        }
    }


    // ================================
    // 드래그 시작
    // ================================
    public void OnBeginDrag(
        PointerEventData eventData)
    {
        // 전투 카드 효과 발동 중이면 무시
        if (battleInteraction != null &&
            IsCardEffectRunning)
        {
            return;
        }


        if (battleInteraction != null)
        {
            CardDragController.Instance?
                .BeginDrag(this);
        }
        else
        {
            deckInteraction?
                .BeginDrag(deckCard);
        }
    }


    // ================================
    // 드래그 중
    // ================================
    public void OnDrag(
        PointerEventData eventData)
    {
        // 전투 카드 효과 발동 중이면 무시
        if (battleInteraction != null &&
            IsCardEffectRunning)
        {
            return;
        }


        if (battleInteraction != null)
        {
            CardDragController.Instance?
                .Drag(this);
        }
        else
        {
            deckInteraction?
                .Drag(deckCard);
        }
    }


    // ================================
    // 드래그 종료
    // ================================
    public void OnEndDrag(
        PointerEventData eventData)
    {
        // 전투 카드 효과 발동 중이면 무시
        if (battleInteraction != null &&
            IsCardEffectRunning)
        {
            return;
        }


        if (battleInteraction != null)
        {
            CardDragController.Instance?
                .EndDrag(this);
        }
        else
        {
            deckInteraction?
                .EndDrag(deckCard);
        }
    }


    // ================================
    // Hover 시작
    // ================================
    public void OnPointerEnter(
        PointerEventData eventData)
    {
        // 전투 카드 효과 발동 중이면 Hover 무시
        if (battleInteraction != null &&
            IsCardEffectRunning)
        {
            return;
        }


        battleInteraction?
            .PointerEnter(this);
    }


    // ================================
    // Hover 종료
    // ================================
    public void OnPointerExit(
        PointerEventData eventData)
    {
        // Exit는 항상 호출
        // Hover 상태가 꼬이는 것을 방지
        battleInteraction?
            .PointerExit(this);
    }
}