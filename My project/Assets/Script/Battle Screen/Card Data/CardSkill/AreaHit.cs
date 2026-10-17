using UnityEngine;

// 범위 타격
[CreateAssetMenu(menuName = "Skill/AreaHit")]
public class AreaHit : SkillType
{
    public SingleHit singleHit;

    public override void ExecuteEffects(SkillContext s)
    {
        // 각 캐릭터마다 타겟팅을 한다고 가정하고 공격
        foreach (CharacterVariable CV in s.RandomChoosedTargets)
        {
            s.Target = CV;
            singleHit.ExecuteEffects(s);
        }
    }
}