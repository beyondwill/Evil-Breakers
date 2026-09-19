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
        // ==========================================
        // 기본 참조 확인
        // ==========================================

        if (caster == null)
        {
            Debug.LogError("[DamageEffect] caster is NULL");
            return;
        }

        if (targets == null)
        {
            Debug.LogError("[DamageEffect] targets is NULL");
            return;
        }

        if (entry == null)
        {
            Debug.LogError("[DamageEffect] entry is NULL");
            return;
        }

        if (entry.valueList == null ||
            entry.valueList.Count == 0)
        {
            Debug.LogError(
                "[DamageEffect] entry.valueList is NULL or EMPTY");

            return;
        }


        // ==========================================
        // 공격 애니메이션
        // ==========================================

        if (caster.characterView != null)
        {
            caster.characterView.PlayAttackAnimation();
        }
        else
        {
            Debug.LogWarning(
                "[DamageEffect] caster.characterView is NULL");
        }


        // ==========================================
        // 디버그
        // ==========================================

        Debug.Log(
            $"[DamageEffect] " +
            $"caster={caster}, " +
            $"caster.statContainer={caster.statContainer}, " +
            $"caster.character_info={caster.character_info}, " +
            $"entry={entry}, " +
            $"entry.floatValueList={entry.floatValueList}, " +
            $"GameRuleManager={GameRuleManager.Instance}"
        );

        Debug.Log(
            $"[DamageEffect] Target Count = {targets.Count}"
        );


        foreach (CharacterVariable target in targets)
        {
            if (target == null)
            {
                Debug.LogWarning(
                    "[DamageEffect] target is NULL");

                continue;
            }


            // ==========================================
            // Target 참조 확인
            // ==========================================

            Debug.Log(
                $"[DamageEffect] " +
                $"target={target}, " +
                $"target.statContainer={target.statContainer}, " +
                $"target.character_info={target.character_info}"
            );


            if (caster.statContainer == null)
            {
                Debug.LogError(
                    "[DamageEffect] caster.statContainer is NULL");

                return;
            }


            if (target.statContainer == null)
            {
                Debug.LogError(
                    "[DamageEffect] target.statContainer is NULL");

                continue;
            }


            if (caster.character_info == null)
            {
                Debug.LogError(
                    "[DamageEffect] caster.character_info is NULL");

                return;
            }


            if (target.character_info == null)
            {
                Debug.LogError(
                    "[DamageEffect] target.character_info is NULL");

                continue;
            }


            // ==========================================
            // 데미지 기본값
            // ==========================================

            int damageValueIndex = 0;


            // ==========================================
            // 특수 조건 확인
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

            float strengthMultiplier = 1f;

            if (entry.floatValueList != null &&
                entry.floatValueList.Count > 0)
            {
                strengthMultiplier =
                    entry.floatValueList[0];
            }


            // ==========================================
            // 상성 배율
            // ==========================================

            float multipleDamage = 1.0f;


            if (GameRuleManager.Instance == null)
            {
                Debug.LogError(
                    "[DamageEffect] GameRuleManager.Instance is NULL");

                return;
            }


            if (GameRuleManager.Instance.Rule == null)
            {
                Debug.LogError(
                    "[DamageEffect] GameRuleManager.Instance.Rule is NULL");

                return;
            }


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
                    Element.PHYSIC)
                {
                    toughnessDamage = 0f;
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