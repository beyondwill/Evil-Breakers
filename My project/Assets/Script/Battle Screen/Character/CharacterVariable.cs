using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterVariable
{
    // 변수
    public CharacterInfo character_info;                        // 캐릭터 원본 데이터
    public CharacterView characterView;                         // 캐릭터 뷰
    public bool is_player_character = true;                     // 플레이어 캐릭터인가?
    public int character_location_index = -1;                   // 캐릭터 위치 인덱스
    public bool is_dead = false;                                // 캐릭터 사망 여부

    // 스탯, 패시브
    public CharacterStatContainer statContainer = new();

    public List<PassiveSkillData> passiveSkillList = new();

    // 구독 이벤트
    public event Action<int, int> OnHealthChanged;

    public event Action<int, int> OnEnergyChanged;

    public event Action OnDeath;

    public event Action<List<CharacterBuffValue>> OnBuffChanged;

    // 현재 생명력
    public float current_health
    {
        get
        {
            return statContainer.GetRuntimeStat(
                CharacterRuntimeStatType.CurrentHealth
            );
        }

        set
        {
            CharacterRuntimeStatValue stat =
                statContainer.runtimeStatList.Find(
                    x =>
                        x.type ==
                        CharacterRuntimeStatType.CurrentHealth
                );


            if (stat == null)
            {
                statContainer.runtimeStatList.Add(
                    new CharacterRuntimeStatValue
                    {
                        type =
                            CharacterRuntimeStatType.CurrentHealth,

                        value = value
                    }
                );
            }
            else
            {
                stat.value = value;
            }


            OnHealthChanged?.Invoke(
                (int)current_health,
                (int)max_health
            );
        }
    }

    // 현재 에너지
    public float current_energy
    {
        get
        {
            return statContainer.GetRuntimeStat(
                CharacterRuntimeStatType.CurrentEnergy
            );
        }

        set
        {
            CharacterRuntimeStatValue stat =
                statContainer.runtimeStatList.Find(
                    x =>
                        x.type ==
                        CharacterRuntimeStatType.CurrentEnergy
                );


            if (stat == null)
            {
                statContainer.runtimeStatList.Add(
                    new CharacterRuntimeStatValue
                    {
                        type =
                            CharacterRuntimeStatType.CurrentEnergy,

                        value = value
                    }
                );
            }
            else
            {
                stat.value = value;
            }


            OnEnergyChanged?.Invoke(
                (int)current_energy,
                (int)MaxEnergy
            );
        }
    }

    // 최대 에너지
    public float MaxEnergy =>
        statContainer.GetBaseStat(
            CharacterBaseStatType.MaxEnergy
        );

    // 최대 생명력
    public float max_health =>
        statContainer.GetBaseStat(
            CharacterBaseStatType.MaxHealth
        );


    // 공격 순서 구하기: 원본 공격 순서 + 가속력
    public float AttackOrder =>
        statContainer.GetBaseStat(CharacterBaseStatType.AttackOrder) + statContainer.GetBuff(CharacterBuffType.Acceleration);

    // 스탯 초기화
    public void InitializeStat(
        CharacterInfo info,
        int level = 0)
    {
        if (info == null)
            return;


        if (statContainer.baseStatList == null)
        {
            statContainer.baseStatList =
                new List<CharacterBaseStatValue>();
        }
        else
        {
            statContainer.baseStatList.Clear();
        }


        CharacterLevelStat levelStat =
            info.levelStatList.Find(
                x =>
                    x != null &&
                    x.level == level
            );


        if (levelStat != null &&
            levelStat.statList != null)
        {
            foreach (CharacterBaseStatValue stat
                     in levelStat.statList)
            {
                if (stat == null)
                    continue;


                statContainer.baseStatList.Add(
                    new CharacterBaseStatValue
                    {
                        type = stat.type,
                        value = stat.value
                    }
                );
            }
        }

        current_health = max_health;
        current_energy = 0;
    }

    public void TriggerPassive(
        PassiveTriggerType triggerType,
        float triggerValue = 0)
    {
        Debug.Log(
            $"[Passive Trigger] " +
            $"{character_info?.character_name} / " +
            $"Trigger={triggerType} / " +
            $"TriggerValue={triggerValue} / " +
            $"HP={current_health} / " +
            $"PassiveCount={passiveSkillList?.Count ?? 0}"
        );


        if (passiveSkillList == null ||
            passiveSkillList.Count == 0)
        {
            return;
        }


        // =================================================
        // 발동할 패시브를 먼저 수집
        // =================================================
        //
        // foreach 도중 passiveSkillList를 수정하면
        // Collection was modified 오류가 발생할 수 있으므로
        // 실제 리스트 제거는 foreach가 끝난 뒤 처리한다.
        // =================================================

        List<PassiveSkillData> triggeredPassives =
            new List<PassiveSkillData>();


        foreach (PassiveSkillData passive
                 in passiveSkillList)
        {
            if (passive == null)
                continue;


            // ---------------------------------------------
            // Trigger 검사
            // ---------------------------------------------

            Debug.Log(
                $"[Passive 검사] " +
                $"{passive.skill_name} / " +
                $"PassiveTrigger={passive.triggerType} / " +
                $"CurrentTrigger={triggerType}"
            );


            if (passive.triggerType != triggerType)
                continue;


            Debug.Log(
                $"[Passive Trigger 일치] " +
                $"{passive.skill_name}"
            );


            // ---------------------------------------------
            // Condition 검사
            // ---------------------------------------------

            bool conditionResult =
                CheckPassiveConditions(
                    passive,
                    triggerValue
                );


            if (!conditionResult)
            {
                Debug.Log(
                    $"[Passive 조건 실패] " +
                    $"{passive.skill_name}"
                );

                continue;
            }


            Debug.Log(
                $"[Passive 발동] " +
                $"{passive.skill_name}"
            );


            triggeredPassives.Add(passive);
        }


        // =================================================
        // 발동된 패시브 실행
        // =================================================

        foreach (PassiveSkillData passive
                 in triggeredPassives)
        {
            if (passive == null)
                continue;


            ExecutePassive(passive);
        }


        // =================================================
        // 일회성 패시브 제거
        // =================================================

        foreach (PassiveSkillData passive
                 in triggeredPassives)
        {
            if (passive == null)
                continue;


            if (!passive.isOneTime)
                continue;


            if (passiveSkillList.Remove(passive))
            {
                Debug.Log(
                    $"[Passive 일회성 제거] " +
                    $"{passive.skill_name}"
                );
            }
        }
    }


    // =====================================================
    // Passive Condition 검사
    // =====================================================

    private bool CheckPassiveConditions(
        PassiveSkillData passive,
        float triggerValue)
    {
        if (passive == null)
            return false;


        if (passive.conditionList == null ||
            passive.conditionList.Count == 0)
        {
            return true;
        }


        foreach (PassiveCondition condition
                 in passive.conditionList)
        {
            if (condition == null)
                continue;


            float currentValue =
                GetPassiveConditionValue(
                    condition.conditionType,
                    triggerValue
                );


            bool result =
                CheckPassiveCondition(
                    currentValue,
                    condition.compareType,
                    condition.value
                );


            Debug.Log(
                $"[Passive Condition] " +
                $"{passive.skill_name} / " +
                $"Type={condition.conditionType} / " +
                $"Current={currentValue} / " +
                $"Compare={condition.compareType} / " +
                $"Value={condition.value} / " +
                $"Result={result}"
            );


            if (!result)
                return false;
        }


        return true;
    }


    // =====================================================
    // Passive Condition 값 가져오기
    // =====================================================

    private float GetPassiveConditionValue(
        PassiveConditionType conditionType,
        float triggerValue)
    {
        switch (conditionType)
        {
            // ---------------------------------------------
            // HP
            // ---------------------------------------------

            case PassiveConditionType.HP:

                return current_health;


            // ---------------------------------------------
            // Turn
            // ---------------------------------------------

            case PassiveConditionType.Turn:

                if (TurnManager.Instance == null)
                    return 0;


                // 현재 TurnManager에
                // CurrentTurn 값이 구현되어 있다면
                // 해당 값을 연결하면 됨.
                return 0;


            // ---------------------------------------------
            // Round
            // ---------------------------------------------

            case PassiveConditionType.Round:

                if (TurnManager.Instance == null)
                    return 0;


                return TurnManager.Instance.CurrentRound;


            // ---------------------------------------------
            // Damage Taken
            // ---------------------------------------------

            case PassiveConditionType.DamageTaken:

                return triggerValue;


            // ---------------------------------------------
            // Mana Spent
            // ---------------------------------------------

            case PassiveConditionType.ManaSpent:

                return triggerValue;


            // ---------------------------------------------
            // Cards Played
            // ---------------------------------------------

            case PassiveConditionType.CardsPlayedThisTurn:

                return triggerValue;
        }


        return 0;
    }


    // =====================================================
    // Passive Condition 비교
    // =====================================================

    private bool CheckPassiveCondition(
        float currentValue,
        PassiveCompareType compareType,
        float conditionValue)
    {
        switch (compareType)
        {
            case PassiveCompareType.Equal:

                return Mathf.Approximately(
                    currentValue,
                    conditionValue
                );


            case PassiveCompareType.Greater:

                return currentValue >
                       conditionValue;


            case PassiveCompareType.GreaterEqual:

                return currentValue >=
                       conditionValue;


            case PassiveCompareType.Less:

                return currentValue <
                       conditionValue;


            case PassiveCompareType.LessEqual:

                return currentValue <=
                       conditionValue;


            default:

                return false;
        }
    }


    // =====================================================
    // Passive 실행
    // =====================================================

    private void ExecutePassive(
        PassiveSkillData passive)
    {
        if (passive == null)
            return;


        if (passive.effects == null ||
            passive.effects.Count == 0)
        {
            Debug.Log(
                $"[Passive Effect 없음] " +
                $"{passive.skill_name}"
            );

            return;
        }


        Debug.Log(
            $"[Passive Effect 실행] " +
            $"{passive.skill_name} / " +
            $"EffectCount={passive.effects.Count}"
        );


        foreach (CardEffectEntry effectEntry
                 in passive.effects)
        {
            if (effectEntry == null)
                continue;


            ExecutePassiveEffect(effectEntry);
        }
    }


    // =====================================================
    // Passive Effect 실행
    // =====================================================

    private void ExecutePassiveEffect(
        CardEffectEntry effectEntry)
    {
        if (effectEntry == null)
            return;


        if (effectEntry.effect == null)
        {
            Debug.LogWarning(
                $"[Passive Effect 실패] " +
                $"Effect가 설정되지 않음"
            );

            return;
        }


        // ---------------------------------------------
        // 기본적으로 자기 자신을 대상으로 함
        // ---------------------------------------------

        List<CharacterVariable> targets =
            new List<CharacterVariable>();


        targets.Add(this);


        Debug.Log(
            $"[Passive Effect Execute] " +
            $"Target={character_info?.character_name}"
        );


        // ---------------------------------------------
        // 기존 CardEffect 시스템 사용
        // ---------------------------------------------

        effectEntry.effect.Execute(
            this,
            targets,
            effectEntry,
            null
        );
    }


    // =====================================================
    // 버프 추가
    // =====================================================

    public void AddBuff(
        CharacterBuffType type,
        float value)
    {
        Debug.Log(
            $"[CharacterVariable] AddBuff : " +
            $"{type} / {value}"
        );


        statContainer.AddBuff(
            type,
            value
        );


        CharacterBuffValue buff =
            statContainer.buffList.Find(
                x => x.type == type
            );


        if (buff != null &&
            Mathf.Approximately(buff.value, 0))
        {
            statContainer.buffList.Remove(buff);


            Debug.Log(
                $"[CharacterVariable] 버프 제거 : " +
                $"{type}"
            );
        }


        OnBuffChanged?.Invoke(
            statContainer.buffList
        );
    }


    // =====================================================
    // 버프 제거
    // =====================================================

    public void RemoveBuff(
        CharacterBuffType type)
    {
        CharacterBuffValue buff =
            statContainer.buffList.Find(
                x => x.type == type
            );


        if (buff == null)
            return;


        statContainer.buffList.Remove(buff);


        Debug.Log(
            $"[CharacterVariable] 버프 제거 : " +
            $"{type}"
        );


        OnBuffChanged?.Invoke(
            statContainer.buffList
        );
    }


    // =====================================================
    // 피해
    // =====================================================

    public virtual void TakeDamage(
        float damage)
    {
        if (is_dead)
            return;


        if (damage < 0)
            damage = 0;


        // ---------------------------------------------
        // 피해 적용
        // ---------------------------------------------

        current_health =
            Mathf.Max(
                0,
                current_health - damage
            );


        characterView?.TakeDamage(
            (int)damage
        );


        // ---------------------------------------------
        // Damaged Passive
        // ---------------------------------------------

        TriggerPassive(
            PassiveTriggerType.Damaged,
            damage
        );


        // ---------------------------------------------
        // 사망
        // ---------------------------------------------

        if (current_health <= 0)
        {
            Die();
        }


        // ---------------------------------------------
        // 아군 피격 시 공포 증가
        // ---------------------------------------------

        if (this is PlayerCharacterVariable)
        {
            DataManager.Instance
                .GetBattleData
                .AddHorror(3);
        }
    }


    // =====================================================
    // 회복
    // =====================================================

    public virtual void Heal(
        float amount)
    {
        if (is_dead)
            return;


        if (amount <= 0)
            return;


        current_health =
            Mathf.Min(
                max_health,
                current_health + amount
            );
    }


    // =====================================================
    // 사망
    // =====================================================

    protected virtual void Die()
    {
        if (is_dead)
            return;


        is_dead = true;


        // ---------------------------------------------
        // Death Passive
        // ---------------------------------------------

        TriggerPassive(
            PassiveTriggerType.Death
        );


        // ---------------------------------------------
        // Death Event
        // ---------------------------------------------

        OnDeath?.Invoke();


        // ---------------------------------------------
        // 현재 턴 종료
        // ---------------------------------------------

        if (TurnManager.Instance != null &&
            TurnManager.Instance.CurrentCharacter == this)
        {
            TurnManager.Instance.EndCurrentTurn();
        }


        // ---------------------------------------------
        // 적 사망 시 공포 감소
        // ---------------------------------------------

        if (this is EnemyCharacterVariable)
        {
            DataManager.Instance
                .GetBattleData
                .AddHorror(-10);
        }
    }
}


// =========================================================
// Player Character
// =========================================================

[System.Serializable]
public class PlayerCharacterVariable
    : CharacterVariable
{
    public PlayerCharacterInfo player_character_info;

    public PlayerCharacterData originalPlayerCharacterData;


    public List<CardVariable> hand_card_list =
        new();

    public List<CardVariable> graveyard_card_list =
        new();

    public List<CardVariable> deck_card_list =
        new();


    public int stress_count = 0;


    // =====================================================
    // Constructor
    // =====================================================

    public PlayerCharacterVariable(
        PlayerCharacterInfo PCI,
        int index,
        PlayerCharacterData data)
    {
        is_player_character = true;

        character_location_index = index;

        character_info = PCI;

        player_character_info = PCI;

        originalPlayerCharacterData = data;


        // ---------------------------------------------
        // 스탯
        // ---------------------------------------------

        InitializeStat(
            PCI,
            data != null
                ? data.player_character_level
                : 0
        );


        // ---------------------------------------------
        // 장비
        // ---------------------------------------------

        ApplyEquipmentStats(data);


        // ---------------------------------------------
        // 패시브
        // ---------------------------------------------

        if (PCI != null &&
            PCI.passiveSkillList != null)
        {
            passiveSkillList =
                new List<PassiveSkillData>(
                    PCI.passiveSkillList
                );
        }
    }


    // =====================================================
    // 장비 스탯
    // =====================================================

    private void ApplyEquipmentStats(
        PlayerCharacterData data)
    {
        if (data == null)
            return;


        if (data.player_equipment_list == null)
            return;


        foreach (EquipmentSlot slot
                 in data.player_equipment_list)
        {
            if (slot == null)
                continue;


            if (slot.equipment_info == null)
                continue;


            EquipmentInfo equipment =
                slot.equipment_info;


            if (equipment.baseStatList == null)
                continue;


            statContainer.MergeBaseStatList(
                equipment.baseStatList
            );


            Debug.Log(
                $"장비 스탯 적용 : " +
                $"{equipment.itemName}"
            );
        }
    }
}


// =========================================================
// Enemy Character
// =========================================================

[System.Serializable]
public class EnemyCharacterVariable
    : CharacterVariable
{
    public EnemyCharacterInfo enemy_character_info;

    public List<CardVariable> enemy_card_list =
        new();

    public CardVariable next_card;

    public int target_index = -1;


    // =====================================================
    // Constructor
    // =====================================================

    public EnemyCharacterVariable(
        EnemyCharacterInfo ECI,
        int index)
    {
        is_player_character = false;

        character_location_index = index;

        character_info = ECI;

        enemy_character_info = ECI;


        // ---------------------------------------------
        // 스탯
        // ---------------------------------------------

        InitializeStat(ECI);


        // ---------------------------------------------
        // 패시브
        // ---------------------------------------------

        if (ECI != null &&
            ECI.passiveSkillList != null)
        {
            passiveSkillList =
                new List<PassiveSkillData>(
                    ECI.passiveSkillList
                );
        }


        // ---------------------------------------------
        // 적 카드
        // ---------------------------------------------

        if (ECI.enemy_card_info_list != null)
        {
            foreach (CardData card
                     in ECI.enemy_card_info_list)
            {
                if (card == null)
                {
                    Debug.LogError(
                        "적 카드 목록에 NULL 카드 존재 : " +
                        ECI.character_name
                    );

                    continue;
                }


                enemy_card_list.Add(
                    new CardVariable(card)
                );
            }
        }
    }


    // =====================================================
    // 랜덤 카드
    // =====================================================

    public void ChooseRandomCard()
    {
        if (next_card != null)
            return;


        if (enemy_card_list.Count == 0)
        {
            next_card = null;


            Debug.LogWarning(
                enemy_character_info.character_name +
                " : 사용할 카드가 없음"
            );


            return;
        }


        int randomIndex =
            UnityEngine.Random.Range(
                0,
                enemy_card_list.Count
            );


        next_card =
            enemy_card_list[randomIndex];
    }


    // =====================================================
    // 랜덤 타겟
    // =====================================================

    public void ChooseRandomTarget(
        List<CharacterVariable> playerCharacters,
        List<CharacterVariable> enemyCharacters)
    {
        target_index = -1;


        if (next_card == null)
            return;


        if (next_card.original_card_info == null)
            return;


        CardTarget targetType =
            next_card.original_card_info.cardTarget;


        if (targetType == CardTarget.None)
        {
            target_index = -1;
            return;
        }


        List<CharacterVariable> targetList =
            new List<CharacterVariable>();


        if (targetType == CardTarget.Enemy)
        {
            AddAliveCharacters(
                targetList,
                playerCharacters
            );
        }
        else if (targetType == CardTarget.Ally)
        {
            AddAliveCharacters(
                targetList,
                enemyCharacters
            );
        }
        else if (targetType == CardTarget.Any)
        {
            AddAliveCharacters(
                targetList,
                playerCharacters
            );


            AddAliveCharacters(
                targetList,
                enemyCharacters
            );
        }


        if (targetList.Count == 0)
        {
            target_index = -1;
            return;
        }


        int randomIndex =
            UnityEngine.Random.Range(
                0,
                targetList.Count
            );


        CharacterVariable target =
            targetList[randomIndex];


        target_index =
            target.character_location_index;
    }


    // =====================================================
    // 살아있는 캐릭터 추가
    // =====================================================

    private void AddAliveCharacters(
        List<CharacterVariable> targetList,
        List<CharacterVariable> characters)
    {
        if (characters == null)
            return;


        foreach (CharacterVariable character
                 in characters)
        {
            if (character == null)
                continue;


            if (character.is_dead)
                continue;


            targetList.Add(character);
        }
    }
}