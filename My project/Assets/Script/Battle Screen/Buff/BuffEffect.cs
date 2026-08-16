using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Buff")]
public class BuffEffect : CardEffect
{
    [SerializeField]
    private CharacterBuffType buffType;

    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        foreach (CharacterVariable target in targets)
        {
            if (target == null)
                continue;

            if (card == null)
                continue;

            target.AddBuff(
                buffType,
                entry.value
            );
        }
    }
}