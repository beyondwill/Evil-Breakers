using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Reduce Time")]
public class ReduceTimeEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        DataManager.Instance.GetBattleData.ReduceTime(entry.value);
    }
}