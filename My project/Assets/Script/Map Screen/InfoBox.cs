using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoBox : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private Image box;                             // 박스
    [SerializeField] private Image icon;                            // 아이콘
    [SerializeField] private TextMeshProUGUI countText;             // 개수 체크
    [SerializeField] private MissionUIConfig missionUIConfig;       // 미션 UI
    [SerializeField] private GameObject checkImage;                 // 체크 이미지

    // 변수
    private int need_count;                         // 필요 카운트
    private bool is_completion = true;              // 필수적인가?
    [SerializeField] private Color completeColor;   // 완료 색상

    // 아이콘 보여주기
    public void IconShowInit(MissionObjectSort sort)
    {
        icon.sprite = missionUIConfig.GetMissionUIData(sort).icon;
        box.color = missionUIConfig.completionConditionsList.Contains(sort) ? missionUIConfig.completionColor : missionUIConfig.failColor;
        is_completion = missionUIConfig.completionConditionsList.Contains(sort);
    }

    // 필요 카운트 기억하기
    public void CountInit(int need_count)
    {
        this.need_count = need_count;
    }

    // 카운트 정보 업데이트
    public void InfoUpdate(int info)
    {
        if (info < need_count)
        {
            countText.text = "<color=red>" + info + "</color>";
        }

        // 필수 조건이라면: 불충족 시 빨간색, 충족 시 초록색
        else if (is_completion)
        {
            countText.text = "<color=green>" + info + "</color>";
            icon.color = completeColor;
            checkImage.SetActive(true);
        }

        else
        {
            countText.text = "<color=blue>" + info + "</color>";
        }

        countText.text += " / " + need_count;
    }
}
