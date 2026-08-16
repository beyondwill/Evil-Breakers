using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocationRank
{
    S,
    A,
    B,
    C,
    D,
    E
}

[CreateAssetMenu(fileName = "Mission Info", menuName = "Main/Mission Info")]
public class MissionInfo : DataEntity
{
    // 변수
    public string location_name;                        // 지역 이름
    public string location_info;                        // 지역 정보
    public LocationRank location_rank;                  // 지역 랭크
    public Sprite location_image;                       // 지역 사진
    public List<string> essential_objects_list;         // 필수 목표 리스트
    public List<string> rewards_list;                   // 보상 리스트
}
