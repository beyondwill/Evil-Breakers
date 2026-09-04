using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TargetDebuffCondition",
    menuName = "Card/Conditions/TargetHasDebuff"
)]
public class TargetHasDebuff : CardCondition
{
    public override bool Check(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardData card)
    {
        // 대상이 없으면 조건 실패
        if (targets == null || targets.Count == 0)
            return false;


        // 첫 번째 대상만 검사
        CharacterVariable target = targets[0];

        if (target == null)
            return false;


        // 대상의 버프 목록 검사
        foreach (CharacterBuffValue buff in target.statContainer.buffList)
        {
            // 음수 버프 = 디버프
            if (buff.value < 0)
                return true;
        }


        return false;
    }
}