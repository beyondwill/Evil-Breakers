using System.Collections;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }


    [Header("외부 요소")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private GameOverUI gameOverUI2;


    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        StartCoroutine(Initialize());
    }


    private IEnumerator Initialize()
    {
        // DataManager를 비롯한 다른 Start가 먼저 실행되도록
        // 한 프레임 기다림
        yield return null;

        if (DataManager.Instance == null)
        {
            Debug.LogWarning(
                "GameOverManager : DataManager가 없습니다."
            );

            yield break;
        }


        if (DataManager.Instance.GetBattleData == null)
        {
            Debug.LogWarning(
                "GameOverManager : BattleData가 없습니다."
            );

            yield break;
        }


        // 시간 변경 이벤트 구독
        DataManager.Instance.GetBattleData.OnTimeChanged += CheckTime;


        // 현재 시간이 이미 0 이하인 경우도 검사
        CheckTime(
            DataManager.Instance.GetBattleData.time,
            0
        );
    }


    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;


        if (DataManager.Instance == null)
            return;


        if (DataManager.Instance.GetBattleData == null)
            return;


        // 시간 변경 이벤트 해제
        DataManager.Instance.GetBattleData.OnTimeChanged -= CheckTime;
    }


    // ==========================================
    // 시간 변경 확인
    // ==========================================

    private void CheckTime(
        int time,
        int changeAmount)
    {
        if (time <= 0)
        {
            GameOver();
        }
    }


    // ==========================================
    // 게임 오버
    // ==========================================

    public void GameOver()
    {
        Debug.Log("GAME OVER");


        // ==========================================
        // 전투 중 변경된 캐릭터 정보 저장
        // ==========================================

        SaveCharacterData();


        // ==========================================
        // 저장
        // ==========================================

        DataManager.Instance.SaveData();


        // ==========================================
        // 게임 오버 UI
        // ==========================================

        gameOverUI.gameObject.SetActive(true);
        gameOverUI.GameOverInit();
    }


    public void GameOver2()
    {
        Debug.Log("GAME OVER");


        // ==========================================
        // 전투 중 변경된 캐릭터 정보 저장
        // ==========================================

        SaveCharacterData();


        // ==========================================
        // 저장
        // ==========================================

        DataManager.Instance.SaveData();


        // ==========================================
        // 게임 오버 UI
        // ==========================================

        gameOverUI2.gameObject.SetActive(true);
        gameOverUI2.GameOverInit();
    }


    // ==========================================
    // 캐릭터 데이터 저장
    // ==========================================

    private void SaveCharacterData()
    {
        if (DataManager.Instance == null)
            return;


        MainData mainData =
            DataManager.Instance
                .GetAllData
                .main_data;


        BattleData battleData =
            DataManager.Instance
                .GetBattleData;


        if (mainData == null ||
            battleData == null)
        {
            return;
        }


        if (battleData.characters_in_battle_data_list == null)
            return;


        if (mainData.player_character_data_list == null)
            return;


        // ==========================================
        // 전투 캐릭터 순회
        // ==========================================

        foreach (
            PlayerCharacterData battleCharacter
            in battleData.characters_in_battle_data_list)
        {
            if (battleCharacter == null)
                continue;


            // MainData에서 같은 캐릭터 찾기
            PlayerCharacterData mainCharacter =
                mainData.player_character_data_list.Find(
                    x =>
                        x != null &&
                        x.player_character_info ==
                        battleCharacter.player_character_info
                );


            if (mainCharacter == null)
            {
                Debug.LogWarning(
                    "게임 오버 저장 실패 : " +
                    battleCharacter.player_character_info
                        .character_name
                );

                continue;
            }


            // ==========================================
            // 현재 체력 저장
            // ==========================================

            mainCharacter.current_health =
                battleCharacter.current_health;


            // ==========================================
            // 스트레스 +10
            // ==========================================

            battleCharacter.current_stress += 10;


            mainCharacter.current_stress =
                battleCharacter.current_stress;


            // ==========================================
            // 로그
            // ==========================================

            Debug.Log(
                "[GAME OVER SAVE] " +
                battleCharacter.player_character_info.character_name +
                " / 체력 : " +
                battleCharacter.current_health +
                " / 스트레스 : " +
                mainCharacter.current_stress
            );
        }
    }
}