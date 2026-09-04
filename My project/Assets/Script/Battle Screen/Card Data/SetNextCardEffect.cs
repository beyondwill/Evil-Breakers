using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Set Next Card")]
public class SetNextCardEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        if (caster is not EnemyCharacterVariable enemy)
            return;

        CardData nextCard =
            entry.dataEntity as CardData;

        if (nextCard == null)
        {
            enemy.next_card = null;
            return;
        }

        enemy.next_card =
            new CardVariable(nextCard);
    }
}
