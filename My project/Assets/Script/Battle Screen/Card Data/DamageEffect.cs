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


        Debug.Log(
        $"[DamageEffect] Target Count = {targets.Count}"
        );

        if (caster == null)
            return;

        if (targets == null)
            return;

        if (entry == null)
            return;

        if (entry.valueList == null ||
            entry.valueList.Count == 0)
        {
            return;
        }


        foreach (CharacterVariable target in targets)
        {
            if (target == null)
                continue;


            // ==========================================
            // 데미지 기본값
            // ==========================================

            int damageValueIndex = 0;


            // ==========================================
            // 특수 조건 확인
            // ==========================================
            //
            // 예:
            // 대상이 약화 상태라면
            // valueList[1] 사용
            //
            // 그렇지 않으면
            // valueList[0] 사용
            // ==========================================

            if (card != null &&
                card.specialcardCondition != null)
            {
                List<CharacterVariable> conditionTargets =
                    new List<CharacterVariable>
                    {
                        target
                    };


                bool specialCondition =
                    card.specialcardCondition.Check(
                        caster,
                        conditionTargets,
                        card
                    );


                if (specialCondition)
                {
                    // valueList[1]이 존재할 때만 사용
                    if (entry.valueList.Count > 1)
                    {
                        damageValueIndex = 1;
                    }
                }
            }


            // ==========================================
            // 실제 카드 피해량
            // ==========================================

            float cardDamage =
                entry.valueList[damageValueIndex];


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
            // 상성 배율
            // ==========================================

            float multipleDamage = 1.0f;


            // 유리한 상성
            if (GameRuleManager.Instance.Rule.IsAdvantage(
                caster.character_info.element,
                target.character_info.element))
            {
                multipleDamage =
                    GameRuleManager.Instance.Rule.AdvDmg;
            }


            // 불리한 상성
            else if (GameRuleManager.Instance.Rule.IsDisadvantage(
                caster.character_info.element,
                target.character_info.element))
            {
                multipleDamage =
                    GameRuleManager.Instance.Rule.DadvDmg;
            }


            // ==========================================
            // 강인함
            // ==========================================

            float toughnessDamage = 1.0f;


            if (target.statContainer.GetBuff(
                CharacterBuffType.Toughness) > 0)
            {
                target.AddBuff(
                    CharacterBuffType.Toughness,
                    -1
                );


                if (target.character_info.element ==
                    Element.Fire)
                {
                    toughnessDamage = 0.2f;
                }
                else
                {
                    toughnessDamage = 0.5f;
                }
            }


            // ==========================================
            // 최종 데미지
            // ==========================================

            float damageAmount =
                Mathf.Max(
                    0,
                    (
                        cardDamage
                        + attack
                        + (strength * strengthMultiplier)
                        - dexterity
                    )
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