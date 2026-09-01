using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Change Character")]
public class ChangeCharacterEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        caster.character_info = (CharacterInfo)entry.dataEntity;
        int current_health = (int)caster.current_health;
        caster.InitializeStat(caster.character_info);
        caster.current_health = current_health;
        caster.characterView.CharacterInit(caster);
    }
}