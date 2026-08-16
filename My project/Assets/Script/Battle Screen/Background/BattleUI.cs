using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    // 외부 요소
    [Header("Image")]
    [SerializeField] private Image battleFieldBackground;           // 전장 배경화면
    [SerializeField] private ZoneNodeConfig zoneNodeConfig;         // 구역 노드
    [SerializeField] private TopSideUI topSideUI;                   // 최상단 UI

    // 변수
    private Area area;

    void Start()
    {
        // 배경 정하고, 무작위 음악 재생
        ZoneNodeData zoneNodeData = zoneNodeConfig.GetZoneNodeData(DataManager.Instance.GetBattleData.zoneType);
        InitBattleUI(zoneNodeData);
        AudioManager.Instance.PlayBGM(zoneNodeData.zoneBGMList[Random.Range(0, zoneNodeData.zoneBGMList.Count)]);
        topSideUI.SetCurrentStateText(DataManager.Instance.GetBattleData.zoneType, area);
    }

    // 전투 UI 새로 설정하기
    public void InitBattleUI(ZoneNodeData zoneNodeData)
    {
        area = zoneNodeData.areaList[Random.Range(0, zoneNodeData.areaList.Count)];
        battleFieldBackground.sprite = area.area_background_spirte;
    }
}
