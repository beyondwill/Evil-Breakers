using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("Card")]
    [SerializeField]
    private CardCircle cardCircle;

    [Header("Current Character")]
    [SerializeField]
    private PlayerCharacterVariable currentCharacterVariable;

    private bool isCardEffectRunning;

    public event Action<int> OnDeckChanged;
    public event Action<int> OnGraveyardChanged;

    public bool IsCardEffectRunning =>
        isCardEffectRunning;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // =========================================================
    // Current Character
    // =========================================================

    public void SetCurrentCharacter(
        PlayerCharacterVariable character)
    {
        if (currentCharacterVariable != null)
        {
            currentCharacterVariable.OnEnergyChanged -=
                BattleUIManager.Instance.ShowEnergy;
        }

        currentCharacterVariable = character;

        if (currentCharacterVariable == null)
            return;

        currentCharacterVariable.OnEnergyChanged +=
            BattleUIManager.Instance.ShowEnergy;

        BattleUIManager.Instance.ShowEnergy(
            (int)currentCharacterVariable.current_energy,
            (int)currentCharacterVariable.MaxEnergy
        );
    }


    public PlayerCharacterVariable GetCurrentCharacter()
    {
        return currentCharacterVariable;
    }


    // =========================================================
    // 카드 시작 가능 여부
    // =========================================================

    public bool CanStartCard(
        CardVariable card)
    {
        if (currentCharacterVariable == null)
            return false;

        if (card == null)
            return false;


        // -----------------------------------------------------
        // 손패 확인
        // -----------------------------------------------------

        if (!currentCharacterVariable.hand_card_list.Contains(card))
            return false;


        // -----------------------------------------------------
        // 에너지 확인
        // -----------------------------------------------------

        if (currentCharacterVariable.current_energy <
            card.current_card_cost)
        {
            return false;
        }


        // -----------------------------------------------------
        // 카드 사용 조건
        // -----------------------------------------------------

        if (!CheckUseConditions(
            card,
            null))
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // 카드 사용 조건
    // =========================================================
    //
    // useConditions의 모든 조건을 검사
    //
    // 하나라도 실패하면 false
    // =========================================================

    private bool CheckUseConditions(
        CardVariable card,
        List<CharacterVariable> targets)
    {
        if (card == null)
            return false;

        if (card.original_card_info == null)
            return false;


        List<CardCondition> conditions =
            card.original_card_info.useConditions;


        // 조건 없음
        if (conditions == null ||
            conditions.Count == 0)
        {
            return true;
        }


        foreach (
            CardCondition condition
            in conditions)
        {
            if (condition == null)
                continue;


            bool result =
                condition.Check(
                    currentCharacterVariable,
                    targets,
                    card.original_card_info
                );


            if (!result)
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // 특수 조건 검사
    // =========================================================
    //
    // 특수 조건을 만족하면
    // CardView에서 황금색으로 표시
    //
    // 주의:
    // 이 조건은 카드 사용을 막지 않는다.
    // =========================================================

    public bool CheckSpecialCondition(
        CardVariable card)
    {
        if (currentCharacterVariable == null)
            return false;

        if (card == null)
            return false;

        if (card.original_card_info == null)
            return false;


        CardCondition condition =
            card.original_card_info.specialcardCondition;


        // 특수 조건 없음
        if (condition == null)
            return false;


        return condition.Check(
            currentCharacterVariable,
            null,
            card.original_card_info
        );
    }


    // =========================================================
    // 카드 사용
    // =========================================================

    public bool UseCard(
        CardVariable usingCard,
        CharacterVariable target)
    {
        if (!CanUseCard(
            usingCard,
            target))
        {
            return false;
        }


        // -----------------------------------------------------
        // 에너지 차감
        // -----------------------------------------------------

        currentCharacterVariable.current_energy -=
            usingCard.current_card_cost;


        // -----------------------------------------------------
        // 명중 판정
        // -----------------------------------------------------

        bool isHit =
            CheckAccuracy(
                usingCard,
                target
            );


        // -----------------------------------------------------
        // 카드 효과 실행
        // -----------------------------------------------------

        ExecuteCardEffect(
            usingCard,
            target,
            isHit
        );


        // -----------------------------------------------------
        // 손패에서 제거
        // -----------------------------------------------------

        bool removed =
            currentCharacterVariable.hand_card_list.Remove(
                usingCard
            );


        if (!removed)
        {
            Debug.LogWarning(
                "CardManager : 사용한 카드가 손패에 존재하지 않습니다."
            );

            return false;
        }


        // -----------------------------------------------------
        // 묘지로 이동
        // -----------------------------------------------------

        currentCharacterVariable.graveyard_card_list.Add(
            usingCard
        );


        OnGraveyardChanged?.Invoke(
            currentCharacterVariable.graveyard_card_list.Count
        );

        OnDeckChanged?.Invoke(
            currentCharacterVariable.deck_card_list.Count
        );


        if (cardCircle != null)
        {
            cardCircle.RefreshLayout();
        }


        return true;
    }


    // =========================================================
    // 명중률
    // =========================================================

    private bool CheckAccuracy(
        CardVariable card,
        CharacterVariable target)
    {
        if (card == null)
            return false;

        if (card.original_card_info == null)
            return false;


        if (!card.original_card_info.useAccuracy)
            return true;


        if (target == null)
            return true;


        float accuracy =
            card.original_card_info.accuracy
            +
            currentCharacterVariable.statContainer.GetBaseStat(
                CharacterBaseStatType.Accuracy
            );


        bool isHit;


        if (accuracy >= 100f)
        {
            isHit = true;
        }
        else if (accuracy <= 0f)
        {
            isHit = false;
        }
        else
        {
            isHit =
                UnityEngine.Random.Range(
                    0f,
                    100f
                )
                <
                accuracy;
        }


        Debug.Log(
            "[CARD ACCURACY] " +
            card.original_card_info.card_name +
            " : " +
            accuracy +
            "% → " +
            (isHit ? "HIT" : "MISS")
        );


        return isHit;
    }


    // =========================================================
    // 실제 카드 사용 가능 여부
    // =========================================================

    public bool CanUseCard(
        CardVariable card,
        CharacterVariable target)
    {
        if (currentCharacterVariable == null)
            return false;

        if (card == null)
            return false;


        // -----------------------------------------------------
        // 손패 확인
        // -----------------------------------------------------

        if (!currentCharacterVariable.hand_card_list.Contains(card))
            return false;


        // -----------------------------------------------------
        // 에너지 확인
        // -----------------------------------------------------

        if (currentCharacterVariable.current_energy <
            card.current_card_cost)
        {
            return false;
        }


        // -----------------------------------------------------
        // 카드 데이터 확인
        // -----------------------------------------------------

        if (card.original_card_info == null)
            return false;


        // -----------------------------------------------------
        // Target 확인
        // -----------------------------------------------------

        CardTarget targetType =
            card.original_card_info.cardTarget;


        if (targetType != CardTarget.None)
        {
            if (target == null)
                return false;


            if (!IsValidTarget(
                targetType,
                target))
            {
                return false;
            }
        }


        // -----------------------------------------------------
        // 조건 검사
        // -----------------------------------------------------

        List<CharacterVariable> targets =
            new List<CharacterVariable>();


        if (target != null)
        {
            targets.Add(target);
        }


        if (!CheckUseConditions(
            card,
            targets))
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // Target 검사
    // =========================================================

    private bool IsValidTarget(
        CardTarget targetType,
        CharacterVariable target)
    {
        if (target == null)
            return false;


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


    // =========================================================
    // 카드 효과 실행
    // =========================================================

    private void ExecuteCardEffect(
        CardVariable card,
        CharacterVariable target,
        bool isHit)
    {
        if (card == null)
            return;

        if (card.original_card_info == null)
            return;


        List<CharacterVariable> targets =
            new List<CharacterVariable>();


        if (card.original_card_info.cardTarget ==
            CardTarget.None)
        {
            targets.Add(
                currentCharacterVariable
            );
        }
        else if (target != null)
        {
            targets.Add(target);
        }


        Sequence sequence =
            DOTween.Sequence();


        bool currentIsHit =
            isHit;


        isCardEffectRunning =
            true;


        foreach (
            CardEffectEntry entry
            in card.original_card_info.effects)
        {
            if (entry == null)
                continue;


            sequence.AppendInterval(
                entry.time
            );


            sequence.AppendCallback(() =>
            {
                // -------------------------------------------------
                // Visual
                // -------------------------------------------------

                if (entry.visual != null)
                {
                    entry.visual.Play(
                        currentCharacterVariable,
                        targets
                    );
                }


                // -------------------------------------------------
                // Hit
                // -------------------------------------------------

                if (currentIsHit)
                {
                    if (entry.effect != null)
                    {
                        entry.effect.Execute(
                            currentCharacterVariable,
                            targets,
                            entry,
                            card.original_card_info
                        );
                    }
                }
                else
                {
                    if (target != null)
                    {
                        Debug.Log(
                            currentCharacterVariable
                                .character_info
                                .character_name
                            +
                            " → "
                            +
                            target
                                .character_info
                                .character_name
                            +
                            " : MISS"
                        );


                        target.characterView?.Miss();
                    }
                }


                // -------------------------------------------------
                // Accuracy Reset
                // -------------------------------------------------

                if (entry.AccuracyReset)
                {
                    currentIsHit =
                        CheckAccuracy(
                            card,
                            target
                        );
                }
            });
        }


        sequence.OnComplete(() =>
        {
            isCardEffectRunning =
                false;

            RefreshHandCardState();
        });


        sequence.Play();
    }


    // =========================================================
    // 손패 상태 갱신
    // =========================================================

    private void RefreshHandCardState()
    {
        if (cardCircle == null)
            return;


        CardView[] cardViews =
            cardCircle.GetComponentsInChildren<CardView>();


        foreach (CardView cardView in cardViews)
        {
            if (cardView == null)
                continue;


            cardView.CardInfoUpdate();
        }


        cardCircle.RefreshCardStates();
    }


    // =========================================================
    // 카드 뽑기
    // =========================================================

    public void DrawCard()
    {
        if (currentCharacterVariable == null)
            return;


        if (currentCharacterVariable.deck_card_list.Count == 0)
        {
            ReshuffleGraveyardIntoDeck();
        }


        if (currentCharacterVariable.deck_card_list.Count == 0)
            return;


        CardVariable card =
            currentCharacterVariable.deck_card_list[0];


        if (card == null)
        {
            currentCharacterVariable.deck_card_list.RemoveAt(0);


            OnDeckChanged?.Invoke(
                currentCharacterVariable.deck_card_list.Count
            );


            return;
        }


        currentCharacterVariable.deck_card_list.RemoveAt(0);

        currentCharacterVariable.hand_card_list.Add(card);


        OnDeckChanged?.Invoke(
            currentCharacterVariable.deck_card_list.Count
        );


        if (cardCircle != null)
        {
            cardCircle.CardAdd(card);
        }
    }


    // =========================================================
    // 시작 손패
    // =========================================================

    public void DrawStartHand(int count)
    {
        if (currentCharacterVariable == null)
            return;

        if (count <= 0)
            return;


        int addition_draw =
            (int)currentCharacterVariable.statContainer.GetBuff(
                CharacterBuffType.Draw
            );


        for (
            int i = 0;
            i < Mathf.Max(
                count + addition_draw,
                1
            );
            i++)
        {
            DrawCard();
        }
    }


    // =========================================================
    // 묘지 → 덱
    // =========================================================

    private void ReshuffleGraveyardIntoDeck()
    {
        if (currentCharacterVariable == null)
            return;


        if (currentCharacterVariable.graveyard_card_list.Count == 0)
            return;


        currentCharacterVariable.deck_card_list.AddRange(
            currentCharacterVariable.graveyard_card_list
        );


        currentCharacterVariable.graveyard_card_list.Clear();


        ShuffleDeck();


        OnDeckChanged?.Invoke(
            currentCharacterVariable.deck_card_list.Count
        );


        OnGraveyardChanged?.Invoke(0);
    }


    // =========================================================
    // 덱 셔플
    // =========================================================

    private void ShuffleDeck()
    {
        if (currentCharacterVariable == null)
            return;


        List<CardVariable> deck =
            currentCharacterVariable.deck_card_list;


        for (
            int i = deck.Count - 1;
            i > 0;
            i--)
        {
            int random =
                UnityEngine.Random.Range(
                    0,
                    i + 1
                );


            CardVariable temp =
                deck[i];

            deck[i] =
                deck[random];

            deck[random] =
                temp;
        }
    }


    // =========================================================
    // 전체 손패 버리기
    // =========================================================

    public void DiscardHand()
    {
        if (currentCharacterVariable == null)
            return;


        if (currentCharacterVariable.hand_card_list.Count == 0)
            return;


        foreach (
            CardVariable card
            in currentCharacterVariable.hand_card_list)
        {
            if (card == null)
                continue;


            currentCharacterVariable.graveyard_card_list.Add(
                card
            );
        }


        currentCharacterVariable.hand_card_list.Clear();


        if (cardCircle != null)
        {
            cardCircle.DiscardAllCards();
        }


        OnGraveyardChanged?.Invoke(
            currentCharacterVariable.graveyard_card_list.Count
        );
    }


    // =========================================================
    // 카드 한 장 버리기
    // =========================================================

    public bool DiscardCard(
        CardVariable card)
    {
        if (currentCharacterVariable == null)
            return false;


        if (card == null)
            return false;


        if (!currentCharacterVariable.hand_card_list.Contains(card))
            return false;


        currentCharacterVariable.hand_card_list.Remove(
            card
        );


        currentCharacterVariable.graveyard_card_list.Add(
            card
        );


        OnGraveyardChanged?.Invoke(
            currentCharacterVariable.graveyard_card_list.Count
        );


        if (cardCircle != null)
        {
            cardCircle.RefreshLayout();
        }


        RefreshHandCardState();


        return true;
    }


    // =========================================================
    // 덱 개수
    // =========================================================

    public int GetDeckCount()
    {
        if (currentCharacterVariable == null)
            return 0;


        return currentCharacterVariable.deck_card_list.Count;
    }


    // =========================================================
    // 손패 개수
    // =========================================================

    public int GetHandCount()
    {
        if (currentCharacterVariable == null)
            return 0;


        return currentCharacterVariable.hand_card_list.Count;
    }


    // =========================================================
    // 묘지 개수
    // =========================================================

    public int GetGraveyardCount()
    {
        if (currentCharacterVariable == null)
            return 0;


        return currentCharacterVariable.graveyard_card_list.Count;
    }
}