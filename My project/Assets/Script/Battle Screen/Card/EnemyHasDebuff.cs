using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyDebuffCondition",
    menuName = "Card/Conditions/EnemyHasDebuff"
)]
public class EnemyHasDebuff : CardCondition
{
    public override bool Check(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardData card)
    {
        if (caster == null)
            return false;

        if (BattleCharacterManager.Instance == null)
            return false;


        List<CharacterVariable> enemies;

        if (caster.is_player_character)
        {
            enemies =
                BattleCharacterManager.Instance.EnemyCharacters;
        }
        else
        {
            enemies =
                BattleCharacterManager.Instance.PlayerCharacters;
        }


        if (enemies == null)
            return false;


        foreach (CharacterVariable enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (enemy.statContainer == null)
                continue;


            foreach (
                CharacterBuffValue buff
                in enemy.statContainer.buffList)
            {
                Debug.Log(
                    "[EnemyHasDebuff] " +
                    enemy.character_info.character_name +
                    " / Buff Value = " +
                    buff.value
                );


                if (buff.value < 0)
                {
                    Debug.Log(
                        "[EnemyHasDebuff] 디버프 발견!"
                    );

                    return true;
                }
            }
        }


        Debug.Log(
            "[EnemyHasDebuff] 디버프 없음"
        );

        return false;
    }
}