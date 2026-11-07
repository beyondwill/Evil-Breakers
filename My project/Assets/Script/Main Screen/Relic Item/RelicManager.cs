using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicManager : MonoBehaviour
{
    private enum RelicMode
    {
        Manage,
        Purification
    }

    [Header("Relic List")]
    [SerializeField] private Transform relicListContent;
    [SerializeField] private IconButton iconPrefab;

    [Header("Equipped Relics")]
    [SerializeField] private IconButton[] equippedRelicIcons;

    [Header("Current Relic")]
    [SerializeField] private IconButton currentRelicIcon;
    [SerializeField] private TextMeshProUGUI currentRelicName;
    [SerializeField] private TextMeshProUGUI currentRelicText;

    [Header("Purification UI")]
    [SerializeField] private IconButton purificationSelectedIcon;
    [SerializeField] private IconButton purificationWaterIcon;
    [SerializeField] private ItemData purificationWaterItem;
    [SerializeField] private Button purifyButton;

    // 최초 진입 시 정렬된 전체 유물 순서
    private List<ItemData> relicList = new();

    private ItemData pendingRelic;
    private ItemData selectedRelic;

    private RelicMode currentMode = RelicMode.Manage;

    private const int MAX_RELIC_COUNT = 3;

    private MainData GetMainData()
    {
        return DataManager.Instance.GetAllData.main_data;
    }


    // ========================================
    // 초기화
    // ========================================

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (DataManager.Instance == null ||
            DataManager.Instance.GetAllData == null ||
            DataManager.Instance.GetAllData.main_data == null)
        {
            Debug.LogError("[Relic] DataManager 데이터가 없습니다.");
            return;
        }

        if (relicListContent == null ||
            iconPrefab == null ||
            equippedRelicIcons == null ||
            equippedRelicIcons.Length != MAX_RELIC_COUNT ||
            currentRelicIcon == null ||
            currentRelicName == null ||
            currentRelicText == null ||
            purificationSelectedIcon == null ||
            purificationWaterIcon == null ||
            purificationWaterItem == null ||
            purifyButton == null)
        {
            Debug.LogError("[Relic] UI 연결을 확인해주세요.");
            return;
        }

        MainData data = GetMainData();

        if (data.storageRelicList == null)
            data.storageRelicList = new List<ItemData>();

        if (data.relicItems == null)
            data.relicItems = new List<ItemData>();

        if (data.purgedRelicList == null)
            data.purgedRelicList = new List<ItemData>();

        if (data.inventoryItemList == null)
            data.inventoryItemList = new List<InventoryItem>();


        // 장착 유물 개수 제한
        while (data.relicItems.Count > MAX_RELIC_COUNT)
        {
            ItemData relic = data.relicItems[^1];

            data.relicItems.RemoveAt(data.relicItems.Count - 1);

            if (relic != null &&
                !data.storageRelicList.Contains(relic))
            {
                data.storageRelicList.Add(relic);
            }
        }

        CompactEquippedRelics();

        // 최초 진입 시에만 정렬
        SortInitialRelicList();

        // 전체 유물 순서 생성
        InitializeRelicOrder();

        pendingRelic = null;
        selectedRelic = null;

        currentMode = RelicMode.Manage;

        RefreshUI();

        //Debug.Log("[Relic] 초기화 완료");
    }


    // ========================================
    // 최초 진입 시 정렬
    // ========================================

    private void SortInitialRelicList()
    {
        MainData data = GetMainData();

        data.storageRelicList.Sort((a, b) =>
        {
            if (a == null) return 1;
            if (b == null) return -1;

            return string.Compare(
                a.itemName,
                b.itemName,
                StringComparison.Ordinal
            );
        });

        data.relicItems.Sort((a, b) =>
        {
            if (a == null) return 1;
            if (b == null) return -1;

            return string.Compare(
                a.itemName,
                b.itemName,
                StringComparison.Ordinal
            );
        });
    }


    // ========================================
    // 전체 유물 순서 초기화
    // ========================================

    private void InitializeRelicOrder()
    {
        relicList.Clear();

        MainData data = GetMainData();

        // 1. 장착된 유물 먼저 추가 (이름순)
        foreach (ItemData relic in data.relicItems)
        {
            AddRelicToOrder(relic);
        }

        // 2. 미장착 유물 추가 (이름순)
        foreach (ItemData relic in data.storageRelicList)
        {
            AddRelicToOrder(relic);
        }
    }

    private void AddRelicToOrder(ItemData relic)
    {
        if (relic == null)
            return;

        if (!relicList.Contains(relic))
            relicList.Add(relic);
    }


    // ========================================
    // 장착 슬롯 압축
    // ========================================

    private void CompactEquippedRelics()
    {
        MainData data = GetMainData();

        data.relicItems.RemoveAll(relic => relic == null);
    }


    // ========================================
    // 모드 전환
    // ========================================

    public void OpenRelicManageMode()
    {
        currentMode = RelicMode.Manage;

        selectedRelic = null;
        pendingRelic = null;

        RefreshUI();

        Debug.Log("[Relic] 유물 관리 모드");
    }

    public void OpenPurificationMode()
    {
        currentMode = RelicMode.Purification;

        selectedRelic = null;
        pendingRelic = null;

        RefreshUI();

        Debug.Log("[Relic] 오염 처리 모드");
    }


    // ========================================
    // 전체 UI 갱신
    // ========================================

    private void RefreshUI()
    {
        if (currentMode == RelicMode.Manage)
        {
            RefreshEquippedRelics();
        }

        CreateRelicList();

        RefreshRelicInformation(selectedRelic);

        if (currentMode == RelicMode.Purification)
        {
            RefreshPurificationUI();
        }
    }


    // ========================================
    // 장착 슬롯 UI
    // ========================================

    private void RefreshEquippedRelics()
    {
        List<ItemData> equippedList = GetMainData().relicItems;

        for (int i = 0; i < MAX_RELIC_COUNT; i++)
        {
            IconButton icon = equippedRelicIcons[i];

            if (icon == null)
                continue;

            icon.ActionRemove();

            ItemData relic = i < equippedList.Count
                ? equippedList[i]
                : null;

            if (relic == null)
            {
                icon.SetEmpty();
                icon.SetText("");
                icon.SetSelected(false);
            }
            else
            {
                icon.SetItemData(relic);
                icon.SetAmount(0);
                icon.SetText("");
                icon.SetSelected(relic == selectedRelic);
            }

            int index = i;

            icon.ActionAdd(() =>
            {
                OnEquippedSlotClicked(index);
            });

            icon.ToggleButtonActive(true);
        }
    }


    // ========================================
    // 왼쪽 유물 목록 생성
    // ========================================

    private void CreateRelicList()
    {
        foreach (Transform child in relicListContent)
        {
            Destroy(child.gameObject);
        }

        MainData data = GetMainData();

        // 기존 relicList 순서를 그대로 사용
        // 데이터의 장착/보유 목록 순서는 변경하지 않음
        foreach (ItemData relic in relicList)
        {
            if (relic == null)
                continue;

            bool isPurified = data.purgedRelicList.Contains(relic);

            // 관리 모드에서는 정화된 유물만 표시
            if (currentMode == RelicMode.Manage && !isPurified)
                continue;

            // 정화 모드에서는 오염된 유물만 표시
            if (currentMode == RelicMode.Purification && isPurified)
                continue;

            IconButton icon = Instantiate(
                iconPrefab,
                relicListContent
            );

            icon.SetItemData(relic);
            icon.SetAmount(0);

            bool isEquipped = data.relicItems.Contains(relic);

            string displayText = "";

            if (currentMode == RelicMode.Manage)
            {
                if (isEquipped)
                    displayText = "장착됨";

                icon.ToggleButtonActive(!isEquipped);
            }
            else
            {
                icon.SetText("");
                icon.ToggleButtonActive(true);
            }

            icon.SetText(displayText);
            icon.SetSelected(relic == selectedRelic);

            icon.ActionAdd(() =>
            {
                OnRelicSelected(relic);
            });
        }
    }


    // ========================================
    // 유물 선택
    // ========================================

    private void OnRelicSelected(ItemData relic)
    {
        if (relic == null)
            return;

        selectedRelic = relic;

        // 정화 모드
        if (currentMode == RelicMode.Purification)
        {
            Debug.Log($"[Relic] 정화 대상 선택: {relic.itemName}");

            RefreshUI();
            return;
        }

        // 유물 관리 모드
        MainData data = GetMainData();

        if (data.relicItems.Contains(relic))
            return;

        Debug.Log($"[Relic] 유물 선택: {relic.itemName}");

        CompactEquippedRelics();

        // 빈 장착 슬롯이 있으면 자동 장착
        if (data.relicItems.Count < MAX_RELIC_COUNT)
        {
            data.relicItems.Add(relic);

            data.storageRelicList.Remove(relic);

            pendingRelic = null;

            Debug.Log($"[Relic] 자동 장착: {relic.itemName}");

            // relicList 순서는 변경하지 않음
            RefreshUI();
            return;
        }

        // 슬롯이 모두 찼으면 교체 대기
        pendingRelic = relic;

        Debug.Log($"[Relic] 교체 대기: {relic.itemName}");

        RefreshUI();
    }


    // ========================================
    // 장착 슬롯 클릭
    // ========================================

    private void OnEquippedSlotClicked(int index)
    {
        if (currentMode != RelicMode.Manage)
            return;

        MainData data = GetMainData();

        if (index < 0 || index >= MAX_RELIC_COUNT)
            return;

        ItemData oldRelic = index < data.relicItems.Count
            ? data.relicItems[index]
            : null;


        // 교체 대기 중인 유물이 있는 경우
        if (pendingRelic != null)
        {
            if (oldRelic == pendingRelic)
            {
                pendingRelic = null;
                selectedRelic = null;

                RefreshUI();
                return;
            }

            // 기존 유물을 보유 목록 맨 뒤에 추가
            if (oldRelic != null &&
                !data.storageRelicList.Contains(oldRelic))
            {
                data.storageRelicList.Add(oldRelic);
            }

            // 새 유물을 보유 목록에서 제거
            data.storageRelicList.Remove(pendingRelic);

            // 기존 슬롯에 새 유물 장착
            data.relicItems[index] = pendingRelic;

            Debug.Log(
                $"[Relic] 교체 완료: " +
                $"{oldRelic?.itemName} → {pendingRelic.itemName}"
            );

            selectedRelic = pendingRelic;
            pendingRelic = null;

            // relicList 순서는 변경하지 않음
            RefreshUI();
            return;
        }


        // 빈 슬롯 클릭
        if (oldRelic == null)
        {
            selectedRelic = null;

            RefreshUI();
            return;
        }


        // 장착 해제
        data.relicItems.RemoveAt(index);

        if (!data.storageRelicList.Contains(oldRelic))
        {
            data.storageRelicList.Add(oldRelic);
        }

        // 선택 및 정보 초기화
        selectedRelic = null;
        pendingRelic = null;

        Debug.Log($"[Relic] 장착 해제: {oldRelic.itemName}");

        // relicList 순서는 변경하지 않음
        RefreshUI();
    }


    // ========================================
    // 정화수 수량 확인
    // ========================================

    private InventoryItem GetPurificationWater()
    {
        MainData data = GetMainData();

        return data.inventoryItemList.Find(
            item =>
                item != null &&
                item.item == purificationWaterItem
        );
    }

    private int GetPurificationWaterAmount()
    {
        InventoryItem water = GetPurificationWater();

        if (water == null)
            return 0;

        return Mathf.Max(0, water.amount);
    }


    // ========================================
    // 정화 모드 상단 UI
    // ========================================

    private void RefreshPurificationUI()
    {
        // 선택한 유물 아이콘
        if (selectedRelic == null)
        {
            purificationSelectedIcon.SetEmpty();
            purificationSelectedIcon.SetText("");
            purificationSelectedIcon.SetAmount(0);
            purificationSelectedIcon.SetSelected(false);
        }
        else
        {
            purificationSelectedIcon.SetItemData(selectedRelic);
            purificationSelectedIcon.SetAmount(0);
            purificationSelectedIcon.SetText("");
            purificationSelectedIcon.SetSelected(false);
        }

        // 정화수 아이콘 및 수량
        purificationWaterIcon.SetItemData(purificationWaterItem);
        purificationWaterIcon.SetAmount(GetPurificationWaterAmount());
        purificationWaterIcon.SetText("");
        purificationWaterIcon.SetSelected(false);

        // 정화 가능 여부
        bool canPurify =
            selectedRelic != null &&
            GetPurificationWaterAmount() >= 1;

        purifyButton.interactable = canPurify;
    }


    // ========================================
    // 정화 실행
    // ========================================

    public void PurifySelectedRelic()
    {
        if (currentMode != RelicMode.Purification)
            return;

        if (selectedRelic == null)
        {
            Debug.LogWarning("[Relic] 정화할 유물을 선택해주세요.");
            return;
        }

        MainData data = GetMainData();

        // 이미 정화된 유물인지 확인
        if (data.purgedRelicList.Contains(selectedRelic))
        {
            Debug.LogWarning("[Relic] 이미 정화된 유물입니다.");

            selectedRelic = null;

            RefreshUI();
            return;
        }

        // 정화수 확인
        InventoryItem water = GetPurificationWater();

        if (water == null || water.amount < 1)
        {
            Debug.LogWarning("[Relic] 정화수가 부족합니다.");

            RefreshUI();
            return;
        }

        // 정화수 1개 소비
        water.amount--;

        // 유물 정화 처리
        data.purgedRelicList.Add(selectedRelic);

        Debug.Log($"[Relic] 정화 완료: {selectedRelic.itemName}");

        selectedRelic = null;

        // 정화 모드 목록에서는 사라짐
        // 전체 유물 순서는 유지됨
        RefreshUI();
    }


    // ========================================
    // 오른쪽 유물 정보
    // ========================================

    private void RefreshRelicInformation(ItemData relic)
    {
        if (relic == null)
        {
            // 아이콘 완전 초기화
            currentRelicIcon.SetColor(Color.white);
            currentRelicIcon.SetText("");
            currentRelicIcon.SetAmount(0);
            currentRelicIcon.SetSelected(false);
            currentRelicIcon.SetNone();

            // 이름 및 설명 완전 초기화
            currentRelicName.text = "";
            currentRelicText.text = "";

            return;
        }

        currentRelicIcon.SetItemData(relic);
        currentRelicIcon.SetAmount(0);
        currentRelicIcon.SetSelected(false);
        currentRelicIcon.PointerEnterRemove();

        currentRelicName.text = relic.itemName;
        currentRelicText.text = relic.description;
    }
}