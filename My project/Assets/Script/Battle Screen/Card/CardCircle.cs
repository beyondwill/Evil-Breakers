using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CardCircle : MonoBehaviour, IBattleCardInteraction
{
    public static CardCircle Instance;


    [Header("Card")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private RectTransform graveyardTarget;
    [SerializeField] private List<CardMoving> cards = new();


    [Header("Layout")]
    [SerializeField] private float radius = 900f;


    [Header("Hover")]
    [SerializeField] private float hoverMoveY = 150f;

    [SerializeField] private float hoverScale = 1.25f;

    [SerializeField] private float sideMove = 40f;


    private CardMoving hoverCard;
    private CardMoving draggingCard;


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        Sequence seq = DOTween.Sequence();
    }



    #region Card

    public void CardAdd(CardVariable cardData)
    {
        GameObject obj =
            Instantiate(
                cardPrefab,
                transform,
                false
            );


        CardMoving moving =
            obj.GetComponent<CardMoving>();

        CardView cardView =
            obj.GetComponent<CardView>();


        cardView.CardInit(cardData);

        cardView.RefreshPlayableState();


        obj.GetComponent<CardInteraction>()
            .Init(
                cardData,
                this
            );


        cards.Add(moving);


        RectTransform rt =
            moving.GetComponent<RectTransform>();

        rt.anchoredPosition =
            new Vector2(0, -1200);


        RefreshLayout();


        moving.MoveCard(
            moving.GetCoordinate().x,
            moving.GetCoordinate().y,
            moving.Angle,
            0.5f
        );

        moving.SetGraveyardTarget(graveyardTarget);
    }



    public void RefreshCardStates()
    {
        foreach (CardMoving card in cards)
        {
            if (card == null)
                continue;

            CardView view =
                card.GetComponent<CardView>();

            if (view == null)
                continue;

            view.RefreshPlayableState();
        }
    }



    public void CardRemove(int index)
    {
        if (index < 0 || index >= cards.Count)
            return;


        CardMoving card =
            cards[index];


        cards.RemoveAt(index);

        Destroy(card.gameObject);

        RefreshLayout();
    }



    public void CardRemove(CardMoving card)
    {
        if (card == null)
            return;


        cards.Remove(card);

        Destroy(card.gameObject);

        draggingCard = null;

        RefreshLayout();
    }



    public void StartDragCard(CardMoving card)
    {
        draggingCard = card;

        ClearHover();

        RefreshLayout(true);
    }



    public void EndDragCard(CardMoving card)
    {
        draggingCard = null;

        RefreshLayout();
    }

    #endregion



    #region Layout

    public void RefreshLayout(bool animate = true)
    {
        float angleStep;

        int count = 0;


        foreach (CardMoving card in cards)
        {
            if (card != draggingCard)
                count++;
        }


        if (count <= 3)
            angleStep = 15f;
        else if (count <= 6)
            angleStep = 10f;
        else
            angleStep = 7.5f;



        int layoutIndex = 0;


        foreach (CardMoving card in cards)
        {
            if (card == draggingCard)
                continue;


            float angle =
                -angleStep * (count - 1) * 0.5f
                +
                layoutIndex * angleStep;


            float rad =
                angle * Mathf.Deg2Rad;


            float x =
                Mathf.Sin(rad) * radius;

            float y =
                Mathf.Cos(rad) * radius;


            card.CardInit(layoutIndex);


            card.SetCoordinate(
                x,
                y,
                angle
            );


            card.MoveCard(
                x,
                y,
                angle,
                animate ? 0.5f : 0f
            );


            card.transform.SetSiblingIndex(layoutIndex);

            layoutIndex++;
        }
    }

    #endregion



    #region Hover

    public void PointerEnter(CardInteraction card)
    {
        if (CardDragController.Instance.IsDragging)
            return;


        CardMoving moving =
            card.Moving;


        ClearHover();

        hoverCard = moving;


        int index =
            cards.IndexOf(moving);


        moving.HoverCard(
            hoverMoveY,
            hoverScale
        );


        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == moving)
                continue;


            Vector2 pos =
                cards[i].GetCoordinate();


            if (i < index)
                pos += Vector2.left * sideMove;
            else
                pos += Vector2.right * sideMove;


            cards[i].MoveOffset(
                pos,
                0.3f
            );
        }


        moving.transform.SetAsLastSibling();
    }



    public void PointerExit(CardInteraction card)
    {
        if (CardDragController.Instance != null &&
           CardDragController.Instance.IsDragging)
            return;


        ClearHover();
    }



    public void ClearHover()
    {
        foreach (CardMoving card in cards)
        {
            if (card == draggingCard)
                continue;

            card.ResetHover();
        }


        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(i);
        }


        hoverCard = null;
    }

    public void DiscardAllCards()
    {
        List<CardMoving> discardCards =
            new(cards);


        foreach (CardMoving card in discardCards)
        {
            card.DiscardMove();
        }


        cards.Clear();
    }

    #endregion



    #region Interaction

    public void LeftClick(CardInteraction card)
    {

    }


    public void RightClick(CardInteraction card)
    {

    }


    public void BeginDrag(CardInteraction card)
    {

    }


    public void Drag(CardInteraction card)
    {

    }


    public void EndDrag(CardInteraction card)
    {

    }

    #endregion
}