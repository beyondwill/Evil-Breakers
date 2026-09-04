using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    // =========================================================
    // 외부 요소
    // =========================================================

    [SerializeField] private GameObject show_and_hide;
    [SerializeField] private GameObject scrollview_contents;
    [SerializeField] private GameObject icon_prefab;
    [SerializeField] private GameObject need_more_cards;

    [SerializeField] private CardManageBox player_deck_cards;
    [SerializeField] private CardManageBox storage_deck_cards;


    // =========================================================
    // 현재 보고 있는 캐릭터
    // =========================================================

    private PlayerCharacterData currentPCD;


    // =========================================================
    // 현재 선택된 캐릭터 아이콘
    // =========================================================

    private IconButton selectedCharacterIcon;

    private Color selectedColor = Color.red;
    private Color selectedCharacterNormalColor;


    // =========================================================
    // 카드 상호작용
    // =========================================================

    private DeckCardInteraction deckInteractionPlayer;
    private DeckCardInteraction deckInteractionStorage;


    // =========================================================
    // 현재 캐릭터에게 표시할 보관함 카드
    // =========================================================

    private List<CardData> filteredStorageCards =
        new List<CardData>();


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        deckInteractionPlayer =
            new DeckCardInteraction(
                this,
                true
            );


        deckInteractionStorage =
            new DeckCardInteraction(
                this,
                false
            );
    }


    // =========================================================
    // Start
    // =========================================================

    private IEnumerator Start()
    {
        yield return null;


        show_and_hide.SetActive(false);


        ShowPlayerCharacter();
    }


    // =========================================================
    // 초기 화면
    // =========================================================

    public void ShowInit()
    {
        show_and_hide.SetActive(false);


        ShowPlayerCharacter();
    }


    // =========================================================
    // 캐릭터 목록 표시
    // =========================================================

    public void ShowPlayerCharacter()
    {
        foreach (Transform child in
                 scrollview_contents.transform)
        {
            Destroy(child.gameObject);
        }


        List<PlayerCharacterData> pcdList =
            DataManager.Instance
                .GetAllData
                .main_data
                .player_character_data_list;


        foreach (PlayerCharacterData pcd in pcdList)
        {
            GameObject characterIcon =
                Instantiate(
                    icon_prefab,
                    scrollview_contents.transform,
                    false
                );


            IconButton iconButton =
                characterIcon.GetComponent<IconButton>();


            Color normalColor =
                pcd.player_character_info
                    .icon_background_color;


            iconButton.SetColor(
                normalColor
            );


            iconButton.SetImage(
                pcd.player_character_info
                    .character_icon
            );


            PlayerCharacterData capturedPCD =
                pcd;


            iconButton.ActionAdd(() =>
            {
                // -------------------------------------------------
                // 이전 선택 복구
                // -------------------------------------------------

                if (selectedCharacterIcon != null)
                {
                    selectedCharacterIcon.SetColor(
                        selectedCharacterNormalColor
                    );
                }


                // -------------------------------------------------
                // 현재 선택
                // -------------------------------------------------

                selectedCharacterIcon =
                    iconButton;


                selectedCharacterNormalColor =
                    normalColor;


                selectedCharacterIcon.SetColor(
                    selectedColor
                );


                // -------------------------------------------------
                // 덱 표시
                // -------------------------------------------------

                ShowCharacterDeck(
                    capturedPCD
                );
            });
        }
    }


    // =========================================================
    // 선택한 캐릭터 덱 표시
    // =========================================================

    public void ShowCharacterDeck(
        PlayerCharacterData pcd)
    {
        if (pcd == null)
            return;


        currentPCD = pcd;


        // =====================================================
        // 현재 캐릭터 직업
        // =====================================================

        CharacterClass currentClass =
            pcd.player_character_info
                .characterClass;


        // =====================================================
        // UI 활성화
        // =====================================================

        show_and_hide.SetActive(true);


        // =====================================================
        // 플레이어 덱
        // =====================================================

        player_deck_cards.RemoveAllCards();


        // 현재 직업 전달
        player_deck_cards.SetCurrentJob(
            currentClass
        );


        player_deck_cards.ShowCards(
            pcd.player_character_deck,
            deckInteractionPlayer
        );


        // =====================================================
        // 보관함 카드 필터링
        // =====================================================

        filteredStorageCards.Clear();


        List<CardData> storageDeck =
            DataManager.Instance
                .GetAllData
                .main_data
                .storage_cards_list;


        int neutralCardCount = 0;


        foreach (CardData card in storageDeck)
        {
            if (card == null)
                continue;


            // -------------------------------------------------
            // 직업 카드
            // -------------------------------------------------

            if (card.characterClass == currentClass)
            {
                filteredStorageCards.Add(card);

                continue;
            }


            // -------------------------------------------------
            // 중립 카드
            // -------------------------------------------------

            if (card.characterClass ==
                CharacterClass.Neutral)
            {
                if (neutralCardCount < 2)
                {
                    filteredStorageCards.Add(card);

                    neutralCardCount++;
                }
            }
        }


        // =====================================================
        // 보관함 표시
        // =====================================================

        storage_deck_cards.RemoveAllCards();


        // 현재 직업 전달
        storage_deck_cards.SetCurrentJob(
            currentClass
        );


        storage_deck_cards.ShowCards(
            filteredStorageCards,
            deckInteractionStorage
        );
    }


    // =========================================================
    // 카드 이동
    // =========================================================

    public void MoveCard(
        CardData card,
        bool isPlayer)
    {
        if (currentPCD == null)
            return;


        if (card == null)
            return;


        List<CardData> playerDeck =
            currentPCD.player_character_deck;


        List<CardData> storageDeck =
            DataManager.Instance
                .GetAllData
                .main_data
                .storage_cards_list;


        // =====================================================
        // 플레이어 덱 → 보관함
        // =====================================================

        if (isPlayer)
        {
            // -------------------------------------------------
            // 최소 5장 유지
            // -------------------------------------------------

            if (playerDeck.Count <= 5)
            {
                if (need_more_cards != null &&
                    !need_more_cards.activeSelf)
                {
                    need_more_cards.SetActive(true);
                }


                return;
            }


            // -------------------------------------------------
            // 실제 덱에 존재하는 카드인지 확인
            // -------------------------------------------------

            if (!playerDeck.Contains(card))
                return;


            // -------------------------------------------------
            // 이동
            // -------------------------------------------------

            playerDeck.Remove(card);


            storageDeck.Add(card);
        }


        // =====================================================
        // 보관함 → 플레이어 덱
        // =====================================================

        else
        {
            // -------------------------------------------------
            // 실제 보관함에 존재하는지 확인
            // -------------------------------------------------

            if (!storageDeck.Contains(card))
                return;


            // -------------------------------------------------
            // 현재 캐릭터가 사용할 수 있는 카드인지 확인
            // -------------------------------------------------

            CharacterClass currentClass =
                currentPCD.player_character_info
                    .characterClass;


            bool isValidCard =
                card.characterClass == currentClass ||
                card.characterClass == CharacterClass.Neutral;


            if (!isValidCard)
                return;


            // -------------------------------------------------
            // 이동
            // -------------------------------------------------

            storageDeck.Remove(card);


            playerDeck.Add(card);
        }


        // =====================================================
        // UI 갱신
        // =====================================================

        ShowCharacterDeck(
            currentPCD
        );


        // =====================================================
        // 저장
        // =====================================================

        DataManager.Instance.SaveData();
    }
}