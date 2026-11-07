using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item Effect/Sight Up")]
public class SightUp : ItemEffect
{
    // 회복 정도
    [SerializeField] private int healValue;

    public override void Execute()
    {
        List<PlayerCharacterData> PCDList = DataManager.Instance.GetBattleData.characters_in_battle_data_list;

        foreach (PlayerCharacterData character in PCDList)
        {
            int maxHealth = (int)character.player_character_info.GetStatValue(CharacterBaseStatType.MaxHealth);

            character.current_health = Mathf.Min(
                character.current_health + healValue,
                maxHealth
            );
        }

        CharacterStateManager.Instance.UpdateCharacterInfo();
    }
}
