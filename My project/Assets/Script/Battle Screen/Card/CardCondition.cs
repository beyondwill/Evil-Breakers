using System.Collections.Generic;
using UnityEngine;

// 카드 조건
public abstract class CardCondition : ScriptableObject
{
    public abstract bool Check(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardData card);
}