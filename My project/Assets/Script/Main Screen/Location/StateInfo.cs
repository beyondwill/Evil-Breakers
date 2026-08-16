using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMusic
{
    public AudioClip map_music;                             // 맵 음악
}

[CreateAssetMenu(fileName = "Location Info", menuName = "Main/Location Info")]
public class StateInfo : ScriptableObject
{
    // 변수
    public string state_name;                               // 도 이름
    public List<LocationInfo> location_info_list;           // 지역 정보 리스트
    public StateMusic state_musics;                         // 도 음악
}