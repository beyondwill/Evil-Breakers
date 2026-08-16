using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenManager : MonoBehaviour
{
    // 인스턴스화
    public static MainScreenManager Instance;

    // 외부 요소
    [SerializeField] private MissionInfoManager missionInfoManager;
    [SerializeField] private List<LocationInfo> location_info_list;
    [SerializeField] GameObject mission_prefab;
    [SerializeField] GameObject needMoreCharacters;
    [SerializeField] GameObject mission_content;
    [SerializeField] MissionInfoManager mission_info_manager;
    [SerializeField] AudioClip main_screen_music;

    [SerializeField] List<TextMeshProUGUI> agent_count_list;        // 요원 수 리스트
    [SerializeField] private List<GameObject> closeGameObjects;

    // 변수
    public int index;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM(main_screen_music);

        if (DataManager.Instance.GetAllData.main_data.mapDataList.Count != 3)
        {
            foreach(LocationInfo LI in location_info_list)
            {
                DataManager.Instance.GetAllData.main_data.mapDataList.Add(HexMapGenerator.Instance.GenerateMapData(LI.HMDS));
            }
        }

        DataManager.Instance.GetAllData.SetCurrentState(CurrentState.MainScreen);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach(GameObject closeGameObject in closeGameObjects)
            {
                closeGameObject.SetActive(false);
            }
        }
    }

    // 구역 선택
    public void SelectedLocation(int index)
    {

    }

    // 미션 선택
    public void SelectdMission(LocationInfo LI)
    {
        mission_info_manager.gameObject.SetActive(true);
        mission_info_manager.ShowMissionInfo(LI);
        AudioManager.Instance.PlaySFX(SFX.Click);
    }

    // 전투 개시
    public void BattleBegin()
    {
        // 캐릭터의 수가 0인 경우: 더 채워야함
        if (missionInfoManager.GetSelectedCharacters().Count == 0)
        {
            if (!needMoreCharacters.activeSelf)
            {
                needMoreCharacters.SetActive(true);
            }
            return;
        }
        DataManager.Instance.GetBattleData.map_data = DataManager.Instance.GetAllData.main_data.mapDataList[index];
        DataManager.Instance.GetBattleData.characters_in_battle_data_list = missionInfoManager.GetSelectedCharacters();
        DataManager.Instance.SaveData();
        LoadingData.next_scene = "Map Scene";
        SceneManager.LoadScene("Loading Scene");
    }

    public void SetIndex(int i)
    {
        index = i;
    }
}
