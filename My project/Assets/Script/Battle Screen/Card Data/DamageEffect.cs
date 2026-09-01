using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Damage")]
public class DamageEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        if (caster == null)
            return;

        foreach (CharacterVariable target in targets)
        {
            if (target == null)
                continue;


            // ==========================================
            // 캐릭터 기본 공격력
            // ==========================================

            float attack =
                caster.statContainer.GetBaseStat(
                    CharacterBaseStatType.Attack
                );


            // ==========================================
            // 힘 버프
            // ==========================================

            float strength =
                caster.statContainer.GetBuff(
                    CharacterBuffType.Strength
                );


            // ==========================================
            // 대상 민첩 버프
            // ==========================================

            float dexterity =
                target.statContainer.GetBuff(
                    CharacterBuffType.Dexterity
                );


            // ==========================================
            // 힘 버프 배율
            // ==========================================

            float strengthMultiplier =
                entry.floatValueList.Count == 0
                    ? 1
                    : entry.floatValueList[0];


            // ==========================================
            // 최종 데미지
            // ==========================================

            float multipleDamage = 1.0f;

            // 유리한 상성
            if (GameRuleManager.Instance.Rule.IsAdvantage(caster.character_info.element, target.character_info.element))
            {
                multipleDamage = GameRuleManager.Instance.Rule.AdvDmg;
            }

            // 불리한 상성
            else if (GameRuleManager.Instance.Rule.IsDisadvantage(caster.character_info.element, target.character_info.element))
            {
                multipleDamage = GameRuleManager.Instance.Rule.DadvDmg;
            }

            float toughnessDamage = 1.0f;

            // 상대에게 강인함 패시브가 있으면: 하나 깎고 경우에 따라 설정
            if (target.statContainer.GetBuff(CharacterBuffType.Toughness) > 0)
            {
                target.AddBuff(
                    CharacterBuffType.Toughness,
                    -1
                );

                if (target.character_info.element == Element.Fire)
                {
                    toughnessDamage = 0.2f;
                }
                else
                {
                    toughnessDamage = 0.5f;
                }
            }

            float damageAmount =
                    Mathf.Max(
                        0,
                        (entry.valueList[0] 
                        + attack
                        + (strength * strengthMultiplier)
                        - dexterity)
                        * multipleDamage
                        * toughnessDamage
                    );


            // ==========================================
            // 데미지 적용
            // ==========================================

            target.TakeDamage(
                damageAmount
            );
        }
    }
}