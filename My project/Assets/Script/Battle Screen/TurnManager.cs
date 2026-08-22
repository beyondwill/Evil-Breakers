using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    // =========================================================
    // Turn Order
    // =========================================================

    private List<CharacterVariable> turnOrderList = new();

    private int currentTurnIndex;

    private Tween turnDelayTween;

    // 현재 턴 종료 처리 중인지
    private bool isEndingTurn;

    // 현재 턴이 Stun 때문에 스킵된 턴인지
    private bool isCurrentTurnStunned;


    // =========================================================
    // 외부 이미지
    // =========================================================

    [SerializeField]
    private GameObject PlayerTurn;

    [SerializeField]
    private GameObject EnemyTurn;

    [SerializeField]
    private GameObject turnEndButton;


    // =========================================================
    // Current Character
    // =========================================================

    public CharacterVariable CurrentCharacter
    {
        get;
        private set;
    }


    // =========================================================
    // Round
    // =========================================================

    public int CurrentRound
    {
        get;
        private set;
    } = 1;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // Start Battle
    // =========================================================

    public void StartBattle()
    {
        KillTurnTween();

        isEndingTurn = false;
        isCurrentTurnStunned = false;

        CreateTurnOrder();

        CurrentRound = 1;

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowRound(
                CurrentRound
            );
        }

        turnDelayTween =
            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    StartTurn();
                }
            );
    }


    // =========================================================
    // Create Turn Order
    // =========================================================

    public void CreateTurnOrder()
    {
        turnOrderList.Clear();

        if (BattleCharacterManager.Instance == null)
        {
            Debug.LogError(
                "TurnManager : BattleCharacterManager가 없습니다."
            );

            return;
        }

        turnOrderList.AddRange(
            BattleCharacterManager.Instance.PlayerCharacters
        );

        turnOrderList.AddRange(
            BattleCharacterManager.Instance.EnemyCharacters
        );

        turnOrderList.RemoveAll(
            x => x == null || x.is_dead
        );

        turnOrderList.Sort(
            (a, b) => (b.AttackOrder + b.statContainer.GetBuff(CharacterBuffType.Acceleration)).CompareTo(a.AttackOrder + a.statContainer.GetBuff(CharacterBuffType.Acceleration))
        );

        currentTurnIndex = 0;

        if (TurnOrderUIManager.Instance != null)
        {
            TurnOrderUIManager.Instance.RefreshTurnOrder(
                turnOrderList
            );
        }
    }


    // =========================================================
    // Start Turn
    // =========================================================

    public void StartTurn()
    {
        KillTurnTween();

        isEndingTurn = false;
        isCurrentTurnStunned = false;

        // 실제 플레이어 턴일 때만 켠다.
        if (turnEndButton != null)
        {
            turnEndButton.SetActive(false);
        }


        // -----------------------------------------------------
        // 턴 목록 확인
        // -----------------------------------------------------

        if (turnOrderList == null ||
            turnOrderList.Count == 0)
        {
            EndBattle();
            return;
        }


        // -----------------------------------------------------
        // Index 보정
        // -----------------------------------------------------

        if (currentTurnIndex < 0)
        {
            currentTurnIndex = 0;
        }


        if (currentTurnIndex >= turnOrderList.Count)
        {
            EndRound();
            return;
        }


        // -----------------------------------------------------
        // 죽은 캐릭터 건너뛰기
        // -----------------------------------------------------

        while (
            currentTurnIndex < turnOrderList.Count &&
            (
                turnOrderList[currentTurnIndex] == null ||
                turnOrderList[currentTurnIndex].is_dead
            ))
        {
            currentTurnIndex++;
        }


        // -----------------------------------------------------
        // 이번 턴 캐릭터 없음
        // -----------------------------------------------------

        if (currentTurnIndex >= turnOrderList.Count)
        {
            EndRound();
            return;
        }


        // -----------------------------------------------------
        // 현재 캐릭터 설정
        // -----------------------------------------------------

        CurrentCharacter =
            turnOrderList[currentTurnIndex];


        if (CurrentCharacter == null)
        {
            EndRound();
            return;
        }


        Debug.Log(
            "현재 턴 : " +
            CurrentCharacter.character_info.character_name
        );


        // =====================================================
        // Turn Order UI
        // =====================================================

        if (TurnOrderUIManager.Instance != null)
        {
            TurnOrderUIManager.Instance.SetCurrentTurn(
                CurrentCharacter
            );
        }


        // =====================================================
        // Character View 현재 턴 표시
        // =====================================================

        foreach (
            CharacterVariable character
            in turnOrderList)
        {
            if (character == null)
                continue;

            if (character.characterView == null)
                continue;

            character.characterView.SetCurrentTurn(
                character == CurrentCharacter
            );
        }


        // =====================================================
        // Player / Enemy 턴 표시
        // =====================================================

        if (CurrentCharacter.is_player_character)
        {
            if (PlayerTurn != null)
            {
                PlayerTurn.SetActive(true);
            }
        }
        else
        {
            if (EnemyTurn != null)
            {
                EnemyTurn.SetActive(true);
            }
        }


        // =====================================================
        // Stun 확인
        // =====================================================

        int stunCount =
            (int)CurrentCharacter.statContainer.GetBuff(
                CharacterBuffType.Stun
            );


        Debug.Log(
            "[TURN START CHECK] " +
            CurrentCharacter.character_info.character_name +
            " / Stun = " +
            stunCount
        );


        if (stunCount > 0)
        {
            Debug.Log(
                CurrentCharacter.character_info.character_name +
                " : Stun으로 이번 턴 행동을 건너뜁니다."
            );


            // -------------------------------------------------
            // 중요
            //
            // 여기서는 Stun을 제거하지 않는다.
            //
            // 캐릭터의 턴은 정상적으로 시작되었고
            // 현재 턴 UI도 표시된 상태로 유지한다.
            // -------------------------------------------------

            isCurrentTurnStunned = true;


            // -------------------------------------------------
            // 행동하지 않고 턴 종료
            // -------------------------------------------------

            EndCurrentTurn();

            return;
        }


        // =====================================================
        // 실제 턴 시작
        // =====================================================

        if (CurrentCharacter.is_player_character)
        {
            StartPlayerTurn();
        }
        else
        {
            StartEnemyTurn();
        }
    }


    // =========================================================
    // Player Turn
    // =========================================================

    private void StartPlayerTurn()
    {
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ResetTurnObjects();
        }


        if (PlayerTurn != null)
        {
            PlayerTurn.SetActive(true);
        }


        if (turnEndButton != null)
        {
            turnEndButton.SetActive(true);
        }


        PlayerCharacterVariable player =
            CurrentCharacter as PlayerCharacterVariable;


        if (player == null)
        {
            Debug.LogError(
                "TurnManager : 현재 캐릭터가 PlayerCharacterVariable이 아닙니다."
            );

            EndCurrentTurn();
            return;
        }


        Debug.Log(
            "플레이어 턴 : " +
            player.character_info.character_name
        );


        if (CardManager.Instance == null)
        {
            Debug.LogError(
                "TurnManager : CardManager가 없습니다."
            );

            EndCurrentTurn();
            return;
        }


        CardManager.Instance.SetCurrentCharacter(
            player
        );


        player.current_energy =
            player.MaxEnergy;


        CardManager.Instance.DrawStartHand(5);
    }


    // =========================================================
    // Enemy Turn
    // =========================================================

    private void StartEnemyTurn()
    {
        if (EnemyTurn != null)
        {
            EnemyTurn.SetActive(true);
        }


        if (turnEndButton != null)
        {
            turnEndButton.SetActive(false);
        }


        if (CurrentCharacter == null)
        {
            Debug.LogError(
                "TurnManager : CurrentCharacter가 없습니다."
            );

            return;
        }


        Debug.Log(
            "적 AI 실행 : " +
            CurrentCharacter.character_info.character_name
        );


        EnemyCharacterVariable enemy =
            CurrentCharacter as EnemyCharacterVariable;


        if (enemy == null)
        {
            Debug.LogError(
                "현재 캐릭터가 EnemyCharacterVariable이 아님"
            );

            EndCurrentTurn();
            return;
        }


        KillTurnTween();


        turnDelayTween =
            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    if (enemy == null ||
                        enemy.is_dead)
                    {
                        return;
                    }


                    if (CurrentCharacter != enemy)
                    {
                        return;
                    }


                    if (BattleCharacterManager.Instance == null)
                    {
                        Debug.LogError(
                            "BattleCharacterManager가 없습니다."
                        );

                        return;
                    }


                    BattleCharacterManager.Instance.EnemyTurn(
                        enemy
                    );
                }
            );
    }


    // =========================================================
    // End Current Turn
    // =========================================================

    public void EndCurrentTurn()
    {
        Debug.Log(
            $"[TURN END 호출] " +
            $"현재 캐릭터 = " +
            $"{CurrentCharacter?.character_info.character_name}, " +
            $"Index = {currentTurnIndex}, " +
            $"Stunned = {isCurrentTurnStunned}, " +
            $"isEndingTurn = {isEndingTurn}"
        );


        // -----------------------------------------------------
        // 이미 종료 중이면 무시
        // -----------------------------------------------------

        if (isEndingTurn)
        {
            return;
        }


        isEndingTurn = true;


        // -----------------------------------------------------
        // 기존 Enemy AI / Turn Tween 제거
        // -----------------------------------------------------

        KillTurnTween();


        // -----------------------------------------------------
        // 종료할 캐릭터 저장
        // -----------------------------------------------------

        CharacterVariable endCharacter =
            CurrentCharacter;


        if (endCharacter == null)
        {
            isEndingTurn = false;

            Debug.LogWarning(
                "TurnManager : 종료할 CurrentCharacter가 없습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // 플레이어 손패 버리기
        // -----------------------------------------------------

        if (endCharacter.is_player_character)
        {
            if (CardManager.Instance != null)
            {
                CardManager.Instance.DiscardHand();
            }
        }


        // -----------------------------------------------------
        // Turn Object 이동
        // -----------------------------------------------------

        turnDelayTween =
            DOVirtual.DelayedCall(
                1f,
                () =>
                {
                    if (BattleUIManager.Instance != null)
                    {
                        BattleUIManager.Instance.MoveTurnObjects();
                    }


                    // -------------------------------------------------
                    // 실제 턴 종료
                    // -------------------------------------------------

                    turnDelayTween =
                        DOVirtual.DelayedCall(
                            1f,
                            () =>
                            {
                                FinishTurn(
                                    endCharacter
                                );
                            }
                        );
                }
            );
    }


    // =========================================================
    // Finish Turn
    // =========================================================

    private void FinishTurn(
        CharacterVariable endCharacter)
    {
        if (endCharacter == null)
        {
            isEndingTurn = false;
            isCurrentTurnStunned = false;

            StartTurn();

            return;
        }


        // =====================================================
        // 공포 증가
        // =====================================================

        if (DataManager.Instance != null)
        {
            BattleData battleData =
                DataManager.Instance.GetBattleData;

            if (battleData != null)
            {
                Debug.Log(
                    $"[HORROR] 턴 종료 → 공포 +10 / 현재 공포 : {battleData.GetHorror()}"
                );
            }
        }


        // =====================================================
        // Stun 턴이었다면 여기서 Stun 1회 소모
        // =====================================================

        if (isCurrentTurnStunned)
        {
            Debug.Log(
                endCharacter.character_info.character_name +
                " : Stun 턴 종료 → Stun 1 감소"
            );

            endCharacter.AddBuff(
                CharacterBuffType.Stun,
                -1
            );

            isCurrentTurnStunned = false;
        }


        // =====================================================
        // 현재 캐릭터 다음의 살아있는 캐릭터 찾기
        // =====================================================

        int oldIndex =
            turnOrderList.IndexOf(endCharacter);


        CharacterVariable nextCharacter = null;


        if (oldIndex >= 0)
        {
            for (
                int i = oldIndex + 1;
                i < turnOrderList.Count;
                i++)
            {
                CharacterVariable character =
                    turnOrderList[i];


                if (character == null)
                    continue;


                if (character.is_dead)
                    continue;


                nextCharacter = character;

                break;
            }
        }


        // =====================================================
        // Debug
        // =====================================================

        Debug.Log(
            "[TURN FINISH] " +
            "현재 캐릭터 : " +
            endCharacter.character_info.character_name +
            " / OldIndex : " +
            oldIndex +
            " / Dead : " +
            endCharacter.is_dead +
            " / 다음 캐릭터 : " +
            (
                nextCharacter != null
                    ? nextCharacter.character_info.character_name
                    : "없음"
            )
        );


        // =====================================================
        // Turn Order UI 제거
        // =====================================================

        if (TurnOrderUIManager.Instance != null)
        {
            TurnOrderUIManager.Instance.RemoveCharacter(
                endCharacter
            );
        }


        // =====================================================
        // 죽은 캐릭터 제거
        // =====================================================

        RemoveDeadCharacters();


        // =====================================================
        // 다음 캐릭터가 존재
        // =====================================================

        if (nextCharacter != null)
        {
            int nextIndex =
                turnOrderList.IndexOf(nextCharacter);


            if (nextIndex >= 0)
            {
                currentTurnIndex = nextIndex;


                Debug.Log(
                    "[TURN NEXT] " +
                    "다음 턴 : " +
                    nextCharacter.character_info.character_name +
                    " / Index : " +
                    currentTurnIndex
                );


                isEndingTurn = false;


                StartTurn();

                return;
            }
        }


        // =====================================================
        // 다음 캐릭터가 없다면 라운드 종료
        // =====================================================

        Debug.Log(
            "[TURN END] 다음 캐릭터 없음 → 라운드 종료"
        );


        isEndingTurn = false;

        EndRound();
    }



    // =========================================================
    // End Round
    // =========================================================

    private void EndRound()
    {
        KillTurnTween();

        isEndingTurn = false;
        isCurrentTurnStunned = false;

        CurrentCharacter = null;

        CurrentRound++;

        DataManager.Instance.GetBattleData.ReduceTime(1);

        CreateTurnOrder();


        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowRound(
                CurrentRound
            );
        }

        DataManager.Instance.GetBattleData.AddHorror(10);

        turnDelayTween =
            DOVirtual.DelayedCall(
                2f,
                () =>
                {
                    StartTurn();
                }
            );
    }


    // =========================================================
    // Remove Dead Characters
    // =========================================================

    private void RemoveDeadCharacters()
    {
        turnOrderList.RemoveAll(
            x =>
                x == null ||
                x.is_dead
        );
    }


    // =========================================================
    // Add Character
    // =========================================================

    public void AddCharacter(
        CharacterVariable character)
    {
        if (character == null)
            return;


        if (character.is_dead)
            return;


        if (turnOrderList.Contains(character))
            return;


        turnOrderList.Add(character);
    }


    // =========================================================
    // Remove Character
    // =========================================================

    public void RemoveCharacter(
        CharacterVariable character)
    {
        if (character == null)
            return;


        if (TurnOrderUIManager.Instance != null)
        {
            TurnOrderUIManager.Instance.RemoveCharacter(
                character
            );
        }
    }


    // =========================================================
    // Kill Tween
    // =========================================================

    private void KillTurnTween()
    {
        if (turnDelayTween != null &&
            turnDelayTween.IsActive())
        {
            turnDelayTween.Kill();
        }


        turnDelayTween = null;
    }


    // =========================================================
    // End Battle
    // =========================================================

    private void EndBattle()
    {
        KillTurnTween();

        isEndingTurn = false;
        isCurrentTurnStunned = false;

        CurrentCharacter = null;


        if (turnEndButton != null)
        {
            turnEndButton.SetActive(false);
        }


        Debug.Log(
            "전투 종료"
        );
    }
}