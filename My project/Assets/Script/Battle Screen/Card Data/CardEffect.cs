using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    public abstract void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card);
}