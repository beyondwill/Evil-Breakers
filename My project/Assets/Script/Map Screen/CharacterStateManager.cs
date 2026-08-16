using System.Collections.Generic;
using UnityEngine;

public class CharacterStateManager : MonoBehaviour
{
    public static CharacterStateManager Instance { get; private set; }

    [SerializeField] private CharacterSlot characterSlotPrefab;
    [SerializeField] private Transform characterParent;

    private List<CharacterSlot> characterSlots = new();

    private DataManager dataManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dataManager = DataManager.Instance;
        ShowAllCharacterInfo();
    }

    // 모든 캐릭터 생성
    public void ShowAllCharacterInfo()
    {
        int playerCount =
            dataManager.GetBattleData.characters_in_battle_data_list.Count;

        for (int i = 0; i < playerCount; i++)
        {
            var character =
                dataManager.GetBattleData.characters_in_battle_data_list[i];

            int maxHp =
                (int)character.player_character_info.GetStatValue(
                    CharacterBaseStatType.MaxHealth);

            int currentHp =
                (int)character.current_health;

            CharacterSlot slot =
                Instantiate(characterSlotPrefab, characterParent);

            slot.SetCharacter(
                character,
                character.player_character_info,
                character.player_character_info.character_icon,
                currentHp,
                maxHp);

            if (character.current_health == 0)
            {
                slot.IsDead();
            }

            characterSlots.Add(slot);
        }
    }

    // 체력만 갱신
    public void UpdateCharacterInfo()
    {
        int playerCount =
            dataManager.GetBattleData.characters_in_battle_data_list.Count;

        for (int i = 0; i < playerCount; i++)
        {
            var character =
                dataManager.GetBattleData.characters_in_battle_data_list[i];

            int maxHp =
                (int)character.player_character_info.GetStatValue(
                    CharacterBaseStatType.MaxHealth);

            int currentHp =
                (int)character.current_health;

            characterSlots[i].UpdateHealth(
                currentHp,
                maxHp);
        }
    }

    // CharacterInfo에 해당하는 슬롯 가져오기
    public CharacterSlot GetCharacterSlot(CharacterInfo characterInfo)
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            if (characterSlots[i].GetCharacterInfo() == characterInfo)
            {
                return characterSlots[i];
            }
        }

        return null;
    }
}