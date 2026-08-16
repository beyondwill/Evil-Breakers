using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BattleCharacterManager : MonoBehaviour
{
    public static BattleCharacterManager Instance;


    [SerializeField] private Turn enemySkillName;


    [Header("Character View")]
    [SerializeField] private CharacterView characterViewPrefab;
    [SerializeField] private Transform playerCharacterParent;
    [SerializeField] private Transform enemyCharacterParent;
    [SerializeField] private CharactersStand charactersStand;


    [Header("Battle Character")]
    [SerializeField] private List<CharacterVariable> playerCharacterList = new();
    [SerializeField] private List<CharacterVariable> enemyCharacterList = new();


    [Header("Victory Defeat")]
    private bool battleEnded = false;

    [SerializeField] private GameObject victoryObject;
    [SerializeField] private GameObject defeatObject;


    private void Awake()
    {
        Instance = this;
    }


    public void InitializeBattleCharacters()
    {
        playerCharacterList.Clear();
        enemyCharacterList.Clear();

        CreatePlayerCharacters();
        CreateEnemyCharacters();
    }


    // =====================================================
    // 플레이어 생성
    // =====================================================

    private void CreatePlayerCharacters()
    {
        BattleData battleData =
            DataManager.Instance.GetBattleData;


        for (int i = 0;
             i < battleData.characters_in_battle_data_list.Count;
             i++)
        {
            PlayerCharacterData data =
                battleData.characters_in_battle_data_list[i];


            if (data.current_health == 0)
                continue;


            PlayerCharacterVariable character =
                CreatePlayerCharacter(data, i);


            playerCharacterList.Add(character);

            CreateCharacterView(character);
        }
    }


    private PlayerCharacterVariable CreatePlayerCharacter(
    PlayerCharacterData data,
    int index)
    {
        PlayerCharacterVariable character =
            new PlayerCharacterVariable(
                data.player_character_info,
                index,
                data
            );


        character.originalPlayerCharacterData = data;


        // ========================================
        // 저장된 현재 체력 적용
        // ========================================

        character.current_health =
            data.current_health;


        // ========================================
        // 덱 생성
        // ========================================

        foreach (CardData cardData
                 in data.player_character_deck)
        {
            if (cardData == null)
            {
                Debug.LogError(
                    "덱에 NULL 카드 존재 : " +
                    data.player_character_info.character_name
                );

                continue;
            }


            character.deck_card_list.Add(
                new CardVariable(cardData)
            );
        }


        // ========================================
        // 스탯 확인 로그
        // ========================================

        Debug.Log(
            $"[캐릭터 생성] " +
            $"{data.player_character_info.character_name} " +
            $"Lv.{data.player_character_level}"
        );

        Debug.Log(
            $"최대 체력 : {character.max_health}"
        );

        Debug.Log(
            $"공격 순서 : {character.AttackOrder}"
        );


        return character;
    }


    // =====================================================
    // 적 생성
    // =====================================================

    private void CreateEnemyCharacters()
    {
        for (int i = 0;
             i < DataManager.Instance.GetBattleData.enemyCharacterList.Count;
             i++)
        {
            EnemyCharacterVariable enemy =
                CreateEnemyCharacter(
                    DataManager.Instance
                        .GetBattleData
                        .enemyCharacterList[i],
                    i
                );


            enemyCharacterList.Add(enemy);

            CreateCharacterView(enemy);
        }
    }


    private EnemyCharacterVariable CreateEnemyCharacter(
        EnemyCharacterInfo info,
        int index)
    {
        EnemyCharacterVariable enemy =
            new EnemyCharacterVariable(
                info,
                index
            );


        enemy.current_health =
            enemy.max_health;


        return enemy;
    }


    // =====================================================
    // View 생성
    // =====================================================

    private void CreateCharacterView(
        CharacterVariable character)
    {
        character.OnDeath += CheckBattleResult;


        Transform parent =
            character.is_player_character
                ? playerCharacterParent
                : enemyCharacterParent;


        CharacterView view =
            Instantiate(
                characterViewPrefab,
                parent
            );


        view.CharacterInit(character);

        charactersStand.AddCharacterView(view);
    }


    // =====================================================
    // 전투 결과
    // =====================================================

    private void CheckBattleResult()
    {
        if (IsAllEnemyDead())
        {
            EndBattle(true);
        }
        else if (IsAllPlayerDead())
        {
            EndBattle(false);
        }
    }


    private bool IsAllEnemyDead()
    {
        foreach (CharacterVariable enemy
                 in enemyCharacterList)
        {
            if (!enemy.is_dead)
                return false;
        }

        return true;
    }


    private bool IsAllPlayerDead()
    {
        foreach (CharacterVariable player
                 in playerCharacterList)
        {
            if (!player.is_dead)
                return false;
        }

        return true;
    }


    // =====================================================
    // 임시
    // =====================================================

    public InventoryItem item1;
    public InventoryItem item2;
    public InventoryItem item3;


    private void EndBattle(bool win)
    {
        if (battleEnded)
            return;


        battleEnded = true;


        DataManager.Instance
            .GetBattleData
            .leftRewards
            .Add(item1);


        DataManager.Instance
            .GetBattleData
            .leftRewards
            .Add(item2);


        DataManager.Instance
            .GetBattleData
            .leftRewards
            .Add(item3);


        DataManager.Instance
            .GetAllData
            .current_state =
            CurrentState.BattleEnd;


        DataManager.Instance
            .GetBattleData
            .enemyCharacterList
            .Clear();


        foreach (PlayerCharacterVariable PCV
                 in playerCharacterList)
        {
            PCV.originalPlayerCharacterData.current_health =
                (int)PCV.current_health;
        }


        DataManager.Instance.SaveData();


        if (win)
        {
            victoryObject.SetActive(true);


            DOVirtual.DelayedCall(
                2f,
                () =>
                {
                    SceneManager.LoadScene(
                        "Map Scene"
                    );
                }
            );
        }
        else
        {
            //defeatObject.SetActive(true);


            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    GameOverManager.Instance.GameOver2();
                }
            );
        }
    }


    // =====================================================
    // Enemy Turn
    // =====================================================

    public void EnemyTurn(
        EnemyCharacterVariable enemy)
    {
        if (enemy == null)
            return;


        if (enemy.is_dead)
            return;


        // =================================================
        // 1. 카드 선택
        // =================================================

        enemy.ChooseRandomCard();


        if (enemy.next_card == null)
        {
            Debug.LogWarning(
                enemy.enemy_character_info.character_name +
                " : 사용할 카드가 없습니다."
            );


            TurnManager.Instance.EndCurrentTurn();

            return;
        }


        CardVariable card =
            enemy.next_card;


        if (card.original_card_info == null)
        {
            Debug.LogWarning(
                enemy.enemy_character_info.character_name +
                " : 카드 정보가 없습니다."
            );


            enemy.next_card = null;

            TurnManager.Instance.EndCurrentTurn();

            return;
        }


        // =================================================
        // 2. 로그
        // =================================================

        Debug.Log(
            enemy.enemy_character_info.character_name +
            " → " +
            card.original_card_info.card_name
        );


        // =================================================
        // 3. 카드 실행
        // =================================================

        ExecuteEnemyCard(enemy);
    }


    // =====================================================
    // 적 카드 실행
    // =====================================================

    private void ExecuteEnemyCard(
        EnemyCharacterVariable enemy)
    {
        if (enemy == null)
            return;


        CardVariable card =
            enemy.next_card;


        if (card == null)
        {
            Debug.LogWarning(
                "적이 사용할 카드가 없습니다."
            );


            TurnManager.Instance.EndCurrentTurn();

            return;
        }


        if (card.original_card_info == null)
        {
            enemy.next_card = null;

            TurnManager.Instance.EndCurrentTurn();

            return;
        }


        // =================================================
        // Target
        // =================================================

        CharacterVariable target = null;

        CardTarget targetType =
            card.original_card_info.cardTarget;


        // None = 자기 자신
        if (targetType == CardTarget.None)
        {
            target = enemy;
        }
        else
        {
            target =
                GetEnemyCardTarget(
                    enemy,
                    targetType
                );

            if (target == null)
            {
                Debug.LogWarning(
                    enemy.enemy_character_info.character_name +
                    " : 카드의 대상이 없습니다."
                );

                enemy.next_card = null;

                TurnManager.Instance.EndCurrentTurn();

                return;
            }
        }


        // =================================================
        // Target List
        // =================================================

        List<CharacterVariable> targets =
            new List<CharacterVariable>();


        if (target != null)
        {
            targets.Add(target);
        }


        // =================================================
        // 스킬 이름
        // =================================================

        enemySkillName.gameObject.SetActive(true);


        enemySkillName.Init(
            card.original_card_info.card_name
        );


        // =================================================
        // 0.8초 후 실제 효과
        // =================================================

        DOVirtual.DelayedCall(
            0.8f,
            () =>
            {
                if (enemy == null ||
                    enemy.is_dead)
                {
                    return;
                }


                if (TurnManager.Instance.CurrentCharacter != enemy)
                {
                    return;
                }


                // =================================================
                // 카드 효과 실행
                // =================================================

                foreach (
                    CardEffectEntry entry
                    in card.original_card_info.effects)
                {
                    if (entry.visual != null)
                    {
                        entry.visual.Play(
                            enemy,
                            targets
                        );
                    }


                    if (entry.effect != null)
                    {
                        entry.effect.Execute(
                            enemy,
                            targets,
                            entry,
                            card.original_card_info
                        );
                    }
                }


                // =================================================
                // 로그
                // =================================================

                if (target != null)
                {
                    Debug.Log(
                        enemy.enemy_character_info.character_name +
                        " → " +
                        card.original_card_info.card_name +
                        " → " +
                        target.character_info.character_name
                    );
                }
                else
                {
                    Debug.Log(
                        enemy.enemy_character_info.character_name +
                        " → " +
                        card.original_card_info.card_name
                    );
                }


                // =================================================
                // 초기화
                // =================================================

                enemy.next_card = null;


                // =================================================
                // 턴 종료
                // =================================================

                TurnManager.Instance.EndCurrentTurn();
            }
        );
    }


    // =====================================================
    // 적 카드 타겟 결정
    // =====================================================

    private CharacterVariable GetEnemyCardTarget(
        EnemyCharacterVariable enemy,
        CardTarget targetType)
    {
        if (enemy == null)
            return null;


        List<CharacterVariable> targetList =
            new List<CharacterVariable>();


        // =================================================
        // Enemy
        // 적 입장에서 Enemy = 플레이어
        // =================================================

        if (targetType == CardTarget.Enemy)
        {
            foreach (CharacterVariable player
                     in playerCharacterList)
            {
                if (player == null)
                    continue;


                if (player.is_dead)
                    continue;


                targetList.Add(player);
            }
        }


        // =================================================
        // Ally
        // 적 입장에서 Ally = 적
        // =================================================

        else if (targetType == CardTarget.Ally)
        {
            foreach (CharacterVariable ally
                     in enemyCharacterList)
            {
                if (ally == null)
                    continue;


                if (ally.is_dead)
                    continue;


                targetList.Add(ally);
            }
        }


        // =================================================
        // Any
        // 플레이어 + 적
        // =================================================

        else if (targetType == CardTarget.Any)
        {
            foreach (CharacterVariable player
                     in playerCharacterList)
            {
                if (player == null)
                    continue;


                if (player.is_dead)
                    continue;


                targetList.Add(player);
            }


            foreach (CharacterVariable ally
                     in enemyCharacterList)
            {
                if (ally == null)
                    continue;


                if (ally.is_dead)
                    continue;


                targetList.Add(ally);
            }
        }


        // =================================================
        // 대상 없음
        // =================================================

        if (targetList.Count == 0)
            return null;


        // =================================================
        // 랜덤 선택
        // =================================================

        int randomIndex =
            Random.Range(
                0,
                targetList.Count
            );


        return targetList[randomIndex];
    }


    // =====================================================
    // 외부 접근
    // =====================================================

    public List<CharacterVariable> PlayerCharacters
        => playerCharacterList;


    public List<CharacterVariable> EnemyCharacters
        => enemyCharacterList;
}