using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private int mission_index;                 // 미션 인덱스
    [SerializeField] private TextMeshProUGUI mission_name;      // 미션명
    [SerializeField] private string mission_name_prefix;        // 미션 앞
    [SerializeField] private LocationInfo location_info;        // 지역 정보

    // 초기화
    public void Init(LocationInfo MI, int index)
    {
        location_info = MI;
        mission_index = index;
        string name = MI.location_name;
        mission_name.text = mission_name_prefix + " " + (mission_index + 1).ToString() + ". " + name;
    }

    // 미션 버튼 클릭할 시
    public void MissionButtonClick()
    {
        MainScreenManager.Instance.SelectdMission(location_info);
    }
}
