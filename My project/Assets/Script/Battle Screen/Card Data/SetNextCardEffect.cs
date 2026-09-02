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
        ((EnemyCharacterVariable)caster).next_card = new CardVariable((CardData)entry.dataEntity);
    }
}
