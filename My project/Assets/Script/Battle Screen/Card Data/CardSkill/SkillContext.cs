using System.Collections.Generic;

public class SkillContext
{
    // 필요한 내용
    public CharacterVariable Caster;                        // 시전자
    public CharacterVariable Target;                        // 마우스로 직접 고른 타겟
    public List<CharacterVariable> RandomChoosedTargets;    // 무작위로 선발된 타겟들
    public SkillEntry Entry;                                // 스킬 엔트리

    // 초기화
    public SkillContext(
        CharacterVariable caster,
        CharacterVariable target,
        List<CharacterVariable> randomChoosedTargets,
        SkillEntry entry)
    {
        Caster = caster;
        Target = target;
        RandomChoosedTargets = randomChoosedTargets;
        Entry = entry;
    }
}