using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterVariable
{
    public CharacterInfo character_info;
    public CharacterView characterView;

    public bool is_player_character = true;
    public int character_location_index = -1;
    public bool is_dead = false;

    public CharacterStatContainer statContainer = new();

    // UI / 시스템 이벤트
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnEnergyChanged;
    public event Action OnDeath;

    // 버프 변경 이벤트
    public event Action<List<CharacterBuffValue>> OnBuffChanged;


    // ========================================
    // 현재 체력
    // ========================================

    public float current_health
    {
        get
        {
            return statContainer.GetRuntimeStat(
                CharacterRuntimeStatType.CurrentHealth);
        }

        set
        {
            CharacterRuntimeStatValue stat =
                statContainer.runtimeStatList
                .Find(x =>
                    x.type == CharacterRuntimeStatType.CurrentHealth);

            if (stat == null)
            {
                statContainer.runtimeStatList.Add(
                    new CharacterRuntimeStatValue
                    {
                        type = CharacterRuntimeStatType.CurrentHealth,
                        value = value
                    });
            }
            else
            {
                stat.value = value;
            }

            OnHealthChanged?.Invoke(
                (int)current_health,
                (int)max_health);
        }
    }


    // ========================================
    // 현재 에너지
    // ========================================

    public float current_energy
    {
        get
        {
            return statContainer.GetRuntimeStat(
                CharacterRuntimeStatType.CurrentEnergy);
        }

        set
        {
            CharacterRuntimeStatValue stat =
                statContainer.runtimeStatList
                .Find(x =>
                    x.type == CharacterRuntimeStatType.CurrentEnergy);

            if (stat == null)
            {
                statContainer.runtimeStatList.Add(
                    new CharacterRuntimeStatValue
                    {
                        type = CharacterRuntimeStatType.CurrentEnergy,
                        value = value
                    });
            }
            else
            {
                stat.value = value;
            }

            OnEnergyChanged?.Invoke(
                (int)current_energy,
                (int)MaxEnergy);
        }
    }


    // ========================================
    // 최대 에너지
    // ========================================

    public float MaxEnergy =>
        statContainer.GetBaseStat(
            CharacterBaseStatType.MaxEnergy);


    // ========================================
    // 최대 체력
    // ========================================

    public float max_health =>
        statContainer.GetBaseStat(
            CharacterBaseStatType.MaxHealth);


    // ========================================
    // 공격 순서
    // ========================================

    public float AttackOrder =>
        statContainer.GetBaseStat(
            CharacterBaseStatType.AttackOrder);


    // ========================================
    // 스탯 초기화
    // ========================================

    protected void InitializeStat(
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

        // 현재 레벨의 스탯 데이터만 가져오기
        CharacterLevelStat levelStat =
            info.levelStatList.Find(
                x => x != null &&
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


    // ========================================
    // 버프 추가
    // ========================================

    public void AddBuff(
        CharacterBuffType type,
        float value)
    {
        Debug.Log(
            $"[CharacterVariable] AddBuff : {type} / {value}"
        );

        statContainer.AddBuff(type, value);

        CharacterBuffValue buff =
            statContainer.buffList
            .Find(x => x.type == type);

        if (buff != null && buff.value == 0)
        {
            statContainer.buffList.Remove(buff);

            Debug.Log(
                $"[CharacterVariable] 버프 제거 : {type}"
            );
        }

        Debug.Log(
            $"[CharacterVariable] 현재 버프 개수 : " +
            $"{statContainer.buffList.Count}"
        );

        OnBuffChanged?.Invoke(
            statContainer.buffList
        );
    }


    // ========================================
    // 버프 제거
    // ========================================

    public void RemoveBuff(
        CharacterBuffType type)
    {
        CharacterBuffValue buff =
            statContainer.buffList
            .Find(x => x.type == type);

        if (buff == null)
            return;

        statContainer.buffList.Remove(buff);

        OnBuffChanged?.Invoke(
            statContainer.buffList);
    }


    // ========================================
    // 데미지
    // ========================================

    public virtual void TakeDamage(float damage)
    {
        if (is_dead)
            return;

        current_health =
            Mathf.Max(
                0,
                current_health - damage);

        characterView?.TakeDamage((int)damage);

        if (current_health <= 0)
        {
            Die();
        }
    }


    // ========================================
    // 회복
    // ========================================

    public virtual void Heal(float amount)
    {
        if (is_dead)
            return;

        current_health =
            Mathf.Min(
                max_health,
                current_health + amount);
    }


    // ========================================
    // 사망 처리
    // ========================================

    protected virtual void Die()
    {
        if (is_dead)
            return;

        is_dead = true;

        OnDeath?.Invoke();

        if (TurnManager.Instance.CurrentCharacter == this)
        {
            TurnManager.Instance.EndCurrentTurn();
        }
    }
}


// ========================================
// 플레이어 캐릭터
// ========================================

[System.Serializable]
public class PlayerCharacterVariable : CharacterVariable
{
    public PlayerCharacterInfo player_character_info;

    public PlayerCharacterData originalPlayerCharacterData;

    public List<CardVariable> hand_card_list = new();

    public List<CardVariable> graveyard_card_list = new();

    public List<CardVariable> deck_card_list = new();

    public int stress_count = 0;


    // ========================================
    // 생성자
    // ========================================

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


        // ========================================
        // 캐릭터 레벨 스탯 적용
        // ========================================

        InitializeStat(
            PCI,
            data != null
                ? data.player_character_level
                : 0
        );


        // ========================================
        // 장착 장비 스탯 적용
        // ========================================

        ApplyEquipmentStats(data);
    }


    // ========================================
    // 장비 스탯 적용
    // ========================================

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


            // 장비의 기본 스탯을
            // 캐릭터 기본 스탯에 합산
            statContainer.MergeBaseStatList(
                equipment.baseStatList
            );


            Debug.Log(
                $"장비 스탯 적용 : " +
                $"{equipment.itemName}"
            );
        }


        // 장비로 최대 체력이 변경되었으므로
        // 현재 체력을 최대 체력에 맞춰 초기화하지 않음.
        //
        // 실제 현재 체력은
        // BattleCharacterManager에서
        // 저장된 current_health를 다시 적용함.
    }
}


// ========================================
// 적 캐릭터
// ========================================

[System.Serializable]
public class EnemyCharacterVariable : CharacterVariable
{
    public EnemyCharacterInfo enemy_character_info;


    // 적이 사용할 수 있는 카드
    public List<CardVariable> enemy_card_list = new();


    // 이번 턴에 사용할 카드
    public CardVariable next_card;


    // 이번 턴에 공격/효과를 받을 대상
    public int target_index = -1;


    public EnemyCharacterVariable(
        EnemyCharacterInfo ECI,
        int index)
    {
        is_player_character = false;

        character_location_index = index;

        character_info = ECI;

        enemy_character_info = ECI;

        InitializeStat(ECI);


        // ========================================
        // 적 카드 생성
        // ========================================

        foreach (CardData card in ECI.enemy_card_info_list)
        {
            if (card == null)
            {
                Debug.LogError(
                    "적 카드 목록에 NULL 카드가 존재 : " +
                    ECI.character_name
                );

                continue;
            }

            enemy_card_list.Add(
                new CardVariable(card)
            );
        }
    }


    // ========================================
    // 랜덤 카드 선택
    // ========================================

    public void ChooseRandomCard()
    {
        if (enemy_card_list.Count == 0)
        {
            next_card = null;

            Debug.LogWarning(
                enemy_character_info.character_name +
                " : 사용할 수 있는 카드가 없음"
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


    // ========================================
    // 랜덤 타겟 선택
    // ========================================

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


    // ========================================
    // 살아있는 캐릭터만 추가
    // ========================================

    private void AddAliveCharacters(
        List<CharacterVariable> targetList,
        List<CharacterVariable> characters)
    {
        if (characters == null)
            return;

        foreach (CharacterVariable character in characters)
        {
            if (character == null)
                continue;

            if (character.is_dead)
                continue;

            targetList.Add(character);
        }
    }


    // ========================================
    // 카드 + 타겟 한 번에 결정
    // ========================================

    public void ChooseRandomAction(
        List<CharacterVariable> playerCharacters,
        List<CharacterVariable> enemyCharacters)
    {
        ChooseRandomCard();

        if (next_card == null)
            return;

        ChooseRandomTarget(
            playerCharacters,
            enemyCharacters
        );
    }
}