using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EventEffect/Deal Character")]
public class EffectDealCharacter : EventEffect
{
    public int amount;

    public override void Execute()
    {
        List<PlayerCharacterData> PCDList =
            DataManager.Instance.GetBattleData.characters_in_battle_data_list;

        foreach (PlayerCharacterData pcd in PCDList)
        {
            if (pcd == null)
                continue;

            // 현재 생명력 감소
            pcd.current_health -= amount;

            // 최소 1 / 최대 생명력 제한
            int maxHealth =
                (int)pcd.player_character_info.GetStatValue(
                    CharacterBaseStatType.MaxHealth,
                    pcd.player_character_level
                );

            pcd.current_health =
                Mathf.Clamp(
                    pcd.current_health,
                    1,
                    maxHealth
                );
        }

        // UI 갱신
        CharacterStateManager.Instance?.UpdateCharacterInfo();
    }
}