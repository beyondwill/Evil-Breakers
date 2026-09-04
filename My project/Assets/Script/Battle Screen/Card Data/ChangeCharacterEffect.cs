using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/Change Character")]
public class ChangeCharacterEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        if (caster == null)
            return;

        CharacterInfo newCharacterInfo =
            (CharacterInfo)entry.dataEntity;

        if (newCharacterInfo == null)
        {
            Debug.LogWarning(
                "[ChangeCharacterEffect] 변경할 CharacterInfo가 없습니다."
            );

            return;
        }


        // =====================================================
        // 기존 현재 HP 저장
        // =====================================================

        int currentHealth =
            (int)caster.current_health;


        // =====================================================
        // 캐릭터 정보 변경
        // =====================================================

        caster.character_info =
            newCharacterInfo;


        // =====================================================
        // 기존 패시브 제거
        // =====================================================

        caster.passiveSkillList.Clear();


        // =====================================================
        // 스탯 초기화
        //
        // 기존 버프는 유지된다.
        // InitializeStat()은 baseStatList만 초기화하고
        // buffList는 건드리지 않기 때문.
        // =====================================================

        caster.InitializeStat(
            newCharacterInfo
        );


        // =====================================================
        // 기존 HP 유지
        // =====================================================

        caster.current_health =
            Mathf.Min(
                currentHealth,
                caster.max_health
            );


        // =====================================================
        // 새로운 패시브 적용
        // =====================================================

        if (newCharacterInfo.passiveSkillList != null)
        {
            caster.passiveSkillList =
                new List<PassiveSkillData>(
                    newCharacterInfo.passiveSkillList
                );
        }


        // =====================================================
        // Enemy 전용 처리
        // =====================================================

        if (caster is EnemyCharacterVariable enemy)
        {
            // -------------------------------------------------
            // 이전 적의 다음 카드 제거
            // -------------------------------------------------

            enemy.next_card = null;


            // -------------------------------------------------
            // 이전 적의 카드 목록 제거
            // -------------------------------------------------

            enemy.enemy_card_list.Clear();


            // -------------------------------------------------
            // 새로운 적의 카드 목록 적용
            // -------------------------------------------------

            if (newCharacterInfo is EnemyCharacterInfo newEnemyInfo)
            {
                enemy.enemy_character_info =
                    newEnemyInfo;


                if (newEnemyInfo.enemy_card_info_list != null)
                {
                    foreach (CardData newCard
                             in newEnemyInfo.enemy_card_info_list)
                    {
                        if (newCard == null)
                        {
                            Debug.LogWarning(
                                "[ChangeCharacterEffect] " +
                                "새 적의 카드 목록에 NULL 카드가 있습니다."
                            );

                            continue;
                        }


                        enemy.enemy_card_list.Add(
                            new CardVariable(newCard)
                        );
                    }
                }


                // -------------------------------------------------
                // 새 적의 시작 버프 적용
                // -------------------------------------------------

                if (newEnemyInfo.initBuffList != null)
                {
                    foreach (CharacterBuffValue buff
                             in newEnemyInfo.initBuffList)
                    {
                        if (buff == null)
                            continue;


                        enemy.AddBuff(
                            buff.type,
                            buff.value
                        );
                    }
                }
            }


            // -------------------------------------------------
            // 타겟도 초기화
            // -------------------------------------------------

            enemy.target_index = -1;
        }


        // =====================================================
        // UI 갱신
        // =====================================================

        caster.characterView?.CharacterInit(
            caster
        );
    }
}