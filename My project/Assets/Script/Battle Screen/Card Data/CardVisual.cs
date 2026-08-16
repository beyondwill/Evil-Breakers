using System.Collections.Generic;
using UnityEngine;

public abstract class CardVisual : ScriptableObject
{
    public abstract void Play(
        CharacterVariable caster,
        List<CharacterVariable> targets);
}