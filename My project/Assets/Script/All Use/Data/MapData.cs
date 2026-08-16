using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapSaveData
{
    public string mapDataId;

    public Vector2Int current_point;
    public List<SerializableHexNode> nodes = new();
    public List<MissionObject> missionObjectList = new();
}

[System.Serializable]
public class SerializableHexNode
{
    public Vector2Int coord;
    public float uiX, uiY, orgX, orgY;
    public int distance;
    public HexNode.NodeType node_type;
    public HexNode.ZoneType zone_type;
    public bool isVisited;
    public bool isRevealed;

    public List<Vector2Int> linkedCoords = new(); // 노드 연결 정보를 저장할 리스트

    public SerializableHexNode() { }

    public SerializableHexNode(Vector2Int key, HexNode node)
    {
        coord = key;
        uiX = node.uiPos.x; uiY = node.uiPos.y;
        orgX = node.originalPos.x; orgY = node.originalPos.y;
        distance = node.distance;
        node_type = node.type;
        zone_type = node.zone;
        isVisited = node.isVisited;
        isRevealed = node.isRevealed;

        // 연결된 노드들의 좌표만 추출하여 저장
        if (node.links != null)
        {
            foreach (var link in node.links)
            {
                linkedCoords.Add(link.coord);
            }
        }
    }

    public HexNode ToHexNode()
    {
        return new HexNode
        {
            coord = coord,
            uiPos = new Vector2(uiX, uiY),
            originalPos = new Vector2(orgX, orgY),
            distance = distance,
            zone = zone_type,
            type = node_type,
            isVisited = isVisited,
            isRevealed = isRevealed,

            // links null 방지
            links = new List<HexNode>()
        };
    }
}

// 미션 오브젝트 분류
public enum MissionObjectSort
{
    KillMonsters,       // 몬스터 처치
    KillBosses,         // 보스 처치
    AreaControl,        // 지역 통제
    CharacterCount      // 캐릭터 수
}

[System.Serializable]
// 미션 오브젝트
public class MissionObject
{
    public MissionObjectSort missionObjectSort;         // 미션 분류
    public int current_count;                           // 현재 카운트
    public int need_count;                              // 필요 카운트

    // 조건 충족 확인
    public bool IsComplete => current_count >= need_count;
}

[System.Serializable]
public class MapData
{
    public HexMapDataSO hexMapDataSO;

    [JsonIgnore]
    public Dictionary<Vector2Int, HexNode> nodes = new();

    public Vector2Int current_point;
    public List<MissionObject> missionObjectList = new List<MissionObject>();

    // 기존에 쓰던 함수들 그대로 유지
    public void SetMapData(Dictionary<Vector2Int, HexNode> newNodes) => nodes = newNodes;
    public void SetCurrentPoint(Vector2Int point) => current_point = point;

    public HexNode GetNode(Vector2Int point)
    {
        nodes.TryGetValue(point, out HexNode node);
        return node;
    }

    public MapSaveData GetSaveData()
    {
        var save = new MapSaveData
        {
            current_point = this.current_point,
            missionObjectList = new List<MissionObject>(this.missionObjectList),

            // SO 저장 (ID 기반)
            mapDataId = hexMapDataSO != null ? hexMapDataSO.data_code : ""
        };

        foreach (var pair in nodes)
        {
            save.nodes.Add(new SerializableHexNode(pair.Key, pair.Value));
        }

        return save;
    }

    public void ApplySaveData(MapSaveData data)
    {
        current_point = data.current_point;
        missionObjectList = new List<MissionObject>(data.missionObjectList);

        Debug.Log("으앙!");

        if (!string.IsNullOrEmpty(data.mapDataId))
        {
            Debug.Log(data.mapDataId);

            hexMapDataSO = TableManager.Instance.MAP.Get(data.mapDataId);

            if (hexMapDataSO == null)
            {
                Debug.LogError($"MapDataSO not found: {data.mapDataId}");
            }
        }

        nodes.Clear();

        // 1단계: 노드 생성
        foreach (var sNode in data.nodes)
        {
            nodes[sNode.coord] = sNode.ToHexNode();
        }

        // 2단계: 링크 복구
        foreach (var sNode in data.nodes)
        {
            var node = nodes[sNode.coord];

            foreach (var linkCoord in sNode.linkedCoords)
            {
                if (nodes.TryGetValue(linkCoord, out var linkedNode))
                {
                    node.links.Add(linkedNode);
                }
            }
        }
    }

    // 미션 오브젝트 업데이트 하기
    public void MissionObjectUpdate(MissionObjectSort sort, int count = 1)
    {
        var mission = missionObjectList.Find(x => x.missionObjectSort == sort);

        // 찾은 미션이 있을 경우에만 업데이트
        if (mission != null)
        {
            mission.current_count += count;
        }
    }

    // 미션 오브젝트 넘겨주기
    public MissionObject GetMissionObject(MissionObjectSort sort)
    {
        var mission = missionObjectList.Find(x => x.missionObjectSort == sort);

        // 없는 경우 에러 메시지
        if (mission == null)
        {
            Debug.LogError($"[MapData] MissionObjectSort.{sort}에 해당하는 미션을 찾을 수 없습니다.");
        }

        return mission;
    }

    public void AddMissionObject(
    MissionObjectSort sort,
    int needCount)
    {
        missionObjectList.Add(new MissionObject
        {
            missionObjectSort = sort,
            current_count = 0,
            need_count = needCount
        });
    }

    public bool IsAllMissionComplete
    {
        get
        {
            foreach (MissionObject mission in missionObjectList)
            {
                if (!mission.IsComplete)
                    return false;
            }

            return true;
        }
    }
}