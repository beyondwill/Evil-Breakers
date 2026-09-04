using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Get Passive Effect")]
public class GetPassiveEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        caster.passiveSkillList.Add((PassiveSkillData)entry.dataEntity);
    }
}
