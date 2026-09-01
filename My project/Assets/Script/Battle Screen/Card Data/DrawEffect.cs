using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Draw")]
public class DrawEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {

        for (int i = 0; i < entry.valueList[0]; i++)
        {
            CardManager.Instance.DrawCard();
        }
    }
}