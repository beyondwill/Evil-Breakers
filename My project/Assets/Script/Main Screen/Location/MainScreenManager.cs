using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenManager : MonoBehaviour
{
    // ================================
    // 인스턴스화
    // ================================

    public static MainScreenManager Instance;


    // ================================
    // 외부 요소
    // ================================

    [SerializeField] private MissionInfoManager missionInfoManager;
    [SerializeField] private MissionItemManager missionItemManager;
    [SerializeField] private List<LocationInfo> location_info_list;

    [SerializeField] private GameObject mission_prefab;
    [SerializeField] private GameObject needMoreCharacters;
    [SerializeField] private GameObject mission_content;
    [SerializeField] private AudioClip main_screen_music;

    [SerializeField] private List<TextMeshProUGUI> agent_count_list;
    [SerializeField] private List<GameObject> closeGameObjects;

    [SerializeField] private GameObject mapBox;
    [SerializeField] private GameObject stateBox;
    [SerializeField] private GameObject nationBox;
    [SerializeField] private GameObject manageBox;


    // ================================
    // 지역 버튼
    // ================================

    [SerializeField] private List<Button> locationButtonList;


    // ================================
    // Enter 버튼
    // ================================

    [SerializeField] private Button enterButton;


    // ================================
    // 현재 선택된 지역
    // ================================

    public int index = -1;


    // ================================
    // Enter 클릭으로 인한 Deselected인지
    // ================================

    private bool isEnterClicked = false;


    // ================================
    // Awake
    // ================================

    private void Awake()
    {
        Instance = this;
    }


    // ================================
    // Start
    // ================================

    private void Start()
    {
        AudioManager.Instance.PlayBGM(main_screen_music);


        // 처음에는 지역 선택 안 됨
        index = -1;


        // 처음에는 Enter 버튼 비활성화
        enterButton.interactable = false;


        // ================================
        // 맵 데이터 생성
        // ================================

        if (DataManager.Instance.GetAllData.main_data.mapDataList.Count != 3)
        {
            foreach (LocationInfo LI in location_info_list)
            {
                DataManager.Instance.GetAllData.main_data.mapDataList.Add(
                    HexMapGenerator.Instance.GenerateMapData(LI.HMDS)
                );
            }
        }


        // ================================
        // 현재 상태 설정
        // ================================

        DataManager.Instance.GetAllData.SetCurrentState(
            CurrentState.MainScreen
        );
    }


    // ================================
    // Update
    // ================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject closeGameObject in closeGameObjects)
            {
                closeGameObject.SetActive(false);
            }

            if (manageBox.activeSelf)
            {
                manageBox.SetActive(false);
            }
            else
            {
                if (nationBox.activeSelf)
                {
                    nationBox.SetActive(false);
                    stateBox.SetActive(true);
                }
                else
                {
                    if (mapBox.activeSelf)
                    {
                        mapBox.SetActive(false);
                    }
                }
            }
        }


        // ================================
        // 마우스 클릭 확인
        // ================================

        if (Input.GetMouseButtonDown(0))
        {
            CheckLocationClick();
        }
    }


    // ============================================================
    // 지역 클릭 확인
    // ============================================================

    private void CheckLocationClick()
    {
        if (index < 0)
        {
            return;
        }


        // ================================
        // PointerEventData
        // ================================

        PointerEventData pointerEventData =
            new PointerEventData(EventSystem.current);

        pointerEventData.position = Input.mousePosition;


        // ================================
        // UI Raycast
        // ================================

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerEventData,
            results
        );


        Debug.Log(
            $"[Check] Raycast 결과 개수 = {results.Count}"
        );


        // ================================
        // 클릭한 UI 확인
        // ================================

        foreach (RaycastResult result in results)
        {
            GameObject clickedObject = result.gameObject;


            Debug.Log(
                $"[Check] Raycast Object = {clickedObject.name}"
            );


            Button clickedButton =
                clickedObject.GetComponentInParent<Button>();


            if (clickedButton == null)
            {
                continue;
            }


            Debug.Log(
                $"[Check] Button 발견 = {clickedButton.gameObject.name}"
            );


            // ====================================================
            // Enter 버튼
            // ====================================================

            if (clickedButton == enterButton)
            {
                Debug.Log(
                    "[Check] Enter 버튼 클릭"
                );

                isEnterClicked = true;

                return;
            }


            // ====================================================
            // 지역 버튼
            // ====================================================

            if (locationButtonList.Contains(clickedButton))
            {
                Debug.Log(
                    $"[Check] 지역 버튼 클릭 → 선택 유지 : {clickedButton.gameObject.name}"
                );

                return;
            }
        }


        // ========================================================
        // 그 외의 곳 클릭
        // ========================================================

        Debug.Log(
            "[Check] 지역/Enter 버튼이 아닌 곳 클릭 → Deselected"
        );

        DeselectedLocation();
    }


    // ============================================================
    // 지역 Selected
    // ============================================================

    public void SelectedLocation(int selectedIndex)
    {
        Debug.Log(
            $"[1] SelectedLocation 호출됨 / selectedIndex = {selectedIndex}"
        );


        // ================================
        // 인덱스 검사
        // ================================

        if (selectedIndex < 0 ||
            selectedIndex >= locationButtonList.Count)
        {
            Debug.Log(
                "[1] 잘못된 지역 인덱스"
            );

            return;
        }


        // ================================
        // 선택 지역 저장
        // ================================

        index = selectedIndex;


        Debug.Log(
            $"[2] index 변경됨 = {index}"
        );


        // ================================
        // Enter 활성화
        // ================================

        enterButton.interactable = true;


        Debug.Log(
            "[2] Enter 버튼 활성화"
        );
    }


    // ============================================================
    // 지역 Deselected
    // ============================================================

    public void DeselectedLocation()
    {
        Debug.Log(
            $"[3] DeselectedLocation 호출됨 / index = {index}"
        );


        // ========================================================
        // Enter 버튼을 눌러서 발생한 Deselected
        // ========================================================

        if (isEnterClicked)
        {
            Debug.Log(
                "[3] Enter 클릭으로 발생한 Deselected → 무시"
            );

            isEnterClicked = false;

            return;
        }


        // ========================================================
        // 일반적인 Deselected
        // ========================================================

        enterButton.interactable = false;


        Debug.Log(
            $"[4] 일반 Deselected → index 유지 = {index}"
        );

        Debug.Log(
            "[4] Enter 버튼 비활성화"
        );
    }


    // ============================================================
    // 전투 개시
    // ============================================================
    // 미션창의 BattleBegin 버튼 OnClick에서 호출
    // ============================================================

    public void BattleBegin()
    {
        Debug.Log(
            $"[5] BattleBegin 호출 / index = {index}"
        );


        // ================================
        // 지역 선택 여부 확인
        // ================================

        if (index < 0 ||
            index >= location_info_list.Count)
        {
            Debug.Log(
                $"[6] 지역 선택 안 됨 / index = {index}"
            );

            return;
        }


        Debug.Log(
            $"[7] 정상적인 지역 선택 상태 / index = {index}"
        );


        // ================================
        // 캐릭터 확인
        // ================================

        if (missionInfoManager.GetSelectedCharacters().Count == 0)
        {
            Debug.Log(
                "[BattleBegin] 선택된 캐릭터가 없습니다."
            );


            if (!needMoreCharacters.activeSelf)
            {
                needMoreCharacters.SetActive(true);
            }

            return;
        }


        // ================================
        // 가져갈 아이템 목록 확인
        // ================================

        List<InventoryItem> carryItemList =
            missionItemManager.GetCarryItemList();


        List<InventoryItem> inventoryList =
            DataManager.Instance.GetAllData.main_data.inventoryItemList;


        // ================================
        // 실제로 가져갈 아이템 목록 구성
        // 최대 12개 슬롯
        // ================================

        List<InventoryItem> selectedItems = new List<InventoryItem>();

        foreach (InventoryItem item in carryItemList)
        {
            if (item == null || item.IsEmpty || item.amount <= 0)
            {
                continue;
            }

            if (selectedItems.Count >= 12)
            {
                break;
            }

            selectedItems.Add(item);
        }


        // ================================
        // 아이템 수량 사전 검사
        // ================================
        // 같은 아이템이 여러 인벤토리 슬롯에
        // 나뉘어 있는 경우도 합산해서 검사
        // ================================

        Dictionary<ItemData, int> requiredAmount = new Dictionary<ItemData, int>();

        foreach (InventoryItem item in selectedItems)
        {
            if (requiredAmount.ContainsKey(item.item))
            {
                requiredAmount[item.item] += item.amount;
            }
            else
            {
                requiredAmount.Add(item.item, item.amount);
            }
        }


        // ================================
        // 인벤토리 수량 검사
        // ================================

        foreach (KeyValuePair<ItemData, int> required in requiredAmount)
        {
            int availableAmount = 0;

            foreach (InventoryItem inventoryItem in inventoryList)
            {
                if (inventoryItem == null ||
                    inventoryItem.IsEmpty ||
                    inventoryItem.amount <= 0)
                {
                    continue;
                }

                if (inventoryItem.item == required.Key)
                {
                    availableAmount += inventoryItem.amount;
                }
            }


            // 수량 부족
            if (availableAmount < required.Value)
            {
                Debug.LogWarning(
                    $"[BattleBegin] 아이템 수량 부족: {required.Key.itemName} / 필요 수량: {required.Value} / 보유 수량: {availableAmount}"
                );

                return;
            }
        }


        // ================================
        // 선택한 아이템 실제 차감
        // ================================

        foreach (InventoryItem selectedItem in selectedItems)
        {
            int remainingAmount = selectedItem.amount;


            // 인벤토리의 여러 슬롯을 순회하며 차감
            foreach (InventoryItem inventoryItem in inventoryList)
            {
                if (remainingAmount <= 0)
                {
                    break;
                }

                if (inventoryItem == null ||
                    inventoryItem.IsEmpty ||
                    inventoryItem.amount <= 0)
                {
                    continue;
                }

                if (inventoryItem.item != selectedItem.item)
                {
                    continue;
                }


                int deductAmount = Mathf.Min(
                    inventoryItem.amount,
                    remainingAmount
                );


                inventoryItem.amount -= deductAmount;
                remainingAmount -= deductAmount;


                Debug.Log(
                    $"[BattleBegin] 아이템 차감: {selectedItem.item.itemName} / {deductAmount}개"
                );
            }
        }


        // ================================
        // 수량이 0이 된 인벤토리 아이템 제거
        // ================================

        inventoryList.RemoveAll(item =>
            item == null ||
            item.IsEmpty ||
            item.amount <= 0
        );


        // ================================
        // 선택한 지역 맵 데이터 저장
        // ================================

        DataManager.Instance.GetBattleData.map_data =
            DataManager.Instance.GetAllData.main_data.mapDataList[index];


        Debug.Log(
            $"[BattleBegin] map_data 저장 완료 / index = {index}"
        );


        // ================================
        // 선택한 캐릭터 저장
        // ================================

        DataManager.Instance.GetBattleData.characters_in_battle_data_list =
            missionInfoManager.GetSelectedCharacters();


        Debug.Log(
            "[BattleBegin] 캐릭터 데이터 저장 완료"
        );


        // ================================
        // 선택한 아이템 저장
        // ================================

        DataManager.Instance.GetBattleData.slots.Clear();


        foreach (InventoryItem item in selectedItems)
        {
            DataManager.Instance.GetBattleData.slots.Add(
                item.Clone()
            );
        }


        // 부족한 슬롯을 12개까지 채우기
        while (DataManager.Instance.GetBattleData.slots.Count < 12)
        {
            DataManager.Instance.GetBattleData.slots.Add(
                new InventoryItem()
            );
        }


        Debug.Log(
            $"[BattleBegin] 최종 슬롯 개수 = {DataManager.Instance.GetBattleData.slots.Count}"
        );


        // ================================
        // 데이터 저장
        // ================================

        DataManager.Instance.SaveData();


        Debug.Log(
            "[BattleBegin] 데이터 저장 완료"
        );


        // ================================
        // 씬 이동
        // ================================

        LoadingData.next_scene = "Map Scene";

        SceneManager.LoadScene("Loading Scene");
    }


    // ============================================================
    // 기존 SetIndex
    // ============================================================

    public void SetIndex(int i)
    {
        Debug.Log(
            $"[SetIndex] 호출됨 / i = {i}"
        );

        SelectedLocation(i);
    }
}