using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardDragController : MonoBehaviour
{
    public static CardDragController Instance;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;

    [Header("Normal Card")]
    [SerializeField] private Vector2 normalCardPosition = new Vector2(0, 250f);

    [Header("Target Card")]
    [SerializeField] private RectTransform targetCardPosition;

    [Header("Animation")]
    [SerializeField] private float moveTime = 0.2f;
    [SerializeField] private float cardScale = 1.3f;

    [SerializeField] private CharactersStand charactersStand;

    private CardMoving draggingCard;
    private CardVariable draggingData;

    private bool isDragging;
    private bool isPreviewPosition;
    private bool isMovingToPreview;

    private CharacterVariable currentTarget;

    public bool IsDragging => isDragging;
    public CharacterVariable CurrentTarget => currentTarget;

    private void Awake()
    {
        Instance = this;
    }

    public void BeginDrag(CardInteraction card)
    {
        if (CardManager.Instance.IsCardEffectRunning)
            return;

        if (!CardManager.Instance.CanStartCard(card.BattleCard))
        {
            CharacterVariable CV =
                CardManager.Instance.GetCurrentCharacter();

            CV.characterView.Conversation(
                CV.character_info.GetDialogue(
                    Situation.CantStartCard
                )
            );

            return;
        }

        draggingCard = card.Moving;
        draggingData = card.BattleCard;
        isDragging = true;
        currentTarget = null;

        draggingCard.KillTween();
        draggingCard.SetFlatRotation();

        CardCircle.Instance.StartDragCard(
            draggingCard
        );

        draggingCard.transform.SetAsLastSibling();

        draggingCard.SizeCard(
            cardScale,
            moveTime
        );

        FollowMouse();
    }

    public void Drag(CardInteraction card)
    {
        if (!isDragging)
            return;

        if (isMovingToPreview)
            return;

        if (CardManager.Instance.IsCardEffectRunning)
            return;

        bool canDrop =
            BattleFieldArea.CheckDropArea(
                Input.mousePosition
            );

        if (canDrop && IsTargetCard())
        {
            if (!isPreviewPosition)
            {
                isPreviewPosition = true;
                isMovingToPreview = true;

                MoveToTargetPosition();

                var list =
                    charactersStand.GetCharacterViewList(
                        card.BattleCard.original_card_info.cardTarget
                    );

                foreach (
                    CharacterView characterView
                    in list)
                {
                    characterView.ShowLShapes(true);
                }
            }
        }
        else
        {
            isPreviewPosition = false;

            FollowMouse();

            foreach (
                CharacterView characterView
                in charactersStand.GetCharacterViewList(
                    CardTarget.Any
                ))
            {
                characterView.ShowLShapes(false);
            }
        }
    }

    private void FollowMouse()
    {
        if (draggingCard == null)
            return;

        RectTransform rt =
            draggingCard.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 mousePos
        );

        rt.DOKill();

        rt.anchoredPosition = mousePos;
        rt.localRotation = Quaternion.identity;
    }

    public void EndDrag(CardInteraction card)
    {
        if (!isDragging)
            return;

        if (CardManager.Instance.IsCardEffectRunning)
        {
            ReturnCard();
            ClearTargetPreview();
            return;
        }

        isDragging = false;

        bool canDrop =
            BattleFieldArea.CheckDropArea(
                Input.mousePosition
            );

        if (canDrop && IsTargetCard())
        {
            if (currentTarget == null)
            {
                ReturnCard();
                ClearTargetPreview();

                CharacterVariable CV =
                    CardManager.Instance.GetCurrentCharacter();

                CV.characterView.Conversation(
                    CV.character_info.GetDialogue(
                        Situation.InvalidTarget
                    )
                );

                return;
            }
        }

        if (canDrop)
            UseCard();
        else
            ReturnCard();

        ClearTargetPreview();
    }

    public void SetTarget(CharacterVariable target)
    {
        if (!IsValidTarget(target))
        {
            currentTarget = null;
            return;
        }

        currentTarget = target;
    }

    private bool IsValidTarget(
        CharacterVariable target)
    {
        if (target == null)
            return false;

        if (draggingData == null)
            return false;

        CardTarget targetType =
            draggingData.original_card_info.cardTarget;

        switch (targetType)
        {
            case CardTarget.Enemy:
                return !target.is_player_character;

            case CardTarget.Ally:
                return target.is_player_character;

            case CardTarget.Any:
                return true;

            case CardTarget.None:
                return true;

            default:
                return false;
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public bool IsTargetCard()
    {
        if (draggingData == null)
            return false;

        return
            draggingData.original_card_info.cardTarget
            != CardTarget.None;
    }

    private void ClearTargetPreview()
    {
        currentTarget = null;

        foreach (
            CharacterView characterView
            in charactersStand.GetCharacterViewList(
                CardTarget.Any
            ))
        {
            characterView.ShowLShapes(false);
        }
    }

    private void MoveToTargetPosition()
    {
        if (draggingCard == null)
            return;

        RectTransform rt =
            draggingCard.GetComponent<RectTransform>();

        rt.DOKill();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                targetCardPosition.position
            ),
            canvas.worldCamera,
            out Vector2 targetPos
        );

        Sequence seq = DOTween.Sequence();

        seq.Join(
            rt.DOAnchorPos(
                targetPos,
                moveTime
            ).SetEase(Ease.OutCubic)
        );

        seq.Join(
            rt.DOLocalRotate(
                Vector3.zero,
                moveTime
            ).SetEase(Ease.OutCubic)
        );

        seq.Join(
            rt.DOScale(
                cardScale,
                moveTime
            ).SetEase(Ease.OutCubic)
        );

        seq.OnComplete(() =>
        {
            isMovingToPreview = false;
        });
    }

    private void UseCard()
    {
        if (CardManager.Instance.IsCardEffectRunning)
        {
            ReturnCard();
            return;
        }

        if (IsTargetCard() &&
            !IsValidTarget(currentTarget))
        {
            ReturnCard();
            return;
        }

        bool success =
            CardManager.Instance.UseCard(
                draggingData,
                currentTarget
            );

        if (!success)
        {
            ReturnCard();
            return;
        }

        CardCircle.Instance.CardRemove(
            draggingCard
        );

        draggingCard = null;
        draggingData = null;
        currentTarget = null;
    }

    private void ReturnCard()
    {
        isPreviewPosition = false;
        isMovingToPreview = false;

        if (draggingCard == null)
            return;

        draggingCard.SetRaycast(true);

        CardCircle.Instance.EndDragCard(
            draggingCard
        );

        draggingCard.ResetHover();

        draggingCard.SizeCard(
            1f,
            moveTime
        );

        draggingCard = null;
        draggingData = null;
        currentTarget = null;
    }
}