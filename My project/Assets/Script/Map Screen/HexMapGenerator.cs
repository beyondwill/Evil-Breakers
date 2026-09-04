using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HexMapGenerator : MonoBehaviour
{
    public static HexMapGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // Grid
    // =========================================================

    [Header("Grid")]
    public float hexSize = 80f;


    // =========================================================
    // Random Offset
    // =========================================================

    [Header("Random Offset")]
    public bool useRandomOffset = true;

    public float randomOffsetPower = 40f;


    // =========================================================
    // Map Structure
    // =========================================================

    [Header("Map Structure")]
    public int targetNodeCount = 15;
    public int extraEdgeCount = 3;


    // =========================================================
    // Distance Rules
    // =========================================================

    [Header("Distance Rules")]
    public int lessLenFromBoss = 0;
    public int lessLenEliteBattle = 0;
    public int lessLenShop = 0;
    public int minShopDistance = 0;


    // =========================================================
    // Node Counts
    // =========================================================

    [Header("Node Counts")]
    public int normalBattleCount = 0;
    public int eliteBattleCount = 0;
    public int bossCount = 1;
    public int shopCount = 0;
    public int obstacleCount = 0;


    // =========================================================
    // Start
    // =========================================================

    public List<Vector2Int> startCandidates = new();


    // =========================================================
    // Map Data
    // =========================================================

    public HexMapDataSO HMD;

    [TextArea(8, 20)]
    public string mask;


    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]
    public Button nodePrefab;

    public Image edgePrefab;

    public Transform nodeLayer;

    public Transform edgeLayer;


    // =========================================================
    // Runtime Data
    // =========================================================

    public Dictionary<Vector2Int, HexNode> nodes =
        new();

    public HashSet<Vector2Int> activeCoords =
        new();

    private Dictionary<Vector2Int, Button> nodeToButton =
        new();

    private Dictionary<Vector2Int, List<Image>> nodeToEdges =
        new();

    private Vector2Int startCoord;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {

    }


    // =========================================================
    // HexMapDataSO → Generator 데이터 적용
    // =========================================================

    private void ApplyData(HexMapDataSO data)
    {
        targetNodeCount = data.targetNodeCount;
        extraEdgeCount = data.extraEdgeCount;

        lessLenFromBoss = data.lessLenFromBoss;
        lessLenEliteBattle = data.lessLenEliteBattle;
        lessLenShop = data.lessLenShop;

        minShopDistance = data.minShopDistance;

        normalBattleCount = data.normalBattleCount;
        eliteBattleCount = data.eliteBattleCount;
        bossCount = data.bossCount;

        mask = data.mask;

        shopCount = data.shopCount;
        obstacleCount = data.obstacleCount;
    }


    // =========================================================
    // 순수 맵 데이터 생성
    // =========================================================

    public MapData GenerateMapData(
        HexMapDataSO HMDS)
    {
        ApplyData(HMDS);

        bool success = false;

        for (int i = 0; i < 1000; i++)
        {
            if (GenerateOnce())
            {
                success = true;
                break;
            }
        }

        if (!success)
        {
            Debug.LogError("맵 생성 실패");
            return null;
        }


        MapData mapData =
            new MapData();

        mapData.hexMapDataSO =
            HMDS;

        mapData.SetMapData(
            new Dictionary<Vector2Int, HexNode>(nodes)
        );

        mapData.SetCurrentPoint(
            startCoord
        );


        return mapData;
    }


    // =========================================================
    // 맵 생성
    // =========================================================

    public void Generate()
    {
        // =====================================================
        // 현재 맵 설정 적용
        // =====================================================

        MapData mapData =
            DataManager.Instance
                .GetBattleData
                .map_data;


        if (mapData == null)
        {
            Debug.LogError(
                "MapData가 없습니다."
            );

            return;
        }


        if (mapData.hexMapDataSO == null)
        {
            Debug.LogError(
                "MapData.hexMapDataSO가 없습니다."
            );

            return;
        }


        ApplyData(
            mapData.hexMapDataSO
        );


        // =====================================================
        // 메인 화면 → 새로운 맵 생성
        // =====================================================

        if (
            DataManager.Instance
                .GetAllData
                .current_state ==
            CurrentState.MainScreen
        )
        {
            ClearUI();


            bool success = false;


            for (int i = 0; i < 1000; i++)
            {
                if (GenerateOnce())
                {
                    success = true;
                    break;
                }
            }


            if (!success)
            {
                Debug.LogError(
                    "맵 생성 실패"
                );

                return;
            }


            // =================================================
            // 생성된 맵 저장
            // =================================================

            mapData.SetMapData(
                new Dictionary<Vector2Int, HexNode>(
                    nodes
                )
            );


            mapData.SetCurrentPoint(
                startCoord
            );


            // =================================================
            // 시간 설정
            // =================================================

            DataManager.Instance
                .GetBattleData
                .SetTime(
                    nodes.Count * 3,
                    false
                );


            // =================================================
            // 공포도 초기화
            // =================================================

            DataManager.Instance
                .GetBattleData
                .SetHorror(0);


            // =================================================
            // 미션 초기화
            // =================================================

            mapData.missionObjectList.Clear();


            // =================================================
            // 일반 + 엘리트 몬스터 미션
            // =================================================

            int monsterCount =
                nodes.Values.Count(
                    n =>
                        n.type ==
                            HexNode.NodeType.Normal ||
                        n.type ==
                            HexNode.NodeType.Elite
                );


            if (monsterCount > 0)
            {
                mapData.AddMissionObject(
                    MissionObjectSort.KillMonsters,
                    monsterCount
                );
            }


            // =================================================
            // 보스 미션
            // =================================================

            int generatedBossCount =
                nodes.Values.Count(
                    n =>
                        n.type ==
                        HexNode.NodeType.Boss
                );


            if (generatedBossCount > 0)
            {
                mapData.AddMissionObject(
                    MissionObjectSort.KillBosses,
                    generatedBossCount
                );
            }


            // =================================================
            // 상태 변경
            // =================================================

            DataManager.Instance
                .GetAllData
                .current_state =
                CurrentState.BattleMap;


            Debug.Log(
                $"새 맵 생성 / " +
                $"일반+엘리트: {monsterCount} / " +
                $"보스: {generatedBossCount}"
            );
        }


        // =====================================================
        // 저장된 맵 사용
        // =====================================================

        else
        {
            Debug.Log(
                "저장된 맵 불러오기"
            );


            nodes =
                mapData.nodes;

            startCoord =
                mapData.current_point;


            Debug.Log(
                $"startCoord : {startCoord}"
            );

            Debug.Log(
                $"node count : {nodes.Count}"
            );
        }


        // =====================================================
        // UI 생성
        // =====================================================

        CreateUI();


        MapManager.Instance.SetMap(
            nodes,
            nodeToButton
        );


        MapManager.Instance.InitMap(
            startCoord
        );


        // =====================================================
        // 저장
        // =====================================================

        DataManager.Instance.SaveData();
    }


    // =========================================================
    // 맵 1회 생성
    // =========================================================

    private bool GenerateOnce()
    {
        ClearData();

        ParseMask();

        GenerateNodes();

        AssignBoss();

        AssignBattle();

        AssignShop();

        AssignObstacle();

        AddExtraEdges();

        return IsValid();
    }


    // =========================================================
    // 맵 유효성 검사
    // =========================================================

    private bool IsValid()
    {
        // 노드 개수
        if (nodes.Count < targetNodeCount)
            return false;


        // 최대 거리
        int maxDist =
            nodes.Values.Max(
                n => n.distance
            );


        if (maxDist < lessLenFromBoss)
            return false;


        // 보스 개수
        int generatedBossCount =
            nodes.Values.Count(
                n =>
                    n.type ==
                    HexNode.NodeType.Boss
            );


        if (generatedBossCount != bossCount)
            return false;


        // 상점 개수
        int generatedShopCount =
            nodes.Values.Count(
                n =>
                    n.type ==
                    HexNode.NodeType.Shop
            );


        if (generatedShopCount != shopCount)
            return false;


        return true;
    }


    // =========================================================
    // 오래된 데이터 제거
    // =========================================================

    private void ClearOld()
    {
        foreach (var node in nodes.Values)
        {
            node.links.Clear();
        }


        foreach (Transform child in nodeLayer)
        {
            Destroy(
                child.gameObject
            );
        }


        foreach (Transform child in edgeLayer)
        {
            Destroy(
                child.gameObject
            );
        }


        nodes.Clear();

        activeCoords.Clear();
    }


    // =========================================================
    // Mask 파싱
    // =========================================================

    private void ParseMask()
    {
        string[] lines =
            mask.Split('\n');


        for (int y = 0;
             y < lines.Length;
             y++)
        {
            string line =
                lines[y].Trim();


            for (int x = 0;
                 x < line.Length;
                 x++)
            {
                if (line[x] == '1')
                {
                    activeCoords.Add(
                        new Vector2Int(
                            x,
                            y
                        )
                    );
                }
            }
        }
    }


    // =========================================================
    // 노드 생성
    // =========================================================

    private void GenerateNodes()
    {
        Queue<HexNode> frontier =
            new();


        // =====================================================
        // 시작 좌표
        // =====================================================

        if (
            startCandidates != null &&
            startCandidates.Count > 0
        )
        {
            startCoord =
                startCandidates[
                    Random.Range(
                        0,
                        startCandidates.Count
                    )
                ];
        }
        else
        {
            if (activeCoords.Count == 0)
            {
                Debug.LogError(
                    "생성 가능한 노드 좌표가 없음 (mask 확인)"
                );

                return;
            }


            startCoord =
                activeCoords.ElementAt(
                    Random.Range(
                        0,
                        activeCoords.Count
                    )
                );
        }


        // =====================================================
        // 시작 노드
        // =====================================================

        HexNode start =
            CreateNode(
                startCoord,
                HexNode.NodeType.Start,
                0
            );


        frontier.Enqueue(
            start
        );


        // =====================================================
        // BFS 방식 노드 생성
        // =====================================================

        while (
            nodes.Count < targetNodeCount &&
            frontier.Count > 0
        )
        {
            HexNode current =
                frontier.Dequeue();


            var dirs =
                (current.coord.y % 2 == 0)
                    ? HexMath.EvenRowDirs
                    : HexMath.OddRowDirs;


            List<Vector2Int> shuffled =
                dirs
                    .OrderBy(
                        _ => Random.value
                    )
                    .ToList();


            foreach (
                Vector2Int dir
                in shuffled
            )
            {
                if (
                    nodes.Count >=
                    targetNodeCount
                )
                {
                    break;
                }


                Vector2Int next =
                    current.coord + dir;


                if (
                    !activeCoords.Contains(
                        next
                    )
                )
                {
                    continue;
                }


                if (
                    nodes.ContainsKey(
                        next
                    )
                )
                {
                    continue;
                }


                HexNode child =
                    CreateNode(
                        next,
                        HexNode.NodeType.Empty,
                        current.distance + 1
                    );


                Connect(
                    current,
                    child
                );


                frontier.Enqueue(
                    child
                );
            }
        }
    }


    // =========================================================
    // 노드 생성
    // =========================================================

    private HexNode CreateNode(
        Vector2Int coord,
        HexNode.NodeType type,
        int distance
    )
    {
        HexNode node =
            new();


        node.coord =
            coord;


        node.type =
            type;


        // =====================================================
        // Zone
        // =====================================================

        var zones =
            (HexNode.ZoneType[])
                System.Enum.GetValues(
                    typeof(HexNode.ZoneType)
                );


        node.zone =
            zones[
                UnityEngine.Random.Range(
                    0,
                    2
                )
            ];


        node.distance =
            distance;


        // =====================================================
        // UI 위치
        // =====================================================

        Vector2 basePos =
            HexMath.HexToUI(
                coord.x,
                coord.y,
                hexSize
            );


        node.originalPos =
            basePos;


        // =====================================================
        // 랜덤 위치
        // =====================================================

        if (useRandomOffset)
        {
            Vector2 finalPos =
                basePos;

            bool valid = false;

            int tryCount = 0;


            while (
                !valid &&
                tryCount < 20
            )
            {
                tryCount++;


                Vector2 offset =
                    Random.insideUnitCircle *
                    randomOffsetPower;


                finalPos =
                    basePos + offset;


                valid = true;


                foreach (
                    HexNode other
                    in nodes.Values
                )
                {
                    float dist =
                        Vector2.Distance(
                            finalPos,
                            other.uiPos
                        );


                    if (
                        dist <
                        hexSize * 0.7f
                    )
                    {
                        valid = false;
                        break;
                    }
                }
            }


            node.uiPos =
                finalPos;
        }
        else
        {
            node.uiPos =
                basePos;
        }


        nodes.Add(
            coord,
            node
        );


        return node;
    }


    // =========================================================
    // 노드 연결
    // =========================================================

    private void Connect(
        HexNode a,
        HexNode b
    )
    {
        if (!a.links.Contains(b))
            a.links.Add(b);


        if (!b.links.Contains(a))
            b.links.Add(a);
    }


    // =========================================================
    // 추가 Edge 생성
    // =========================================================

    private void AddExtraEdges()
    {
        List<HexNode> list =
            nodes.Values.ToList();


        int created = 0;

        int safety = 1000;


        while (
            created < extraEdgeCount &&
            safety > 0
        )
        {
            safety--;


            HexNode a =
                list[
                    Random.Range(
                        0,
                        list.Count
                    )
                ];


            List<HexNode> neighbors =
                GetNeighborNodes(a);


            if (neighbors.Count == 0)
                continue;


            HexNode b =
                neighbors[
                    Random.Range(
                        0,
                        neighbors.Count
                    )
                ];


            if (a.links.Contains(b))
                continue;


            if (
                a.type ==
                HexNode.NodeType.Boss
            )
                continue;


            if (
                b.type ==
                HexNode.NodeType.Boss
            )
                continue;


            if (a.links.Count >= 3)
                continue;


            if (b.links.Count >= 3)
                continue;


            Connect(
                a,
                b
            );


            created++;
        }
    }


    // =========================================================
    // 이웃 노드 가져오기
    // =========================================================

    private List<HexNode> GetNeighborNodes(
        HexNode node)
    {
        List<HexNode> result =
            new();


        bool isEvenRow =
            node.coord.y % 2 == 0;


        var dirs =
            isEvenRow
                ? HexMath.EvenRowDirs
                : HexMath.OddRowDirs;


        foreach (
            Vector2Int dir
            in dirs
        )
        {
            Vector2Int next =
                node.coord + dir;


            if (
                nodes.TryGetValue(
                    next,
                    out HexNode target
                )
            )
            {
                result.Add(
                    target
                );
            }
        }


        return result;
    }


    // =========================================================
    // 보스 배치
    // =========================================================

    private void AssignBoss()
    {
        int maxDist =
            nodes.Values.Max(
                n => n.distance
            );


        List<HexNode> farthest =
            nodes.Values
                .Where(
                    n =>
                        n.distance ==
                        maxDist
                )
                .OrderBy(
                    _ => Random.value
                )
                .ToList();


        // 실제 가능한 개수만큼
        int count =
            Mathf.Min(
                bossCount,
                farthest.Count
            );


        for (int i = 0; i < count; i++)
        {
            farthest[i].type =
                HexNode.NodeType.Boss;
        }
    }


    // =========================================================
    // 전투 배치
    // =========================================================

    private void AssignBattle()
    {
        var allNodes =
            nodes.Values
                .Where(
                    n =>
                        n.type !=
                        HexNode.NodeType.Boss
                )
                .ToList();


        // =====================================================
        // 엘리트
        // =====================================================

        var elitePool =
            allNodes
                .Where(
                    n =>
                        n.distance >=
                        lessLenEliteBattle
                )
                .ToList();


        var elites =
            elitePool
                .OrderBy(
                    _ => Random.value
                )
                .Take(
                    eliteBattleCount
                )
                .ToList();


        foreach (var n in elites)
        {
            n.type =
                HexNode.NodeType.Elite;
        }


        // =====================================================
        // 일반 전투
        // =====================================================

        var normalPool =
            allNodes
                .Except(elites)
                .Where(
                    n =>
                        n.distance > 0
                )
                .ToList();


        var normals =
            normalPool
                .OrderBy(
                    _ => Random.value
                )
                .Take(
                    normalBattleCount
                )
                .ToList();


        foreach (var n in normals)
        {
            n.type =
                HexNode.NodeType.Normal;
        }
    }


    // =========================================================
    // 상점 배치
    // =========================================================

    private void AssignShop()
    {
        var shopPool =
            nodes.Values
                .Where(
                    n =>
                        n.type ==
                        HexNode.NodeType.Empty
                )
                .Where(
                    n =>
                        n.distance >=
                        lessLenShop
                )
                .OrderBy(
                    _ => Random.value
                )
                .ToList();


        List<HexNode> shops =
            new();


        foreach (
            HexNode candidate
            in shopPool
        )
        {
            bool tooClose =
                shops.Any(
                    shop =>
                        HexMath.GetDistance(
                            shop.coord,
                            candidate.coord
                        ) <
                        minShopDistance
                );


            if (tooClose)
                continue;


            shops.Add(
                candidate
            );


            if (
                shops.Count >=
                shopCount
            )
            {
                break;
            }
        }


        foreach (HexNode n in shops)
        {
            n.type =
                HexNode.NodeType.Shop;
        }
    }


    // =========================================================
    // 장애물 설정
    // =========================================================

    private void AssignObstacle()
    {
        var obstaclePool =
            nodes.Values
                .Where(
                    n =>
                        n.type ==
                        HexNode.NodeType.Empty
                )
                .Where(
                    n =>
                        n.distance > 0
                )
                .OrderBy(
                    _ => Random.value
                )
                .ToList();


        var obstacles =
            obstaclePool
                .Take(
                    obstacleCount
                )
                .ToList();


        foreach (HexNode n in obstacles)
        {
            n.type =
                HexNode.NodeType.Event;
        }
    }


    // =========================================================
    // UI 생성
    // =========================================================

    private void CreateUI()
    {
        Vector2 centerOffset =
            GetCenterOffset();


        foreach (HexNode node in nodes.Values)
        {
            Button btn =
                Instantiate(
                    nodePrefab,
                    nodeLayer
                );


            nodeToButton[
                node.coord
            ] = btn;


            RectTransform rt =
                btn.GetComponent<RectTransform>();


            rt.anchoredPosition =
                node.uiPos -
                centerOffset;


            MapManager.Instance.BindNodeClick(
                node,
                btn
            );
        }


        HashSet<string> drawn =
            new();


        foreach (HexNode node in nodes.Values)
        {
            foreach (
                HexNode target
                in node.links
            )
            {
                string key =
                    node.coord.ToString() +
                    target.coord.ToString();


                string reverse =
                    target.coord.ToString() +
                    node.coord.ToString();


                if (
                    drawn.Contains(key) ||
                    drawn.Contains(reverse)
                )
                {
                    continue;
                }


                CreateEdge(
                    node,
                    target,
                    centerOffset
                );


                drawn.Add(key);
            }
        }
    }


    // =========================================================
    // Edge 생성
    // =========================================================

    private void CreateEdge(
        HexNode a,
        HexNode b,
        Vector2 centerOffset
    )
    {
        Image edge =
            Instantiate(
                edgePrefab,
                edgeLayer
            );


        // =====================================================
        // Node ↔ Edge 매핑
        // =====================================================

        if (
            !nodeToEdges.ContainsKey(
                a.coord
            )
        )
        {
            nodeToEdges[
                a.coord
            ] = new List<Image>();
        }


        if (
            !nodeToEdges.ContainsKey(
                b.coord
            )
        )
        {
            nodeToEdges[
                b.coord
            ] = new List<Image>();
        }


        nodeToEdges[
            a.coord
        ].Add(edge);


        nodeToEdges[
            b.coord
        ].Add(edge);


        // =====================================================
        // UI 좌표
        // =====================================================

        Vector2 aPos =
            a.uiPos -
            centerOffset;


        Vector2 bPos =
            b.uiPos -
            centerOffset;


        RectTransform rt =
            edge.GetComponent<RectTransform>();


        // 위치
        Vector2 mid =
            (aPos + bPos) *
            0.5f;


        rt.anchoredPosition =
            mid;


        // 길이
        float dist =
            Vector2.Distance(
                aPos,
                bPos
            );


        rt.sizeDelta =
            new Vector2(
                dist,
                8f
            );


        // 회전
        float angle =
            Mathf.Atan2(
                bPos.y - aPos.y,
                bPos.x - aPos.x
            ) *
            Mathf.Rad2Deg;


        rt.rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );
    }


    // =========================================================
    // 중심 Offset
    // =========================================================

    private Vector2 GetCenterOffset()
    {
        if (nodes.Count == 0)
            return Vector2.zero;


        Vector2 sum =
            Vector2.zero;


        foreach (HexNode node in nodes.Values)
        {
            sum += node.uiPos;
        }


        return sum /
               nodes.Count;
    }


    // =========================================================
    // 랜덤 Offset 토글
    // =========================================================

    public void ToggleRandomOffset()
    {
        useRandomOffset =
            !useRandomOffset;


        while (
            edgeLayer.childCount > 0
        )
        {
            DestroyImmediate(
                edgeLayer
                    .GetChild(0)
                    .gameObject
            );
        }


        nodeToEdges.Clear();


        ApplyOffset();
    }


    // =========================================================
    // Offset 적용
    // =========================================================

    private void ApplyOffset()
    {
        foreach (
            HexNode node
            in nodes.Values
        )
        {
            if (useRandomOffset)
            {
                Vector2 finalPos =
                    node.originalPos;


                bool valid = false;

                int tryCount = 0;


                while (
                    !valid &&
                    tryCount < 20
                )
                {
                    tryCount++;


                    Vector2 offset =
                        Random.insideUnitCircle *
                        randomOffsetPower;


                    finalPos =
                        node.originalPos +
                        offset;


                    valid = true;


                    foreach (
                        HexNode other
                        in nodes.Values
                    )
                    {
                        if (other == node)
                            continue;


                        float dist =
                            Vector2.Distance(
                                finalPos,
                                other.uiPos
                            );


                        if (
                            dist <
                            hexSize * 0.7f
                        )
                        {
                            valid = false;
                            break;
                        }
                    }
                }


                node.uiPos =
                    finalPos;
            }
            else
            {
                node.uiPos =
                    node.originalPos;
            }
        }


        RefreshUIPosition();
    }


    // =========================================================
    // UI 위치 갱신
    // =========================================================

    private void RefreshUIPosition()
    {
        Vector2 centerOffset =
            GetCenterOffset();


        foreach (
            var pair
            in nodeToButton
        )
        {
            Vector2Int coord =
                pair.Key;


            Button btn =
                pair.Value;


            if (
                !nodes.TryGetValue(
                    coord,
                    out HexNode node
                )
            )
            {
                continue;
            }


            RectTransform rt =
                btn.GetComponent<RectTransform>();


            rt.anchoredPosition =
                node.uiPos -
                centerOffset;
        }


        // 기존 Edge 제거
        while (
            edgeLayer.childCount > 0
        )
        {
            DestroyImmediate(
                edgeLayer
                    .GetChild(0)
                    .gameObject
            );
        }


        nodeToEdges.Clear();


        // Edge 다시 생성
        HashSet<string> drawn =
            new();


        foreach (HexNode node in nodes.Values)
        {
            foreach (
                HexNode target
                in node.links
            )
            {
                string key =
                    node.coord +
                    "-" +
                    target.coord;


                string reverse =
                    target.coord +
                    "-" +
                    node.coord;


                if (
                    drawn.Contains(key) ||
                    drawn.Contains(reverse)
                )
                {
                    continue;
                }


                CreateEdge(
                    node,
                    target,
                    centerOffset
                );


                drawn.Add(key);
            }
        }
    }


    // =========================================================
    // 전체 맵 삭제
    // =========================================================

    public void ClearMap()
    {
        while (
            nodeLayer.childCount > 0
        )
        {
            DestroyImmediate(
                nodeLayer
                    .GetChild(0)
                    .gameObject
            );
        }


        while (
            edgeLayer.childCount > 0
        )
        {
            DestroyImmediate(
                edgeLayer
                    .GetChild(0)
                    .gameObject
            );
        }


        nodes.Clear();

        activeCoords.Clear();

        nodeToButton.Clear();

        nodeToEdges.Clear();
    }


    // =========================================================
    // 데이터 삭제
    // =========================================================

    private void ClearData()
    {
        foreach (
            var node
            in nodes.Values
        )
        {
            node.links.Clear();
        }


        nodes.Clear();

        activeCoords.Clear();
    }


    // =========================================================
    // UI 삭제
    // =========================================================

    private void ClearUI()
    {
        foreach (
            Transform child
            in nodeLayer
        )
        {
            Destroy(
                child.gameObject
            );
        }


        foreach (
            Transform child
            in edgeLayer
        )
        {
            Destroy(
                child.gameObject
            );
        }


        nodeToButton.Clear();

        nodeToEdges.Clear();
    }
}