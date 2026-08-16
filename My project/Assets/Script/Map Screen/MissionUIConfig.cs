using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MissionUIData
{
    public MissionObjectSort sort;
    public string missionName; // 미션명
    public Sprite icon;        // 아이콘
}

[CreateAssetMenu(fileName = "MissionUIConfig", menuName = "ScriptableObjects/MissionUIConfig")]
public class MissionUIConfig : ScriptableObject
{
    // 변수
    public List<MissionUIData> missionUIDataList;                       // 미션 UI Data들
    public List<MissionObjectSort> completionConditionsList;            // 완료 조건 목록 (이걸 다 충족시켜야 게임 성공) 
    public List<MissionObjectSort> failConditionsList;                  // 실패 조건 목록 (이걸 미달하면 즉시 패배)

    public Color completionColor;                                       // 완료 색상
    public Color failColor;                                             // 실패 색상

    public MissionUIData GetMissionUIData(MissionObjectSort sort) => missionUIDataList.Find(x => x.sort == sort);

    // 자동 초기화
    private void OnValidate()
    {
        // 리스트가 null이면 초기화
        if (missionUIDataList == null) missionUIDataList = new List<MissionUIData>();

        // Enum의 모든 값을 순회하며 빠진 항목 추가
        foreach (MissionObjectSort sort in System.Enum.GetValues(typeof(MissionObjectSort)))
        {
            if (!missionUIDataList.Exists(x => x.sort == sort))
            {
                missionUIDataList.Add(new MissionUIData
                {
                    sort = sort,
                    missionName = sort.ToString() // 기본값을 Enum 이름으로 설정
                });
            }
        }

        // 만약 Enum에서 삭제된 값이 있다면 리스트에서도 제거
        missionUIDataList.RemoveAll(x => !System.Enum.IsDefined(typeof(MissionObjectSort), x.sort));
    }
}