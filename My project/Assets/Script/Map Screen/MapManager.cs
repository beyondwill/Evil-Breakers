using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    void Awake()
    {
        Instance = this;
    }

    private static readonly string[] KeyLabels = new string[] { "D", "E", "W", "A", "Z", "X" };

    // 임시
    [SerializeField] private EventInfo testEvent;
    [SerializeField] private EventInfo testEvent2;

    // 외부 요소
    [SerializeField] private HexMapGenerator HexMapGenerator;
    [SerializeField] private IrisTransition irisTransition;
    [SerializeField] private TopSideUI topSideUI;
    [SerializeField] private RewardManager rewardManager;
    [SerializeField] private Inventory bag;

    [SerializeField] private BattleResult battle_result;
    [SerializeField] private AudioClip map_music;
    [SerializeField] private RectTransform content_RT;
    [SerializeField] private ScrollRect scrollRect;             // 스크롤 렉트
    [SerializeField] private MakingText timeText;

    [SerializeField] private int max_size;
    [SerializeField] private int min_size;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private MapData mapData;

    [SerializeField] private GameObject missionObjectBoxPrefab;         // 미션 오브젝트 박스 프리팹
    [SerializeField] private Transform missionInfoVLG;                  // 미션 정보 버티컬 레이아웃 그룹
    [SerializeField] private List<MissionObject> missionObjectList;     // 미션 오브젝트 리스트
    [SerializeField] private List<InfoBox> infoBoxList;                 // 정보 박스 리스트

    [SerializeField] private Button missionEndButton;                   // 미션 엔드 버튼


    public Dictionary<Vector2Int, HexNode> nodes = new();
    public Dictionary<Vector2Int, Button> nodeToButton = new();

    public Vector2Int current_point;
    private bool isMoving = false;

    void Start()
    {
        AudioManager.Instance.PlayBGM(map_music);
        HexMapGenerator.Generate();
        RestoreRevealedNodes();
        current_point = DataManager.Instance.GetBattleData.map_data.current_point;
        mapData = DataManager.Instance.GetBattleData.map_data;
        ApplyCurrentNode();
        RefreshSelectableNodes();
        InitMissionInfos();
        CheckMissionInfos(0f);
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            ZoomMap(scroll);
        }

        string input = Input.inputString.ToUpper();

        if (!string.IsNullOrEmpty(input))
        {
            if (isMoving)
                return;

            foreach (string key in KeyLabels)
            {
                if (input == key)
                {
                    MoveByKey(input);
                }
            }
        }
    }

    // =========================
    // MAP INIT
    // =========================

    public void InitMap(Vector2Int start)
    {
        current_point = start;

        //SaveBaseColors();
        ApplyAllNodeColors();

        ApplyCurrentNode();

        RefreshSelectableNodes();

        CenterOnNode(nodeToButton[current_point]);
    }

    private void RestoreRevealedNodes()
    {
        foreach (var pair in nodes)
        {
            HexNode node = pair.Value;

            if (!node.isRevealed)
                continue;

            if (nodeToButton.TryGetValue(node.coord, out Button btn))
            {
                btn.GetComponent<NodeButton>().RevealButtonInfo(0f);
            }
        }
    }

    public void SetMap(Dictionary<Vector2Int, HexNode> newNodes, Dictionary<Vector2Int, Button> newNodeToButtons)
    {
        nodes = newNodes;
        nodeToButton = newNodeToButtons;
    }

    private void ApplyAllNodeColors()
    {
        foreach (var pair in nodeToButton)
        {
            Vector2Int coord = pair.Key;
            Button btn = pair.Value;

            if (nodes.TryGetValue(coord, out HexNode node))
            {
                btn.GetComponent<NodeButton>().ChangeButtonColor(node.type, node.isVisited, 0f);
                btn.GetComponent<NodeButton>().SetZoneImage(node.zone);
            }
        }
    }

    // =========================
    // MOVEMENT
    // =========================

    // 키보드로 움직임
    public void MoveByKey(string k)
    {
        HexNode current = nodes[current_point];

        bool isEvenRow = (current.coord.y % 2 == 0);
        Vector2Int[] currentDirs = isEvenRow ? HexMath.EvenRowDirs : HexMath.OddRowDirs;

        // 눌린 키가 어느 방향인지 찾기
        int dirIndex = System.Array.IndexOf(KeyLabels, k);

        if (dirIndex < 0)
            return;

        Vector2Int targetCoord = current.coord + currentDirs[dirIndex];

        // 해당 위치에 노드가 있는지 확인
        if (!nodes.TryGetValue(targetCoord, out HexNode targetNode))
            return;

        // 현재 노드와 실제로 연결되어 있는지 확인
        if (!current.links.Contains(targetNode))
            return;

        MoveToNode(targetNode);
    }

    public void MoveToNode(HexNode target)
    {
        if (isMoving)
            return;

        DataManager.Instance.GetBattleData.ReduceTime(1);
        timeText.TextInit(-1);
        isMoving = true;

        HexNode prev = nodes[current_point];

        foreach (var btn in nodeToButton.Values)
        {
            btn.GetComponent<NodeButton>().HideKeyBox();
            btn.interactable = false;
        }


        current_point = target.coord;

        DataManager.Instance.GetBattleData.map_data.SetCurrentPoint(current_point);


        // 이전 위치 얼굴 제거
        if (nodeToButton.ContainsKey(prev.coord))
        {
            nodeToButton[prev.coord]
                .GetComponent<NodeButton>()
                .FaceHide();
        }


        DOVirtual.DelayedCall(1f, () =>
        {
            bool isBattleNode =
                target.type == HexNode.NodeType.Normal ||
                target.type == HexNode.NodeType.Elite;


            // 처음 들어가는 전투지역
            if (isBattleNode && !target.isVisited)
            {
                isMoving = false;
                return;
            }


            RefreshSelectableNodes(true);

            ApplyCurrentNode();


            isMoving = false;
        });


        DataManager.Instance.GetBattleData.nodeType = target.type;
        DataManager.Instance.GetBattleData.zoneType = target.zone;

        topSideUI.SetCurrentStateText(target.zone);

        DataManager.Instance.SaveData();


        EnterNode(target);
    }

    public void EnterNode(HexNode target)
    {
        Button btn = nodeToButton[target.coord];

        bool firstEnter = !target.isVisited;

        Sequence seq = DOTween.Sequence();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content_RT);

        CenterOnNode(btn);


        if (firstEnter)
        {
            if (target.type == HexNode.NodeType.Normal ||
                target.type == HexNode.NodeType.Elite)
            {
                seq.AppendInterval(1f);

                seq.AppendCallback(() =>
                {
                    irisTransition.CloseAtScreenPosition(
                        RectTransformUtility.WorldToScreenPoint(
                            null,
                            btn.transform.position
                        )
                    );
                });

                seq.AppendInterval(1f);
            }
            else
            {
                seq.AppendInterval(0.5f);
            }


            seq.AppendCallback(() =>
            {
                NodeSelect(target);
            });
        }
    }

    public void NodeSelect(HexNode target)
    {
        switch (target.type)
        {
            case HexNode.NodeType.Normal:
                UpdateMissionInfos(MissionObjectSort.KillMonsters, 1, false);
                target.isVisited = true;
                BattleStart();
                break;


            case HexNode.NodeType.Elite:
                UpdateMissionInfos(MissionObjectSort.KillMonsters, 1, false);
                target.isVisited = true;
                BattleStart();
                break;


            case HexNode.NodeType.Event:
                EventManager.Instance.ShowEvent(testEvent);
                ConversationManager.Instance.StartConversation(ConversationManager.Instance.conver2);
                target.isVisited = true;
                break;


            case HexNode.NodeType.Boss:
                EventManager.Instance.ShowEvent(testEvent2);
                break;

            case HexNode.NodeType.Shop:
                target.isVisited = true;
                break;
        }
    }

    public Tween CenterOnNode(Button nodeBtn)
    {
        RectTransform target = nodeBtn.GetComponent<RectTransform>();
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        Canvas.ForceUpdateCanvases();

        // 1. viewport 기준 target 위치 (로컬)
        Vector2 viewportLocalPos =
            (Vector2)viewport.InverseTransformPoint(target.position);

        // 2. viewport 중심
        Vector2 center = viewport.rect.center;

        // 3. offset 계산
        Vector2 offset = center - viewportLocalPos;

        // 4. content 이동 (핵심)
        Vector2 targetPos = content.anchoredPosition + offset;

        return content.DOAnchorPos(targetPos, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    // 현재 노드 색상 적용
    void ApplyCurrentNode()
    {
        HexNode current = nodes[current_point];

        if (!nodeToButton.ContainsKey(current.coord))
            return;

        NodeButton currentNodeButton =
            nodeToButton[current.coord].GetComponent<NodeButton>();


        // 이미 방문한 지역만 얼굴 표시
        if (current.isVisited || current.type == HexNode.NodeType.Start)
        {
            currentNodeButton.FaceShow();
        }
        else
        {
            currentNodeButton.FaceHide();
        }


        currentNodeButton.RevealButtonInfo();

        nodeToButton[current.coord].interactable = false;

        currentNodeButton.ChangeButtonColor(
            current.type,
            current.isVisited
        );
    }

    // =========================
    // SELECTABLE NODES
    // =========================

    public void RefreshSelectableNodes(bool linkchange = true)
    {
        HexNode current = nodes[current_point];

        // 현재 위치는 항상 공개
        current.isRevealed = true;

        // Start 제외하고 점령 처리
        if (!current.isVisited)
        {
            if (current.type != HexNode.NodeType.Start)
            {
                current.isVisited = true;

                if (current.type != HexNode.NodeType.Normal &&
                    current.type != HexNode.NodeType.Elite)
                {
                    UpdateMissionInfos(
                        MissionObjectSort.AreaControl,
                        1,
                        true
                    );
                }
                else
                {
                    UpdateMissionInfos(
                        MissionObjectSort.AreaControl,
                        1,
                        false
                    );
                }
            }
        }

        // 행(row)이 홀수인지 짝수인지 확인
        bool isEvenRow = (current.coord.y % 2 == 0);
        Vector2Int[] currentDirs = isEvenRow ? HexMath.EvenRowDirs : HexMath.OddRowDirs;

        if (linkchange)
        {
            foreach (HexNode linked in current.links)
            {
                if (nodeToButton.TryGetValue(linked.coord, out Button btn))
                {
                    Vector2Int diff = linked.coord - current.coord;

                    // 수정된 GetDirectionIndex 호출
                    int index = GetDirectionIndex(diff, currentDirs);

                    if (index != -1)
                    {
                        btn.GetComponent<NodeButton>().ShowKeyBox(KeyLabels[index]);
                    }

                    btn.GetComponent<NodeButton>().RevealButtonInfo();
                    btn.interactable = true;
                    linked.isRevealed = true;
                }
            } 
        }
    }

    // 매개변수로 현재 행에 맞는 방향 배열을 받도록 수정
    private int GetDirectionIndex(Vector2Int diff, Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i] == diff) return i;
        }
        return -1;
    }

    // =========================
    // BUTTON CLICK HOOK
    // =========================

    public void BindNodeClick(HexNode node, Button btn)
    {
        btn.onClick.RemoveAllListeners();

        btn.onClick.AddListener(() =>
        {
            MoveToNode(node);
        });
    }

    // =========================
    // BATTLE / UI
    // =========================

    // 테스트용
    public EnemyCharacterInfo enemy1;
    public EnemyCharacterInfo enemy2;

    public void BattleStart()
    {
        HexNode currentNode = nodes[current_point];
        DataManager.Instance.GetBattleData.enemyCharacterList.Clear();

        if (currentNode.zone == HexNode.ZoneType.Street)
        {
            if (currentNode.type == HexNode.NodeType.Normal)
            {
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy1);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy1);
            }

            if (currentNode.type == HexNode.NodeType.Elite)
            {
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy1);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy1);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy1);
            }
        }

        else if (currentNode.zone == HexNode.ZoneType.Subway)
        {
            if (currentNode.type == HexNode.NodeType.Normal)
            {
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy2);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy2);
            }

            if (currentNode.type == HexNode.NodeType.Elite)
            {
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy2);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy2);
                DataManager.Instance.GetBattleData.enemyCharacterList.Add(enemy2);
            }
        }

        DataManager.Instance.GetAllData.current_state = CurrentState.BattleBegin;
        DataManager.Instance.SaveData();
        //LoadingData.next_scene = "Battle Scene";
        //SceneManager.LoadScene("Loading Scene");
        SceneManager.LoadScene("Battle Scene");
    }

    public void BattleResult()
    {
        battle_result.gameObject.SetActive(true);
        DataManager.Instance.GetAllData.SetCurrentState(CurrentState.MainScreen);
        battle_result.ShowBattleResult(true);
    }


    public void BackToMainScene()
    {
        Destroy(DataManager.Instance.gameObject);
        //LoadingData.next_scene = "Main Scene";
        //SceneManager.LoadScene("Loading Scene");
        SceneManager.LoadScene("Main Scene");
    }

    // =========================
    // ZOOM
    // =========================

    private void ZoomMap(float direction)
    {
        float currentScale = content_RT.localScale.x;

        float targetScale = currentScale + direction * zoomSpeed;
        targetScale = Mathf.Clamp(targetScale, min_size, max_size);

        content_RT.DOScale(targetScale, 0.2f).SetEase(Ease.OutQuad);
    }

    // 미션 정보 업데이트
    public void InitMissionInfos()
    {
        for (int i = 0; i < mapData.missionObjectList.Count; i++)
        {
            MissionObject missionObject = mapData.missionObjectList[i];
            GameObject infoBox = Instantiate(missionObjectBoxPrefab, missionInfoVLG);

            InfoBox prefabInfoBox = infoBox.GetComponent<InfoBox>();
            prefabInfoBox.IconShowInit(missionObject.missionObjectSort);
            prefabInfoBox.CountInit(missionObject.need_count);
            prefabInfoBox.InfoUpdate(missionObject.current_count);

            // 추가하기
            missionObjectList.Add(missionObject);
            infoBoxList.Add(prefabInfoBox);
        }
    }

    // 미션 정보 업데이트
    public void UpdateMissionInfos(MissionObjectSort sort, int count = 1, bool check = true)
    {
        int index = missionObjectList.FindIndex(x => x.missionObjectSort == sort);
        if (index == -1) return;
        missionObjectList[index].current_count += count;
        int current_count = missionObjectList[index].current_count;

        DataManager.Instance.SaveData();

        if (check)
        {
            infoBoxList[index].InfoUpdate(current_count);
            CheckMissionInfos();
        }
    }

    // 미션 정보 총 확인
    public void CheckMissionInfos(float time = 2f)
    {
        // 하나라도 완료 안 된 미션이 있으면 체크 중단
        for (int i = 0; i < missionObjectList.Count; i++)
        {
            if (!missionObjectList[i].IsComplete) return;
        }

        // 모든 미션이 완료되었다면 1초 뒤 BattleResult 호출
        DOVirtual.DelayedCall(1f, () =>
        {
            SetupAndShowMissionEndButton(time);
        });
    }

    public void SetupAndShowMissionEndButton(float time = 2f)
    {
        // 이미 켜져 있으면: 아무것도 안 하기
        if (missionEndButton.gameObject.activeSelf) return;

        // 맨 아래로 버튼 보내기
        missionEndButton.gameObject.SetActive(true);
        missionEndButton.transform.SetAsLastSibling();

        CanvasGroup cg = missionEndButton.GetComponent<CanvasGroup>();
        if (cg == null) cg = missionEndButton.gameObject.AddComponent<CanvasGroup>();

        // 투명도 0으로 초기화 후 2초 동안 투명 -> 불투명으로 서서히 드러나게
        cg.alpha = 0f;
        cg.DOFade(1f, time);
    }
}