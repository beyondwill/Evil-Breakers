using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private GameObject show_and_hide;
    [SerializeField] private GameObject scrollview_contents;
    [SerializeField] private GameObject icon_prefab;
    [SerializeField] private GameObject need_more_cards;

    [SerializeField] private CardManageBox player_deck_cards;
    [SerializeField] private CardManageBox storage_deck_cards;

    // 현재 보고 있는 캐릭터
    private PlayerCharacterData currentPCD;

    // 현재 선택된 캐릭터 아이콘
    private IconButton selectedCharacterIcon;

    private Color selectedColor = Color.red;
    private Color selectedCharacterNormalColor;

    // 카드 상호작용
    private DeckCardInteraction deckInteractionPlayer;
    private DeckCardInteraction deckInteractionStorage;


    private void Awake()
    {
        deckInteractionPlayer = new DeckCardInteraction(this, true);
        deckInteractionStorage = new DeckCardInteraction(this, false);
    }


    private IEnumerator Start()
    {
        // TableManager → DataManager 초기화가 먼저 끝나도록 한 프레임 대기
        yield return null;

        show_and_hide.SetActive(false);
        ShowPlayerCharacter();
    }


    public void ShowInit()
    {
        show_and_hide.SetActive(false);
        ShowPlayerCharacter();
    }


    // 캐릭터 목록 표시
    public void ShowPlayerCharacter()
    {
        foreach (Transform child in scrollview_contents.transform)
        {
            Destroy(child.gameObject);
        }

        List<PlayerCharacterData> pcdList =
            DataManager.Instance.GetAllData.main_data.player_character_data_list;

        foreach (PlayerCharacterData pcd in pcdList)
        {
            GameObject characterIcon =
                Instantiate(icon_prefab, scrollview_contents.transform, false);

            IconButton iconButton =
                characterIcon.GetComponent<IconButton>();

            Color normalColor =
                pcd.player_character_info.icon_background_color;

            iconButton.SetColor(normalColor);

            iconButton.SetImage(
                pcd.player_character_info.character_icon);

            PlayerCharacterData capturedPCD = pcd;

            iconButton.ActionAdd(() =>
            {
                // 이전 선택 캐릭터 색 복구
                if (selectedCharacterIcon != null)
                {
                    selectedCharacterIcon.SetColor(
                        selectedCharacterNormalColor
                    );
                }

                // 현재 선택 캐릭터 저장
                selectedCharacterIcon = iconButton;

                selectedCharacterNormalColor = normalColor;

                // 빨간색 표시
                selectedCharacterIcon.SetColor(
                    selectedColor
                );

                ShowCharacterDeck(capturedPCD);
            });
        }
    }


    // 선택한 캐릭터 덱 표시
    public void ShowCharacterDeck(PlayerCharacterData pcd)
    {
        currentPCD = pcd;

        foreach (CardData card in pcd.player_character_deck)
        {
            Debug.Log(
                card.name + " / " + card.GetInstanceID()
            );
        }

        show_and_hide.SetActive(true);

        // 플레이어 덱
        player_deck_cards.RemoveAllCards();

        player_deck_cards.ShowCards(
            pcd.player_character_deck,
            deckInteractionPlayer
        );

        // 보관함
        storage_deck_cards.RemoveAllCards();

        storage_deck_cards.ShowCards(
            DataManager.Instance.GetAllData.main_data.storage_cards_list,
            deckInteractionStorage
        );
    }


    // 카드 이동
    public void MoveCard(int index, bool isPlayer)
    {
        if (currentPCD == null)
            return;

        List<CardData> playerDeck =
            currentPCD.player_character_deck;

        List<CardData> storageDeck =
            DataManager.Instance.GetAllData.main_data.storage_cards_list;

        if (isPlayer)
        {
            // 캐릭터 덱 → 보관함
            // 카드가 5장 이하라면 이동 불가
            if (playerDeck.Count <= 5)
            {
                if (!need_more_cards.activeSelf)
                {
                    need_more_cards.SetActive(true);
                }

                return;
            }

            if (index >= 0 && index < playerDeck.Count)
            {
                CardData card = playerDeck[index];

                playerDeck.RemoveAt(index);
                storageDeck.Add(card);
            }
        }
        else
        {
            // 보관함 → 캐릭터 덱
            if (index >= 0 && index < storageDeck.Count)
            {
                CardData card = storageDeck[index];

                storageDeck.RemoveAt(index);
                playerDeck.Add(card);
            }
        }

        ShowCharacterDeck(currentPCD);
        DataManager.Instance.SaveData();
    }
}