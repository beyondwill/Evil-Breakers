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
        if (entry.valueList.Count > 0) caster.Heal(entry.valueList[0]);
        if (entry.floatValueList.Count > 0) caster.Heal(entry.floatValueList[0] * caster.max_health);
    }
}