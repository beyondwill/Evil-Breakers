using System;
using System.Collections.Generic;
using UnityEngine;

#region TABLES

[System.Serializable]
public class PlayerCharacterTable : DataBase<PlayerCharacterInfo> { }

[System.Serializable]
public class EnemyCharacterTable : DataBase<EnemyCharacterInfo> { }

[System.Serializable]
public class EquipmentTable : DataBase<EquipmentInfo> { }

[System.Serializable]
public class ItemTable : DataBase<ItemData> { }

[System.Serializable]
public class LocationTable : DataBase<LocationInfo> { }

[System.Serializable]
public class MissionTable : DataBase<MissionInfo> { }

[System.Serializable]
public class CardTable : DataBase<CardData> { }

[System.Serializable]
public class MapTable : DataBase<HexMapDataSO> { }

[System.Serializable]
public class RelicTable : DataBase<RelicInfo> { }

#endregion

public class TableManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static TableManager Instance { get; private set; }


    // =========================================================
    // 외부 요소
    // =========================================================

    [Header("외부 요소")]
    [SerializeField] private DataManager dataManager;


    // =========================================================
    // Data Folders Path
    // =========================================================

    [Header("Data Folders Path")]

    public string playerFolderPath = "Assets/Data/Players";
    public string enemyFolderPath = "Assets/Data/Enemies";
    public string equipmentFolderPath = "Assets/Data/Equipments";
    public string itemFolderPath = "Assets/Data/Items";
    public string locationFolderPath = "Assets/Data/Locations";
    public string missionFolderPath = "Assets/Data/Missions";
    public string playercardFolderPath = "Assets/Data/PlayerCards";
    public string mapFolderPath = "Assets/Data/Maps";
    public string relicFolderPath = "Assets/Data/Relics";


    // =========================================================
    // Tables
    // =========================================================

    [Header("Tables")]

    public PlayerCharacterTable PCT = new();
    public EnemyCharacterTable ECT = new();
    public EquipmentTable ET = new();
    public ItemTable IT = new();
    public LocationTable LT = new();
    public MissionTable MT = new();
    public CardTable CT = new();
    public MapTable MAP = new();
    public RelicTable RT = new();


    // =========================================================
    // Table Dictionary
    // =========================================================

    private Dictionary<Type, object> tableMap;


    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        // 이미 TableManager가 존재한다면
        // 새로 생성된 TableManager는 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Singleton 등록
        Instance = this;

        // 씬이 변경되어도 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);


        // -----------------------------------------------------
        // 테이블 초기화
        // -----------------------------------------------------

        InitAllTables();


        // -----------------------------------------------------
        // 테이블 검색용 Dictionary 생성
        // -----------------------------------------------------

        BuildTableMap();


        // -----------------------------------------------------
        // DataManager 초기화
        // -----------------------------------------------------

        Debug.Log("TableManager 초기화 성공!");
    }


    // =========================================================
    // Table Dictionary 생성
    // =========================================================

    private void BuildTableMap()
    {
        tableMap = new Dictionary<Type, object>
        {
            { typeof(PlayerCharacterInfo), PCT },
            { typeof(EnemyCharacterInfo), ECT },
            { typeof(EquipmentInfo), ET },
            { typeof(ItemData), IT },
            { typeof(LocationInfo), LT },
            { typeof(MissionInfo), MT },
            { typeof(CardData), CT },
            { typeof(HexMapDataSO), MAP },
            { typeof(RelicInfo), RT }
        };
    }


    // =========================================================
    // 모든 테이블 초기화
    // =========================================================

    public void InitAllTables()
    {
        PCT.InitTable();
        ECT.InitTable();
        ET.InitTable();
        IT.InitTable();
        LT.InitTable();
        MT.InitTable();
        CT.InitTable();
        MAP.InitTable();
        RT.InitTable();

        Debug.Log("모든 테이블 초기화 완료!");
    }


    // =========================================================
    // 폴더에서 모든 데이터 불러오기
    // =========================================================

    [ContextMenu("Load All Tables From Folders")]
    public void LoadAllTablesFromFolder()
    {
        PCT.FillDataFromFolder(playerFolderPath);
        ECT.FillDataFromFolder(enemyFolderPath);
        ET.FillDataFromFolder(equipmentFolderPath);
        IT.FillDataFromFolder(itemFolderPath);
        LT.FillDataFromFolder(locationFolderPath);
        MT.FillDataFromFolder(missionFolderPath);
        CT.FillDataFromFolder(playercardFolderPath);
        MAP.FillDataFromFolder(mapFolderPath);
        RT.FillDataFromFolder(relicFolderPath);

        Debug.Log("모든 폴더 데이터 로드 완료!");
    }


    // =========================================================
    // 모든 테이블 데이터 삭제
    // =========================================================

    public void ClearAllTables()
    {
        PCT.ClearList();
        ECT.ClearList();
        ET.ClearList();
        IT.ClearList();
        LT.ClearList();
        MT.ClearList();
        CT.ClearList();
        MAP.ClearList();
        RT.ClearList();

        Debug.Log("모든 테이블 클리어 완료!");
    }


    // =========================================================
    // Type + Key로 데이터 검색
    // =========================================================

    public ScriptableObject GetAny(Type type, string key)
    {
        if (tableMap == null)
        {
            Debug.LogError("TableManager : tableMap이 초기화되지 않았습니다.");
            return null;
        }


        if (tableMap.TryGetValue(type, out object table))
        {
            var method = table.GetType().GetMethod("Get");

            return method?.Invoke(
                table,
                new object[] { key }
            ) as ScriptableObject;
        }


        Debug.LogWarning(
            $"TableManager : 등록되지 않은 Type입니다. Type = {type}"
        );

        return null;
    }
}