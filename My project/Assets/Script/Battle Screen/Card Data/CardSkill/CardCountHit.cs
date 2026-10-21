using UnityEngine;

// 단일 타격
[CreateAssetMenu(menuName = "Skill/CardCountHit")]
public class CardCountHit : SkillType
{
    public override void ExecuteEffects(SkillContext s)
    {
        int handCount = CardManager.Instance.GetHandCount();
    }
}