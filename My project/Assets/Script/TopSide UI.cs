using TMPro;
using UnityEngine;

public class TopSideUI : MonoBehaviour
{
    // ==========================================
    // 외부 요소
    // ==========================================

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI currentStateText;
    [SerializeField] private TextMeshProUGUI moneyCount;
    [SerializeField] private TextMeshProUGUI timeCount;

    [SerializeField] private ZoneNodeConfig zoneNodeConfig;

    [Header("시간 변경 텍스트")]
    [SerializeField] private MakingText makingText;


    // ==========================================
    // Unity
    // ==========================================

    private void OnEnable()
    {
        if (DataManager.Instance == null)
            return;

        if (DataManager.Instance.GetBattleData == null)
            return;

        // 시간 변경 이벤트 구독
        DataManager.Instance.GetBattleData.OnTimeChanged += OnTimeChanged;
    }


    private void OnDisable()
    {
        if (DataManager.Instance == null)
            return;

        if (DataManager.Instance.GetBattleData == null)
            return;

        // 시간 변경 이벤트 구독 해제
        DataManager.Instance.GetBattleData.OnTimeChanged -= OnTimeChanged;
    }


    private void Start()
    {
        SetLocationText();
        SetCurrentStateText();

        moneyCount.text =
            DataManager.Instance
                .GetAllData
                .main_data
                .money
                .ToString();

        // 현재 시간 표시
        RefreshTime();
    }


    // ==========================================
    // 지역 명 설정
    // ==========================================

    public void SetLocationText()
    {
        locationText.text =
            DataManager.Instance
                .GetBattleData
                .map_data
                .hexMapDataSO
                .location_name;
    }


    // ==========================================
    // 현재 상황
    // ==========================================

    public void SetCurrentStateText()
    {
        SetCurrentStateText(
            DataManager.Instance.GetBattleData.zoneType
        );
    }


    public void SetCurrentStateText(
        HexNode.ZoneType zone,
        Area area = null)
    {
        currentStateText.text =
            zoneNodeConfig
                .GetZoneNodeData(zone)
                .zone_name;

        if (area != null)
        {
            currentStateText.text +=
                " : " + area.area_name;
        }
    }


    // ==========================================
    // 시간
    // ==========================================

    // 현재 BattleData의 시간을 그대로 표시
    public void RefreshTime()
    {
        if (DataManager.Instance == null)
            return;

        SetTimeText(
            DataManager.Instance.GetBattleData.time
        );
    }


    public void SetTimeText(int time)
    {
        timeCount.text = time.ToString();
    }


    // ==========================================
    // 시간 변경 이벤트
    // ==========================================

    private void OnTimeChanged(
        int time,
        int changeAmount)
    {
        // 시간 숫자 갱신
        SetTimeText(time);

        // 실제 시간 변화 연출
        if (changeAmount != 0)
        {
            ShowTimeChange(changeAmount);
        }
    }


    // ==========================================
    // 시간 변경 텍스트
    // ==========================================

    public void ShowTimeChange(int amount)
    {
        if (makingText == null)
            return;

        makingText.TextInit(amount);
    }
}