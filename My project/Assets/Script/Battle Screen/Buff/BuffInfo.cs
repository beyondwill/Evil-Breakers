using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "Buff/Buff Info")]
public class BuffInfo : ScriptableObject
{
    // 변수
    public string buff_name;                                // 버프명
    public int buff_duration;                               // 버프 지속 시간
    public bool duration_show_bool;                         // 버프 지속 시간 보여주기 여부
    public List<PassiveCondition> durationReduceList;       // 패시브 감소조건 리스트

    // 패시브 이벤트 리스트
    public List<PassiveEvent> passiveEventList;
}