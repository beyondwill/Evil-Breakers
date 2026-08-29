using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Rule")]
public class GameRuleSO : ScriptableObject
{
    public float AdvDmg = 1.2f;
    public float DadvDmg = 0.9f;
    public List<int> expPerLevelList;           // 레벨 별 exp
    public List<string> NamePerLevelList;       // 레벨 별 호칭

    // 유리한 상성인가?
    public bool IsAdvantage(Element caster, Element target)
    {
        int c = (int)caster;
        int t = (int)target;

        int elementSize = Enum.GetValues(typeof(Element)).Length;

        if (c == 0)
            return false;

        if ((c == 1 && t == 2) ||
            (c == 2 && t == 1))
            return true;

        int next = c + 1;

        if (next >= elementSize)
            next = 3;

        return t == next;
    }

    public bool IsDisadvantage(Element caster, Element target)
    {
        int c = (int)caster;
        int t = (int)target;

        int elementSize =
            Enum.GetValues(typeof(Element)).Length;

        // 상성 자체가 없으면 false
        if (c == 0)
            return false;

        // 크로스 카운터는 양쪽 모두 유리하므로
        // 불리한 상성이 아님
        if ((c == 1 && t == 2) ||
            (c == 2 && t == 1))
            return false;

        // 이전 원소에게 불리
        int previous = c - 1;

        if (previous < 3)
            previous = elementSize - 1;

        return t == previous;
    }
}