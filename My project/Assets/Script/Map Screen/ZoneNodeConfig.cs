using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneNodeConfig", menuName = "ScriptableObjects/ZoneNodeConfig")]
public class ZoneNodeConfig : ScriptableObject
{
    public List<ZoneNodeData> zoneNodeDataList;

    // 반환 타입 수정 (MissionUIData -> MapNodeData)
    public ZoneNodeData GetZoneNodeData(HexNode.ZoneType zone_type) => zoneNodeDataList.Find(x => x.zone_type == zone_type);
}