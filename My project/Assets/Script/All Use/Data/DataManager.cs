using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine;


// =========================================================
// 현재 상황
// =========================================================

public enum CurrentState
{
    None,
    MainScreen,
    BattleMap,
    BattleBegin,
    BattleEnd,
    End
}


// =========================================================
// 모든 데이터
// =========================================================

[System.Serializable]
public class AllData
{
    public CurrentState current_state;

    public MainData main_data;
    public BattleData battle_data;


    public void SetCurrentState(CurrentState state)
    {
        current_state = state;
    }


    public CurrentState GetCurrentState()
    {
        return current_state;
    }
}


// =========================================================
// 전투 저장 데이터
// =========================================================

[System.Serializable]
public class BattleSaveData
{
    public HexNode.NodeType nodeType;
    public HexNode.ZoneType zoneType;

    public List<PlayerCharacterData>
        characters_in_battle_data_list;

    public List<EnemyCharacterInfo>
        enemyCharacterList;

    public List<InventoryItem>
        slots;

    public List<InventoryItem>
        leftRewards;

    public BattleResultVariable
        battle_result_variables;

    public MapSaveData map_data;

    public int time;

    // 추가
    public int horror;
}


// =========================================================
// DataManager
// =========================================================

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;


    [SerializeField]
    private AllData all_data;


    // 실제 저장 파일
    private string saveFilePath;


    private JsonSerializerSettings jsonSettings;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(gameObject);


        // -----------------------------------------------------
        // 저장 파일 경로
        // -----------------------------------------------------

        saveFilePath =
            Path.Combine(
                Application.streamingAssetsPath,
                "savefile.json"
            );


        // -----------------------------------------------------
        // JSON 설정
        // -----------------------------------------------------

        jsonSettings =
            new JsonSerializerSettings();


        jsonSettings.Converters.Add(
            new ScriptableObjectConverter()
        );


        jsonSettings.Formatting =
            Formatting.Indented;


        Debug.Log(
            "[DataManager] 사용할 저장 파일 : " +
            saveFilePath
        );


        Init();
    }


    // =========================================================
    // 초기화
    // =========================================================

    public void Init()
    {
        LoadData();
    }


    // =========================================================
    // Destroy
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // 데이터 초기화
    // =========================================================

    public void SetDataInit()
    {
        if (all_data == null)
        {
            all_data = new AllData();
        }


        if (all_data.main_data == null)
        {
            all_data.main_data =
                new MainData();
        }


        if (all_data.battle_data == null)
        {
            all_data.battle_data =
                new BattleData();
        }
    }


    // =========================================================
    // 저장
    // =========================================================

    public void SaveData()
    {
        if (all_data == null)
        {
            Debug.LogError(
                "[DataManager] 저장할 데이터가 없습니다."
            );

            return;
        }


        BattleData battle =
            all_data.battle_data;


        // -----------------------------------------------------
        // 저장 데이터 구성
        // -----------------------------------------------------

        var saveObj = new
        {
            current_state =
                all_data.current_state,

            main_data =
                all_data.main_data,

            battle_data =
                battle == null
                    ? null
                    : new
                    {
                        battle.nodeType,

                        battle.zoneType,

                        battle.characters_in_battle_data_list,

                        battle.enemyCharacterList,

                        battle.slots,

                        battle.leftRewards,

                        battle.battle_result_variables,

                        map_data =
                            battle.map_data != null
                                ? battle.map_data.GetSaveData()
                                : null,

                        time =
                            battle.GetTime(),

                        horror =
                            battle.GetHorror()
                    }
        };


        // -----------------------------------------------------
        // JSON 변환
        // -----------------------------------------------------

        string json =
            JsonConvert.SerializeObject(
                saveObj,
                jsonSettings
            );


        // -----------------------------------------------------
        // 저장
        // -----------------------------------------------------

        File.WriteAllText(
            saveFilePath,
            json
        );


        Debug.Log(
            $"게임 저장 완료!\n" +
            $"저장 경로 : {saveFilePath}"
        );
    }


    // =========================================================
    // Load용 Wrapper
    // =========================================================

    private class SaveDataWrapper
    {
        public CurrentState current_state;

        public MainData main_data;

        public BattleSaveData battle_data;
    }


    // =========================================================
    // 불러오기
    // =========================================================

    public void LoadData()
    {
        Debug.Log(
            $"[DataManager] 저장 경로 : " +
            saveFilePath
        );


        Debug.Log(
            $"[DataManager] 파일 존재 여부 : " +
            File.Exists(saveFilePath)
        );


        // -----------------------------------------------------
        // 파일 없음
        // -----------------------------------------------------

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning(
                "[DataManager] 저장 파일이 없습니다.\n" +
                $"경로 : {saveFilePath}"
            );

            SetDataInit();

            return;
        }


        // -----------------------------------------------------
        // JSON 읽기
        // -----------------------------------------------------

        string json =
            File.ReadAllText(
                saveFilePath
            );


        Debug.Log(
            $"[DataManager] 불러온 파일 : " +
            saveFilePath
        );


        // -----------------------------------------------------
        // Deserialize
        // -----------------------------------------------------

        SaveDataWrapper loadedData =
            JsonConvert.DeserializeObject<SaveDataWrapper>(
                json,
                jsonSettings
            );


        if (loadedData == null)
        {
            Debug.LogError(
                "[DataManager] " +
                "저장 데이터 Deserialize 실패"
            );

            return;
        }


        // -----------------------------------------------------
        // 기본 데이터
        // -----------------------------------------------------

        if (all_data == null)
        {
            all_data =
                new AllData();
        }


        all_data.current_state =
            loadedData.current_state;


        all_data.main_data =
            loadedData.main_data;


        // -----------------------------------------------------
        // BattleData
        // -----------------------------------------------------

        if (all_data.battle_data == null)
        {
            all_data.battle_data =
                new BattleData();
        }


        BattleSaveData save =
            loadedData.battle_data;


        if (save != null)
        {
            BattleData battle =
                all_data.battle_data;


            battle.nodeType =
                save.nodeType;


            battle.zoneType =
                save.zoneType;


            battle.characters_in_battle_data_list =
                save.characters_in_battle_data_list
                ?? new List<PlayerCharacterData>();


            battle.enemyCharacterList =
                save.enemyCharacterList
                ?? new List<EnemyCharacterInfo>();


            battle.slots =
                save.slots
                ?? new List<InventoryItem>();


            battle.leftRewards =
                save.leftRewards
                ?? new List<InventoryItem>();


            battle.battle_result_variables =
                save.battle_result_variables;


            // -------------------------------------------------
            // 시간
            // -------------------------------------------------

            battle.SetTime(
                save.time,
                false
            );


            // -------------------------------------------------
            // 공포도
            // -------------------------------------------------

            battle.SetHorror(
                save.horror,
                false
            );


            // -------------------------------------------------
            // Map
            // -------------------------------------------------

            if (save.map_data != null)
            {
                if (battle.map_data == null)
                {
                    battle.map_data =
                        new MapData();
                }


                battle.map_data.ApplySaveData(
                    save.map_data
                );
            }
        }


        Debug.Log(
            "[DataManager] 데이터 불러오기 완료"
        );
    }


    // =========================================================
    // 캐릭터 데이터 가져오기
    // =========================================================

    public PlayerCharacterData
        GetPlayerCharacterDataInBattle(
            PlayerCharacterInfo PCI)
    {
        if (all_data == null ||
            all_data.battle_data == null ||
            all_data.battle_data
                .characters_in_battle_data_list == null)
        {
            return null;
        }


        return all_data
            .battle_data
            .characters_in_battle_data_list
            .Find(
                n =>
                    n.player_character_info == PCI
            );
    }


    // =========================================================
    // 데이터 접근
    // =========================================================

    public AllData GetAllData
    {
        get
        {
            return all_data;
        }
    }


    public MainData GetMainData
    {
        get
        {
            return all_data.main_data;
        }
    }


    public BattleData GetBattleData
    {
        get
        {
            return all_data.battle_data;
        }
    }


    public List<PlayerCharacterData>
        PlayerCharacterDataList
    {
        get
        {
            return all_data
                .main_data
                .player_character_data_list;
        }
    }
}
