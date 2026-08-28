using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

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

    [Header("Localization")]
    [SerializeField] private string localizationTable = "UI_Text";

    // 현재 런타임 카드
    private CardVariable currentCard;

    // 현재 표시 중인 CardData
    private CardData currentCardData;

    private Dictionary<string, Func<int, int>> valueFunctions;


    // =========================================================
    // 초기화
    // =========================================================

    private void Awake()
    {
        InitializeValueFunctions();
    }


    // =========================================================
    // 수치 함수
    // =========================================================

    private void InitializeValueFunctions()
    {
        valueFunctions = new Dictionary<string, Func<int, int>>
        {
            { "DMG", DMG }
        };
    }


    // =========================================================
    // 데미지 계산
    // =========================================================

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


    // =========================================================
    // 카드 설명 번역
    // =========================================================

    private string GetLocalizedCardDescription(CardData CD)
    {
        if (CD == null)
            return "";


        string originalText = CD.card_description;


        // 원본 설명이 없으면 그대로 반환
        if (string.IsNullOrEmpty(originalText))
            return originalText;


        string localizedText =
            LocalizationSettings.StringDatabase.GetLocalizedString(
                localizationTable,
                originalText
            );


        // 번역이 없으면 원본 SO의 설명 사용
        if (string.IsNullOrEmpty(localizedText) ||
            localizedText.Contains("No translation found"))
        {
            return originalText;
        }


        return localizedText;
    }


    // =========================================================
    // 카드 이름 번역
    // =========================================================

    private string GetLocalizedCardName(CardData CD)
    {
        if (CD == null)
            return "";


        string originalName = CD.card_name;


        if (string.IsNullOrEmpty(originalName))
            return originalName;


        string localizedName =
            LocalizationSettings.StringDatabase.GetLocalizedString(
                localizationTable,
                originalName
            );


        // 번역이 없으면 원본 이름 사용
        if (string.IsNullOrEmpty(localizedName) ||
            localizedName.Contains("No translation found"))
        {
            return originalName;
        }


        return localizedName;
    }


    // =========================================================
    // 카드 초기화 - CardData
    // =========================================================

    public void CardInit(CardData CD)
    {
        currentCard = null;
        currentCardData = CD;


        card_image.sprite =
            CD.card_image;


        card_name.text =
            GetLocalizedCardName(CD);


        card_type.text =
            CD.cardType.ToString();


        card_cost.text =
            CD.card_cost.ToString();


        card_text.text =
            MakeBaseText(
                GetLocalizedCardDescription(CD)
            );
    }


    // =========================================================
    // 카드 초기화 - Character + CardData
    // =========================================================

    public void CardInit(
        CharacterVariable CV,
        CardData CD)
    {
        currentCard = null;
        currentCardData = CD;


        card_image.sprite =
            CD.card_image;


        card_name.text =
            GetLocalizedCardName(CD);
    }


    // =========================================================
    // 카드 초기화 - CardVariable
    // =========================================================

    public void CardInit(CardVariable CV)
    {
        currentCard = CV;


        CardData CD =
            CV.original_card_info;


        currentCardData = CD;


        card_image.sprite =
            CD.card_image;


        card_name.text =
            GetLocalizedCardName(CD);


        card_cost.text =
            CV.current_card_cost.ToString();


        card_text.text =
            MakeBaseText(
                GetLocalizedCardDescription(CD)
            );
    }


    // =========================================================
    // 카드 정보 업데이트
    // =========================================================

    public void CardInfoUpdate()
    {
        if (currentCard == null)
            return;


        CardInit(currentCard);
    }


    // =========================================================
    // 언어 변경 시 호출
    // =========================================================

    public void RefreshLocalization()
    {
        CardData CD = null;


        // 런타임 카드가 있으면 런타임 카드의 원본 데이터 사용
        if (currentCard != null)
        {
            CD =
                currentCard.original_card_info;
        }
        // 일반 CardData 카드
        else if (currentCardData != null)
        {
            CD =
                currentCardData;
        }


        if (CD == null)
            return;


        // =====================================================
        // 카드 이름
        // =====================================================

        card_name.text =
            GetLocalizedCardName(CD);


        // =====================================================
        // 카드 설명
        // =====================================================

        card_text.text =
            MakeBaseText(
                GetLocalizedCardDescription(CD)
            );


        // =====================================================
        // 카드 비용
        // =====================================================

        if (currentCard != null)
        {
            card_cost.text =
                currentCard.current_card_cost.ToString();
        }
        else
        {
            card_cost.text =
                CD.card_cost.ToString();
        }


        // =====================================================
        // 카드 타입
        // =====================================================

        card_type.text =
            CD.cardType.ToString();
    }


    // =========================================================
    // 카드 설명 / 동적 수치
    // =========================================================

    public string MakeBaseText(string text)
    {
        string result = text;


        // =====================================================
        // 동적 수치 치환
        // =====================================================

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


        // =====================================================
        // 명중률
        // =====================================================

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


    // =========================================================
    // 런타임 카드 텍스트
    // =========================================================

    public string MakeRuntimeText(
        CharacterVariable CV)
    {
        if (currentCard == null)
            return "";


        return MakeBaseText(
            GetLocalizedCardDescription(
                currentCard.original_card_info
            )
        );
    }


    // =========================================================
    // 카드 상태 표시
    // =========================================================

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