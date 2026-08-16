using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "Relic/Relic Info")]
public class RelicInfo : DataEntity
{
    public string relic_name;                           // 유물 이름
    public RelicSort sort;                              // 유물 분류
    public Sprite relic_image;                          // 유물 이미지
    [TextArea]
    public string relic_script;                         // 유물 설명
    [TextArea]
    public string relic_story;                          // 유물 이야기
    public OwnershipScope ownerShipScope;               // 캐릭터 소지 가능 여부
    public List<PassiveEvent> relicPassiveEventList;    // 유물 패시브 이벤트 리스트
}