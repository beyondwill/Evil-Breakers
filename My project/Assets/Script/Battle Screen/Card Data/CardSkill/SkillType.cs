using System.Collections.Generic;
using UnityEngine;

public abstract class SkillType : ScriptableObject
{
    // 카드의 효과를 실행하는 함수
    public abstract void ExecuteEffects(SkillContext s);
}