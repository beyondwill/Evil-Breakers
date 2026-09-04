using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardManageBox : MonoBehaviour
{
    // =========================================================
    // 외부 요소
    // =========================================================

    [SerializeField] private GameObject card_content;
    [SerializeField] private GameObject card_prefab;


    // =========================================================
    // 필터 버튼
    // =========================================================

    [Header("Filter Buttons")]

    [SerializeField] private Image allButton;
    [SerializeField] private Image jobButton;
    [SerializeField] private Image neutralButton;
    [SerializeField] private Image attackButton;
    [SerializeField] private Image skillButton;


    [Header("Filter Button Text")]

    [SerializeField] private TMP_Text allText;
    [SerializeField] private TMP_Text jobText;
    [SerializeField] private TMP_Text neutralText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text skillText;


    [Header("Button Colors")]

    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color normalColor = Color.gray;

    [SerializeField] private Color selectedTextColor = Color.black;
    [SerializeField] private Color normalTextColor = Color.gray;


    // =========================================================
    // 필터 모드
    // =========================================================

    public enum CardFilterMode
    {
        Character,
        Shop
    }


    [Header("Filter Mode")]

    [SerializeField]
    private CardFilterMode filterMode =
        CardFilterMode.Character;


    // =========================================================
    // 카드
    // =========================================================

    public List<CardData> cards;


    // 현재 카드 상호작용
    private IDeckCardInteraction currentInteraction;


    // =========================================================
    // 현재 캐릭터 직업
    // =========================================================

    [SerializeField] private CharacterClass currentJob;


    // =========================================================
    // 현재 필터
    // =========================================================

    private CardFilter currentFilter = CardFilter.All;


    private enum CardFilter
    {
        All,
        Job,
        Neutral,
        Attack,
        Skill
    }



    // =========================================================
    // 초기화
    // =========================================================

    private void Start()
    {
        UpdateButtonColor();
    }



    // =========================================================
    // 필터 모드 설정
    // =========================================================

    public void SetFilterMode(CardFilterMode mode)
    {
        filterMode = mode;

        currentFilter = CardFilter.All;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 현재 직업 설정
    // =========================================================

    public void SetCurrentJob(CharacterClass job)
    {
        currentJob = job;

        // 캐릭터 모드에서 직업 변경 시 갱신
        if (filterMode == CardFilterMode.Character &&
            currentFilter == CardFilter.Job)
        {
            RefreshCards();
        }
    }



    // =========================================================
    // 카드 추가
    // =========================================================

    public void CardAdd(CardData card)
    {
        if (card == null)
        {
            Debug.LogError("추가하려는 카드가 null");
            return;
        }


        GameObject newCard =
            Instantiate(
                card_prefab,
                card_content.transform
            );


        CardView view =
            newCard.GetComponent<CardView>();


        if (view != null)
        {
            view.CardInit(card);
        }


        CardInteraction interaction =
            newCard.GetComponent<CardInteraction>();


        if (interaction != null)
        {
            interaction.Init(
                card,
                currentInteraction
            );
        }
    }



    // =========================================================
    // 카드 제거
    // =========================================================

    public void CardRemove(int index)
    {

    }



    // =========================================================
    // 모든 카드 제거
    // =========================================================

    public void RemoveAllCards()
    {
        foreach (Transform child in card_content.transform)
        {
            Destroy(child.gameObject);
        }
    }



    // =========================================================
    // 카드 표시
    // =========================================================

    public void ShowCards(List<CardData> cardList)
    {
        if (cardList == null)
            return;


        cards = cardList;

        currentInteraction = null;

        currentFilter = CardFilter.All;

        RefreshCards();

        UpdateButtonColor();
    }



    // =========================================================
    // 카드 표시 + 상호작용
    // =========================================================

    public void ShowCards(
        List<CardData> cardList,
        IDeckCardInteraction interaction)
    {
        if (cardList == null)
            return;


        cards = cardList;

        currentInteraction = interaction;

        currentFilter = CardFilter.All;

        RefreshCards();

        UpdateButtonColor();
    }



    // =========================================================
    // 전체
    // =========================================================

    public void OnClickAll()
    {
        currentFilter = CardFilter.All;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 직업
    // =========================================================

    public void OnClickJob()
    {
        currentFilter = CardFilter.Job;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 중립
    // =========================================================

    public void OnClickNeutral()
    {
        currentFilter = CardFilter.Neutral;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 공격
    // =========================================================

    public void OnClickAttack()
    {
        currentFilter = CardFilter.Attack;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 스킬
    // =========================================================

    public void OnClickSkill()
    {
        currentFilter = CardFilter.Skill;

        UpdateButtonColor();
        RefreshCards();
    }



    // =========================================================
    // 카드 갱신
    // =========================================================

    private void RefreshCards()
    {
        if (cards == null)
            return;


        RemoveAllCards();


        for (int i = 0; i < cards.Count; i++)
        {
            CardData card = cards[i];


            if (card == null)
            {
                Debug.LogError("카드 리스트에 null 카드 존재");
                continue;
            }


            if (!IsCardMatchFilter(card))
                continue;


            GameObject newCard =
                Instantiate(
                    card_prefab,
                    card_content.transform
                );


            // =====================================================
            // CardView
            // =====================================================

            CardView view =
                newCard.GetComponent<CardView>();


            if (view != null)
            {
                view.CardInit(card);
            }


            // =====================================================
            // CardInteraction
            // =====================================================

            CardInteraction cardInteraction =
                newCard.GetComponent<CardInteraction>();


            if (cardInteraction != null)
            {
                cardInteraction.Init(
                    card,
                    currentInteraction
                );


                // 원본 리스트의 실제 인덱스
                cardInteraction.SetDeckIndex(i);
            }
        }
    }



    // =========================================================
    // 필터 검사
    // =========================================================

    private bool IsCardMatchFilter(CardData card)
    {
        switch (currentFilter)
        {
            // -------------------------------------------------
            // 전체
            // -------------------------------------------------

            case CardFilter.All:

                return true;


            // -------------------------------------------------
            // 직업
            // -------------------------------------------------

            case CardFilter.Job:

                // 캐릭터 덱
                if (filterMode ==
                    CardFilterMode.Character)
                {
                    return card.characterClass == currentJob;
                }


                // 상점
                // 중립을 제외한 모든 직업 카드
                if (filterMode ==
                    CardFilterMode.Shop)
                {
                    return card.characterClass !=
                           CharacterClass.Neutral;
                }

                return false;


            // -------------------------------------------------
            // 중립
            // -------------------------------------------------

            case CardFilter.Neutral:

                return card.characterClass ==
                       CharacterClass.Neutral;


            // -------------------------------------------------
            // 공격
            // -------------------------------------------------

            case CardFilter.Attack:

                return card.cardType ==
                       CardType.Attack;


            // -------------------------------------------------
            // 스킬
            // -------------------------------------------------

            case CardFilter.Skill:

                return card.cardType ==
                       CardType.Skill;
        }


        return false;
    }



    // =========================================================
    // 버튼 색상 갱신
    // =========================================================

    private void UpdateButtonColor()
    {
        // =====================================================
        // 배경
        // =====================================================

        if (allButton != null)
        {
            allButton.color =
                currentFilter == CardFilter.All
                    ? selectedColor
                    : normalColor;
        }


        if (jobButton != null)
        {
            jobButton.color =
                currentFilter == CardFilter.Job
                    ? selectedColor
                    : normalColor;
        }


        if (neutralButton != null)
        {
            neutralButton.color =
                currentFilter == CardFilter.Neutral
                    ? selectedColor
                    : normalColor;
        }


        if (attackButton != null)
        {
            attackButton.color =
                currentFilter == CardFilter.Attack
                    ? selectedColor
                    : normalColor;
        }


        if (skillButton != null)
        {
            skillButton.color =
                currentFilter == CardFilter.Skill
                    ? selectedColor
                    : normalColor;
        }


        // =====================================================
        // 글자
        // =====================================================

        if (allText != null)
        {
            allText.color =
                currentFilter == CardFilter.All
                    ? selectedTextColor
                    : normalTextColor;
        }


        if (jobText != null)
        {
            jobText.color =
                currentFilter == CardFilter.Job
                    ? selectedTextColor
                    : normalTextColor;
        }


        if (neutralText != null)
        {
            neutralText.color =
                currentFilter == CardFilter.Neutral
                    ? selectedTextColor
                    : normalTextColor;
        }


        if (attackText != null)
        {
            attackText.color =
                currentFilter == CardFilter.Attack
                    ? selectedTextColor
                    : normalTextColor;
        }


        if (skillText != null)
        {
            skillText.color =
                currentFilter == CardFilter.Skill
                    ? selectedTextColor
                    : normalTextColor;
        }
    }
}