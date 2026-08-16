using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/HealSelf")]
public class HealSelfEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        caster.Heal(entry.value);
    }
}