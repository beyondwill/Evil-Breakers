using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapNodeData
{
    public HexNode.NodeType type;
    public string zone_name;        // 구역명
    public Color zone_color;        // 구역 색
}

[CreateAssetMenu(fileName = "MapNodeConfig", menuName = "ScriptableObjects/MapNodeConfig")]
public class MapNodeConfig : ScriptableObject
{
    public List<MapNodeData> mapNodeDataList;

    public Color isVisitedColor;

    // 반환 타입 수정 (MissionUIData -> MapNodeData)
    public MapNodeData GetMapNodeData(HexNode.NodeType type) => mapNodeDataList.Find(x => x.type == type);

    private void OnValidate()
    {
        // 1. 리스트 초기화 체크
        if (mapNodeDataList == null) mapNodeDataList = new List<MapNodeData>();

        // 2. HexNode.NodeType의 모든 값을 순회
        foreach (HexNode.NodeType type in System.Enum.GetValues(typeof(HexNode.NodeType)))
        {
            // 리스트에 해당 타입이 없으면 추가
            if (!mapNodeDataList.Exists(x => x.type == type))
            {
                mapNodeDataList.Add(new MapNodeData
                {
                    type = type,
                    zone_name = type.ToString() // 기본값을 Enum 이름으로 설정
                });
            }
        }

        // 3. Enum에서 삭제된 값이 있다면 리스트에서도 제거
        mapNodeDataList.RemoveAll(x => !System.Enum.IsDefined(typeof(HexNode.NodeType), x.type));
    }
}