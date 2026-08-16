using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterManager : MonoBehaviour
{
    [SerializeField] private StatConfig statConfig;

    [Header("Character Info")]
    [SerializeField] private ShowCharacterInfo showCharacterInfoPrefab;

    [SerializeField] private Transform current_character_info;
    [SerializeField] private Transform next_level_character_info;
    [SerializeField] private Transform equipment_character_info;


    [Header("Equipment Slot")]
    [SerializeField] private List<IconButton> equipment_button_list;

    // 업그레이드 창 현재 장비
    [SerializeField] private IconButton equipment_current_button;

    // 업그레이드 창 다음 레벨 장비
    // ※ 정보 표시용
    [SerializeField] private IconButton equipment_nextlevel_button;

    // 실제 업그레이드 버튼
    [SerializeField] private Button upgradeButton;


    [Header("Equipment Window")]
    [SerializeField] private GameObject equipment_window;

    [SerializeField] private GameObject equipmentContent;

    // 장비 목록 창
    [SerializeField] private GameObject equipments;

    // 업그레이드 창
    [SerializeField] private GameObject upgrades;


    [Header("Character")]
    [SerializeField] private Image character_image;

    [SerializeField] private TextMeshProUGUI player_character_name_text;

    [SerializeField] private TextMeshProUGUI player_character_level_text;

    // 업그레이드 불가능 사유
    [SerializeField] private TextMeshProUGUI wrongText;


    [Header("Character List")]
    [SerializeField] private GameObject show_and_hide;

    [SerializeField] private GameObject scrollview_contents;


    [Header("Prefab")]
    [SerializeField] private GameObject icon_prefab;

    [SerializeField] private Sprite no_equipment_icon;


    [Header("Equipment Detail UI")]
    [SerializeField] private GameObject equipment_info_window;


    private PlayerCharacterData currentCharacter;


    // =========================================================
    // 현재 선택한 슬롯
    //
    // 0 = 무기 업그레이드
    // 1 = 방어구 업그레이드
    // 2 이상 = 장비 장착 슬롯
    // =========================================================

    private int selectedEquipmentSlot = -1;


    // =========================================================
    // 현재 업그레이드할 다음 장비
    // =========================================================

    private EquipmentInfo nextUpgradeEquipment;


    // =========================================================
    // 실제 업그레이드 가능 여부
    // =========================================================

    private bool canUpgrade = false;


    public PlayerCharacterData et;


    private void Start()
    {
        show_and_hide.SetActive(false);

        equipment_window.SetActive(false);

        equipment_info_window.SetActive(false);

        equipments.SetActive(false);

        upgrades.SetActive(false);


        InitEquipmentButton();

        InitUpgradeButton();

        ShowEquipmentList();

        ShowPlayerCharacter();


        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }


        HideWrongText();


        // =====================================================
        // 중요
        //
        // wrongText는 경고 문구만 보여주는 UI다.
        // 마우스 클릭 / PointerEnter 등을 가로채면 안 된다.
        // =====================================================

        if (wrongText != null)
        {
            wrongText.raycastTarget = false;
        }
    }


    public void ShowInit()
    {
        show_and_hide.SetActive(false);

        ShowPlayerCharacter();
    }


    // =========================================================
    // 업그레이드 버튼 초기화
    // =========================================================

    private void InitUpgradeButton()
    {
        if (upgradeButton == null)
            return;


        upgradeButton.onClick.RemoveListener(UpgradeEquipment);

        upgradeButton.onClick.AddListener(UpgradeEquipment);
    }


    // =========================================================
    // 캐릭터 정보 UI 초기화
    // =========================================================

    private void ClearCharacterInfo()
    {
        ClearChildren(current_character_info);

        ClearChildren(equipment_character_info);

        ClearChildren(next_level_character_info);
    }


    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;


        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }


    // =========================================================
    // 장비 버튼 초기화
    // =========================================================

    private void InitEquipmentButton()
    {
        for (int i = 0;
             i < equipment_button_list.Count;
             i++)
        {
            int index = i;


            equipment_button_list[i]
                .ActionAdd(() =>
                {
                    SelectEquipmentSlot(index);
                });


            equipment_button_list[i]
                .ActionAdd(() =>
                {
                    ShowEquipmentInfo(index);
                });
        }
    }


    // =========================================================
    // 장비 상세 정보
    // =========================================================

    private void ShowEquipmentInfo(int slot)
    {
        if (currentCharacter == null)
            return;


        if (slot < 0 ||
            slot >= currentCharacter.player_equipment_list.Count)
            return;


        // 0~1번은 업그레이드 슬롯
        if (slot <= 1)
            return;


        EquipmentInfo equipment =
            currentCharacter
                .player_equipment_list[slot]
                .equipment_info;


        if (equipment == null)
            return;


        equipment_info_window.SetActive(true);


        Debug.Log(equipment.name);
    }


    // =========================================================
    // 캐릭터 목록
    // =========================================================

    public void ShowPlayerCharacter()
    {
        foreach (Transform child
                 in scrollview_contents.transform)
        {
            Destroy(child.gameObject);
        }


        List<PlayerCharacterData> list =
            DataManager.Instance
                .GetAllData
                .main_data
                .player_character_data_list;


        foreach (PlayerCharacterData PCD in list)
        {
            GameObject obj =
                Instantiate(
                    icon_prefab,
                    scrollview_contents.transform,
                    false);


            IconButton icon =
                obj.GetComponent<IconButton>();


            icon.SetColor(
                PCD.player_character_info
                    .icon_background_color);


            icon.SetImage(
                PCD.player_character_info
                    .character_icon);


            PlayerCharacterData captured = PCD;


            icon.ActionAdd(() =>
            {
                ShowCharacterInfo(captured);
            });
        }
    }


    // =========================================================
    // 캐릭터 정보
    // =========================================================

    public void ShowCharacterInfo(
        PlayerCharacterData PCD)
    {
        ClearCharacterInfo();


        currentCharacter = PCD;


        show_and_hide.SetActive(true);

        equipment_info_window.SetActive(true);


        player_character_name_text.text =
            PCD.player_character_info.character_name;


        player_character_level_text.text =
            "Lv." + PCD.player_character_level;


        character_image.sprite =
            PCD.player_character_info.character_full_art;


        ShowCurrentLevelStats(PCD);

        ShowEquipmentStats();

        ShowNextLevelStats(PCD);

        ShowEquippedEquipment();


        // 기본 선택
        // 2번부터 장비 슬롯
        SelectEquipmentSlot(0, false);
    }


    public void ShowCharacterInfo()
    {
        ShowCharacterInfo(et);
    }


    // =========================================================
    // 현재 레벨 캐릭터 스탯
    // =========================================================

    private void ShowCurrentLevelStats(
        PlayerCharacterData PCD)
    {
        ShowCharacterInfo currentLevelInfo =
            Instantiate(
                showCharacterInfoPrefab,
                current_character_info);


        currentLevelInfo.ShowInfo(
            "레벨",
            PCD.player_character_level.ToString());


        int currentLevel =
            PCD.player_character_level;


        foreach (CharacterBaseStatType type
                 in Enum.GetValues(
                     typeof(CharacterBaseStatType)))
        {
            CharacterBaseStatSort stat =
                statConfig.FindBaseStat(type);


            if (stat == null)
                continue;


            float value =
                PCD.player_character_info
                    .GetStatValue(
                        type,
                        currentLevel);


            ShowCharacterInfo SCI =
                Instantiate(
                    showCharacterInfoPrefab,
                    current_character_info);


            SCI.ShowInfo(
                stat.statName,
                value.ToString());
        }
    }


    // =========================================================
    // 장비 스탯 표시
    // =========================================================

    private void ShowEquipmentStats()
    {
        if (currentCharacter == null)
            return;


        foreach (CharacterBaseStatType type
                 in Enum.GetValues(
                     typeof(CharacterBaseStatType)))
        {
            CharacterBaseStatSort stat =
                statConfig.FindBaseStat(type);


            if (stat == null)
                continue;


            float equipmentValue =
                GetEquipmentStat(type);


            if (equipmentValue == 0f)
                continue;


            ShowCharacterInfo SCI =
                Instantiate(
                    showCharacterInfoPrefab,
                    equipment_character_info);


            SCI.ShowInfo(
                stat.statName,
                equipmentValue.ToString());
        }
    }


    // =========================================================
    // 장착 장비 스탯 합산
    // =========================================================

    private float GetEquipmentStat(
        CharacterBaseStatType type)
    {
        if (currentCharacter == null)
            return 0f;


        float total = 0f;


        foreach (var equipmentSlot
                 in currentCharacter.player_equipment_list)
        {
            if (equipmentSlot == null)
                continue;


            EquipmentInfo equipment =
                equipmentSlot.equipment_info;


            if (equipment == null)
                continue;


            foreach (CharacterBaseStatValue stat
                     in equipment.baseStatList)
            {
                if (stat == null)
                    continue;


                if (stat.type == type)
                {
                    total += stat.value;
                }
            }
        }


        return total;
    }


    // =========================================================
    // 다음 레벨 캐릭터 스탯
    // =========================================================

    private void ShowNextLevelStats(
        PlayerCharacterData PCD)
    {
        ShowCharacterInfo nextLevelInfo =
            Instantiate(
                showCharacterInfoPrefab,
                next_level_character_info);


        nextLevelInfo.ShowInfo(
            "레벨",
            (PCD.player_character_level + 1).ToString());


        int nextLevel =
            PCD.player_character_level + 1;


        foreach (CharacterBaseStatType type
                 in Enum.GetValues(
                     typeof(CharacterBaseStatType)))
        {
            CharacterBaseStatSort stat =
                statConfig.FindBaseStat(type);


            if (stat == null)
                continue;


            float value =
                PCD.player_character_info
                    .GetStatValue(
                        type,
                        nextLevel);


            ShowCharacterInfo SCI =
                Instantiate(
                    showCharacterInfoPrefab,
                    next_level_character_info);


            SCI.ShowInfo(
                stat.statName,
                value.ToString());
        }
    }


    // =========================================================
    // 현재 장착 장비 표시
    // =========================================================

    private void ShowEquippedEquipment()
    {
        if (currentCharacter == null)
            return;


        for (int i = 0;
             i < equipment_button_list.Count;
             i++)
        {
            if (i >= currentCharacter.player_equipment_list.Count)
                continue;


            EquipmentInfo equipment =
                currentCharacter
                    .player_equipment_list[i]
                    .equipment_info;


            // 아이콘
            if (equipment == null)
            {
                equipment_button_list[i]
                    .SetImage(no_equipment_icon);
            }
            else
            {
                equipment_button_list[i]
                    .SetImage(equipment.icon);
            }


            // Tooltip
            ItemTooltipTrigger tooltip =
                equipment_button_list[i]
                    .GetComponent<ItemTooltipTrigger>();


            if (tooltip != null)
            {
                tooltip.SetItem(equipment);
            }


            equipment_button_list[i]
                .ToggleButtonActive(true);


            // 선택 표시
            if (i == selectedEquipmentSlot &&
                equipment_window.activeSelf)
            {
                equipment_button_list[i]
                    .SetColor(Color.red);
            }
            else
            {
                equipment_button_list[i]
                    .SetColor(Color.black);
            }
        }
    }


    // =========================================================
    // 슬롯 선택
    // =========================================================

    private void SelectEquipmentSlot(
        int slot,
        bool pickup = true)
    {
        if (currentCharacter == null)
            return;


        if (slot < 0 ||
            slot >= currentCharacter.player_equipment_list.Count)
            return;


        // 같은 슬롯 재클릭
        if (selectedEquipmentSlot == slot &&
            pickup)
        {
            // 2번 이상만 장비 해제
            if (slot >= 2)
            {
                EquipmentInfo equipment =
                    currentCharacter
                        .player_equipment_list[slot]
                        .equipment_info;


                if (equipment != null)
                {
                    UnequipEquipment(slot);
                }
            }


            return;
        }


        selectedEquipmentSlot = slot;


        equipment_window.SetActive(true);


        // =====================================================
        // 0~1번
        // → 업그레이드
        // =====================================================

        if (slot <= 1)
        {
            equipments.SetActive(false);

            upgrades.SetActive(true);

            equipment_info_window.SetActive(false);


            ShowUpgradeInfo(slot);
        }


        // =====================================================
        // 2번 이상
        // → 장비
        // =====================================================

        else
        {
            upgrades.SetActive(false);

            equipments.SetActive(true);

            equipment_info_window.SetActive(true);


            ShowEquipmentList();


            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }


            HideWrongText();
        }


        ShowEquippedEquipment();
    }


    // =========================================================
    // 업그레이드 정보 표시
    // =========================================================

    private void ShowUpgradeInfo(int slot)
    {
        canUpgrade = false;

        nextUpgradeEquipment = null;


        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }


        HideWrongText();


        if (currentCharacter == null)
        {
            HideNextUpgradeEquipment();
            return;
        }


        if (slot < 0 ||
            slot > 1)
        {
            HideNextUpgradeEquipment();
            return;
        }


        if (equipment_current_button == null ||
            equipment_nextlevel_button == null)
        {
            return;
        }


        // =====================================================
        // 현재 장비
        // =====================================================

        EquipmentInfo currentEquipment =
            currentCharacter
                .player_equipment_list[slot]
                .equipment_info;


        if (currentEquipment == null)
        {
            HideNextUpgradeEquipment();
            return;
        }


        // =====================================================
        // 현재 장비 표시
        // =====================================================

        equipment_current_button
            .gameObject
            .SetActive(true);


        equipment_current_button
            .SetImage(currentEquipment.icon);


        ItemTooltipTrigger currentTooltip =
            equipment_current_button
                .GetComponent<ItemTooltipTrigger>();


        if (currentTooltip != null)
        {
            currentTooltip.SetItem(currentEquipment);
        }


        // =====================================================
        // 리스트 가져오기
        // =====================================================

        List<ItemData> itemList;


        if (slot == 0)
        {
            itemList =
                currentCharacter
                    .player_character_info
                    .weaponItemDataList;
        }
        else
        {
            itemList =
                currentCharacter
                    .player_character_info
                    .armorItemDataList;
        }


        if (itemList == null ||
            itemList.Count == 0)
        {
            HideNextUpgradeEquipment();
            return;
        }


        // =====================================================
        // 현재 장비 인덱스
        // =====================================================

        int currentIndex =
            FindItemIndex(
                itemList,
                currentEquipment);


        if (currentIndex == -1)
        {
            HideNextUpgradeEquipment();
            return;
        }


        // =====================================================
        // 다음 장비
        // =====================================================

        int nextIndex =
            currentIndex + 1;


        // 다음 장비 자체가 없으면 숨김
        if (nextIndex >= itemList.Count)
        {
            HideNextUpgradeEquipment();
            return;
        }


        ItemData nextItem =
            itemList[nextIndex];


        if (nextItem == null)
        {
            HideNextUpgradeEquipment();
            return;
        }


        EquipmentInfo nextEquipment =
            nextItem as EquipmentInfo;


        if (nextEquipment == null)
        {
            HideNextUpgradeEquipment();
            return;
        }


        nextUpgradeEquipment =
            nextEquipment;


        // =====================================================
        // ★ 중요
        //
        // 레벨 부족 여부와 관계없이
        // 다음 장비 버튼은 항상 활성화
        // =====================================================

        equipment_nextlevel_button
            .gameObject
            .SetActive(true);


        equipment_nextlevel_button
            .SetImage(nextEquipment.icon);


        // =====================================================
        // ★ Tooltip도 항상 설정
        // =====================================================

        ItemTooltipTrigger nextTooltip =
            equipment_nextlevel_button
                .GetComponent<ItemTooltipTrigger>();


        if (nextTooltip != null)
        {
            nextTooltip.SetItem(nextEquipment);
        }


        // =====================================================
        // ★ 중요
        //
        // 다음 장비 버튼의 interactable은 건드리지 않는다.
        //
        // 업그레이드 가능 여부는 upgradeButton만 관리한다.
        // =====================================================

        Button nextEquipmentButton =
            equipment_nextlevel_button
                .GetComponent<Button>();


        if (nextEquipmentButton != null)
        {
            nextEquipmentButton.interactable = true;
        }


        // =====================================================
        // 다음 장비 레벨
        // =====================================================

        int nextEquipmentLevel =
            nextIndex + 1;


        // =====================================================
        // 캐릭터 레벨 검사
        // =====================================================

        if (currentCharacter.player_character_level >=
            nextEquipmentLevel)
        {
            // 강화 가능

            canUpgrade = true;


            if (upgradeButton != null)
            {
                upgradeButton.interactable = true;
            }


            HideWrongText();
        }
        else
        {
            // 강화 불가능

            canUpgrade = false;


            // ★ 업그레이드 버튼만 비활성화
            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }


            // ★ 다음 장비 버튼은 그대로 유지
            // ★ Tooltip도 그대로 작동
            ShowWrongText(
                "레벨이 부족합니다.");
        }
    }


    // =========================================================
    // 경고 텍스트 표시
    // =========================================================

    private void ShowWrongText(string message)
    {
        if (wrongText == null)
            return;


        wrongText.text = message;

        wrongText.gameObject.SetActive(true);


        // =====================================================
        // ★ 핵심
        //
        // 경고 텍스트가 마우스 이벤트를 가로채면 안 된다.
        //
        // 레벨 부족 상태에서도
        // nextlevel 장비 버튼의 ItemTooltipTrigger가
        // 정상적으로 PointerEnter를 받아야 한다.
        // =====================================================

        wrongText.raycastTarget = false;
    }


    // =========================================================
    // 경고 텍스트 숨김
    // =========================================================

    private void HideWrongText()
    {
        if (wrongText == null)
            return;


        wrongText.text = "";

        wrongText.gameObject.SetActive(false);

        // 혹시 Inspector에서 켜져 있어도
        // 항상 마우스 이벤트를 통과시킨다.
        wrongText.raycastTarget = false;
    }


    // =========================================================
    // 다음 업그레이드 장비 숨기기
    // =========================================================

    private void HideNextUpgradeEquipment()
    {
        canUpgrade = false;

        nextUpgradeEquipment = null;


        if (equipment_nextlevel_button != null)
        {
            equipment_nextlevel_button
                .gameObject
                .SetActive(false);


            ItemTooltipTrigger tooltip =
                equipment_nextlevel_button
                    .GetComponent<ItemTooltipTrigger>();


            if (tooltip != null)
            {
                tooltip.SetItem(null);
            }
        }


        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }


        HideWrongText();
    }


    // =========================================================
    // 현재 장비의 리스트 인덱스 찾기
    // =========================================================

    private int FindItemIndex(
        List<ItemData> itemList,
        EquipmentInfo currentEquipment)
    {
        if (itemList == null ||
            currentEquipment == null)
        {
            return -1;
        }


        // 1. ScriptableObject 참조 비교
        for (int i = 0;
             i < itemList.Count;
             i++)
        {
            if (itemList[i] == currentEquipment)
            {
                return i;
            }
        }


        // 2. 이름 비교
        for (int i = 0;
             i < itemList.Count;
             i++)
        {
            if (itemList[i] == null)
                continue;


            if (itemList[i].name ==
                currentEquipment.name)
            {
                return i;
            }


            if (itemList[i].itemName ==
                currentEquipment.itemName)
            {
                return i;
            }
        }


        return -1;
    }


    // =========================================================
    // 실제 장비 업그레이드
    // =========================================================

    public void UpgradeEquipment()
    {
        if (!canUpgrade)
            return;


        if (currentCharacter == null)
            return;


        if (nextUpgradeEquipment == null)
            return;


        if (selectedEquipmentSlot < 0 ||
            selectedEquipmentSlot > 1)
        {
            return;
        }


        if (selectedEquipmentSlot >=
            currentCharacter.player_equipment_list.Count)
        {
            return;
        }


        EquipmentInfo currentEquipment =
            currentCharacter
                .player_equipment_list[
                    selectedEquipmentSlot]
                .equipment_info;


        if (currentEquipment == null)
            return;


        List<ItemData> itemList;


        if (selectedEquipmentSlot == 0)
        {
            itemList =
                currentCharacter
                    .player_character_info
                    .weaponItemDataList;
        }
        else
        {
            itemList =
                currentCharacter
                    .player_character_info
                    .armorItemDataList;
        }


        if (itemList == null ||
            itemList.Count == 0)
        {
            return;
        }


        int currentIndex =
            FindItemIndex(
                itemList,
                currentEquipment);


        if (currentIndex == -1)
            return;


        int nextIndex =
            currentIndex + 1;


        if (nextIndex >= itemList.Count)
            return;


        EquipmentInfo actualNextEquipment =
            itemList[nextIndex]
                as EquipmentInfo;


        if (actualNextEquipment == null)
            return;


        if (actualNextEquipment !=
            nextUpgradeEquipment)
        {
            return;
        }


        int nextEquipmentLevel =
            nextIndex + 1;


        if (currentCharacter.player_character_level <
            nextEquipmentLevel)
        {
            canUpgrade = false;


            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }


            // ★ Tooltip은 건드리지 않는다.
            ShowWrongText(
                "레벨이 부족합니다.");


            return;
        }


        // =====================================================
        // 실제 업그레이드
        // =====================================================

        currentCharacter
            .player_equipment_list[
                selectedEquipmentSlot]
            .equipment_info =
                actualNextEquipment;

        // 장비 레벨 기록
        if (selectedEquipmentSlot == 0)
        {
            currentCharacter.current_weapon_level = nextEquipmentLevel;
        }
        else if (selectedEquipmentSlot == 1)
        {
            currentCharacter.current_armor_level = nextEquipmentLevel;
        }

        SaveCharacterEquipment();


        RefreshCharacterStats();

        ShowEquippedEquipment();

        ShowUpgradeInfo(
            selectedEquipmentSlot);


        Debug.Log(
            "장비 업그레이드 : "
            + currentEquipment.itemName
            + " → "
            + actualNextEquipment.itemName);
    }


    // =========================================================
    // 장비 해제
    // =========================================================

    private void UnequipEquipment(int slot)
    {
        if (slot <= 1)
            return;


        EquipmentInfo equipment =
            currentCharacter
                .player_equipment_list[slot]
                .equipment_info;


        if (equipment == null)
            return;


        DataManager.Instance
            .GetAllData
            .main_data
            .equipmentInfoList
            .Add(equipment);


        currentCharacter
            .player_equipment_list[slot]
            .equipment_info = null;


        SaveCharacterEquipment();


        RefreshCharacterStats();


        ShowEquippedEquipment();

        ShowEquipmentList();
    }


    // =========================================================
    // 보유 장비 목록
    // =========================================================

    private void ShowEquipmentList()
    {
        foreach (Transform child
                 in equipmentContent.transform)
        {
            Destroy(child.gameObject);
        }


        List<EquipmentInfo> equipmentList =
            DataManager.Instance
                .GetAllData
                .main_data
                .equipmentInfoList;


        int totalSlotCount = 12;


        if (equipmentList.Count > totalSlotCount)
        {
            int extraCount =
                equipmentList.Count -
                totalSlotCount;


            totalSlotCount +=
                Mathf.CeilToInt(
                    extraCount / 3f) * 3;
        }


        foreach (EquipmentInfo equipment
                 in equipmentList)
        {
            GameObject obj =
                Instantiate(
                    icon_prefab,
                    equipmentContent.transform,
                    false);


            IconButton icon =
                obj.GetComponent<IconButton>();


            if (equipment.icon != null)
            {
                icon.SetImage(equipment.icon);
            }
            else
            {
                icon.SetEmpty();
            }


            ItemTooltipTrigger tooltip =
                obj.GetComponent<ItemTooltipTrigger>();


            if (tooltip != null)
            {
                tooltip.SetItem(equipment);
            }


            EquipmentInfo capturedEquipment =
                equipment;


            icon.ActionAdd(() =>
            {
                EquipEquipment(
                    capturedEquipment);
            });
        }


        int emptyCount =
            totalSlotCount -
            equipmentList.Count;


        for (int i = 0;
             i < emptyCount;
             i++)
        {
            GameObject obj =
                Instantiate(
                    icon_prefab,
                    equipmentContent.transform,
                    false);


            IconButton icon =
                obj.GetComponent<IconButton>();


            icon.SetEmpty();

            icon.ToggleButtonActive(false);


            ItemTooltipTrigger tooltip =
                obj.GetComponent<ItemTooltipTrigger>();


            if (tooltip != null)
            {
                tooltip.SetItem(null);
            }
        }
    }


    // =========================================================
    // 장비 장착 / 교체
    // =========================================================

    private void EquipEquipment(
        EquipmentInfo equipment)
    {
        if (currentCharacter == null)
            return;


        if (selectedEquipmentSlot == -1)
            return;


        if (selectedEquipmentSlot <= 1)
            return;


        if (selectedEquipmentSlot >=
            currentCharacter.player_equipment_list.Count)
        {
            return;
        }


        EquipmentInfo oldEquipment =
            currentCharacter
                .player_equipment_list[
                    selectedEquipmentSlot]
                .equipment_info;


        if (oldEquipment != null)
        {
            DataManager.Instance
                .GetAllData
                .main_data
                .equipmentInfoList
                .Add(oldEquipment);
        }


        DataManager.Instance
            .GetAllData
            .main_data
            .equipmentInfoList
            .Remove(equipment);


        currentCharacter
            .player_equipment_list[
                selectedEquipmentSlot]
            .equipment_info =
                equipment;


        SaveCharacterEquipment();


        RefreshCharacterStats();


        ShowEquippedEquipment();

        ShowEquipmentList();
    }


    // =========================================================
    // 캐릭터 스탯 UI 전체 갱신
    // =========================================================

    private void RefreshCharacterStats()
    {
        if (currentCharacter == null)
            return;


        ClearChildren(current_character_info);

        ClearChildren(equipment_character_info);

        ClearChildren(next_level_character_info);


        ShowCurrentLevelStats(
            currentCharacter);


        ShowEquipmentStats();


        ShowNextLevelStats(
            currentCharacter);
    }


    // =========================================================
    // 선택 색상 초기화
    // =========================================================

    public void ResetEquipmentSlotColor()
    {
        for (int i = 0;
             i < equipment_button_list.Count;
             i++)
        {
            equipment_button_list[i]
                .SetColor(Color.black);
        }


        selectedEquipmentSlot = -1;


        equipments.SetActive(false);

        upgrades.SetActive(false);


        canUpgrade = false;

        nextUpgradeEquipment = null;


        if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }


        if (equipment_nextlevel_button != null)
        {
            equipment_nextlevel_button
                .gameObject
                .SetActive(false);
        }


        HideWrongText();
    }


    // =========================================================
    // 저장
    // =========================================================

    private void SaveCharacterEquipment()
    {
        DataManager.Instance.SaveData();
    }
}