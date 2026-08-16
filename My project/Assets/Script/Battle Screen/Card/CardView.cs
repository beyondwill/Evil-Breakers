using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject cost_box;
    [SerializeField] private Image card_image;
    [SerializeField] private TextMeshProUGUI card_name;
    [SerializeField] private TextMeshProUGUI card_type;
    [SerializeField] private TextMeshProUGUI card_cost;
    [SerializeField] private TextMeshProUGUI card_text;

    [Header("State")]
    [SerializeField] private GameObject Playable;
    [SerializeField] private GameObject Special;

    private CardVariable currentCard;

    private Dictionary<string, Func<int, int>> valueFunctions;


    private void Awake()
    {
        InitializeValueFunctions();
    }


    // =========================
    // 수치 함수
    // =========================

    private void InitializeValueFunctions()
    {
        valueFunctions = new Dictionary<string, Func<int, int>>
        {
            { "DMG", DMG }
        };
    }


    // =========================
    // 데미지 계산
    // =========================

    private int DMG(int value)
    {
        if (CardManager.Instance == null)
            return value;


        PlayerCharacterVariable character =
            CardManager.Instance.GetCurrentCharacter();


        if (character == null)
            return value;


        // 캐릭터 기본 공격력
        int attack =
            (int)character.statContainer.GetBaseStat(
                CharacterBaseStatType.Attack
            );


        // 힘 버프
        int strength =
            (int)character.statContainer.GetBuff(
                CharacterBuffType.Strength
            );


        // 카드 피해량
        int cardDamage = value;


        // 최종 표시 데미지
        int damage =
            cardDamage
            + attack
            + strength;


        return damage;
    }


    // =========================
    // 카드 초기화
    // =========================

    public void CardInit(CardData CD)
    {
        currentCard = null;

        card_image.sprite = CD.card_image;
        card_name.text = CD.card_name;
        card_type.text = CD.cardType.ToString();
        card_cost.text = CD.card_cost.ToString();

        card_text.text =
            MakeBaseText(
                CD.card_description
            );
    }


    public void CardInit(
        CharacterVariable CV,
        CardData CD)
    {
        currentCard = null;

        card_image.sprite = CD.card_image;
        card_name.text = CD.card_name;
    }


    public void CardInit(CardVariable CV)
    {
        currentCard = CV;

        CardData CD =
            CV.original_card_info;

        card_image.sprite =
            CD.card_image;

        card_name.text =
            CD.card_name;

        card_cost.text =
            CV.current_card_cost.ToString();

        card_text.text =
            MakeBaseText(
                CD.card_description
            );
    }


    public void CardInfoUpdate()
    {
        if (currentCard == null)
            return;

        CardInit(currentCard);
    }


    // =========================
    // 카드 설명
    // =========================

    public string MakeBaseText(string text)
    {
        string result = text;


        result = Regex.Replace(
            result,
            @"\{([A-Za-z]+)(\d+)\}",
            match =>
            {
                string functionName =
                    match.Groups[1].Value;

                int value =
                    int.Parse(
                        match.Groups[2].Value
                    );


                // 전투 중이면 캐릭터 능력치 적용
                if (currentCard != null &&
                    valueFunctions.TryGetValue(
                        functionName,
                        out Func<int, int> function))
                {
                    return function(value)
                        .ToString();
                }


                // 전투가 아니면 원본 수치
                return value.ToString();
            });


        // =========================
        // 명중률
        // =========================

        if (currentCard != null)
        {
            if (currentCard.original_card_info.useAccuracy)
            {
                int accuracy =
                    (int)(
                        currentCard.original_card_info.accuracy
                        +
                        CardManager.Instance
                            .GetCurrentCharacter()
                            .statContainer
                            .GetBaseStat(
                                CharacterBaseStatType.Accuracy
                            )
                    );


                result +=
                    "\n명중률: " +
                    accuracy +
                    "%";
            }
            else
            {
                result +=
                    "\n명중률: 100%";
            }
        }


        return result;
    }


    public string MakeRuntimeText(
        CharacterVariable CV)
    {
        if (currentCard == null)
            return "";


        return MakeBaseText(
            currentCard
                .original_card_info
                .card_description
        );
    }


    // =========================
    // 카드 상태 표시
    // =========================

    public void ShowUnplayable()
    {
        Playable.SetActive(false);
        Special.SetActive(false);
    }


    public void ShowPlayable()
    {
        Playable.SetActive(true);
        Special.SetActive(false);
    }


    public void ShowSpecial()
    {
        Playable.SetActive(false);
        Special.SetActive(true);
    }


    public void RefreshPlayableState()
    {
        if (currentCard == null)
        {
            ShowUnplayable();
            return;
        }


        if (CardManager.Instance.CanStartCard(
                currentCard))
        {
            ShowPlayable();
        }
        else
        {
            ShowUnplayable();
        }
    }
}