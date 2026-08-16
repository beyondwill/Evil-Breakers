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
                entry.value2 == 0
                    ? 1
                    : entry.value2;


            // ==========================================
            // 최종 데미지
            // ==========================================

            float damageAmount =
                Mathf.Max(
                    0,
                    entry.value
                    + attack
                    + (strength * strengthMultiplier)
                    - dexterity
                );


            // ==========================================
            // 로그
            // ==========================================

            Debug.Log(
                $"[Damage] " +
                $"{caster.character_info.character_name} → " +
                $"{target.character_info.character_name} | " +
                $"카드:{entry.value} + " +
                $"공격력:{attack} + " +
                $"힘:{strength * strengthMultiplier} - " +
                $"민첩:{dexterity} = " +
                $"{damageAmount}"
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