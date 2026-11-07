
using System.Collections.Generic;
using UnityEngine;

public class HospitalManager : MonoBehaviour
{
    [Header("Hospital Slots")]
    [SerializeField] private IconButton[] hospitalSlotIcons;

    [Header("Character List")]
    [SerializeField] private Transform characterListContent;
    [SerializeField] private CharacterHeal characterPrefab;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.gray;

    // 전체 캐릭터 목록
    private List<PlayerCharacterData> characterList =
        new List<PlayerCharacterData>();

    // 치료 슬롯에 배정된 캐릭터
    private PlayerCharacterData[] hospitalCharacters;

    // 오른쪽 캐릭터 UI 목록
    private List<CharacterHeal> characterUIList =
        new List<CharacterHeal>();

    // 현재 선택한 캐릭터
    private PlayerCharacterData selectedCharacter;


    // ========================================
    // 초기화
    // ========================================

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        //Debug.Log("[Hospital] 초기화 시작");

        if (hospitalSlotIcons == null ||
            hospitalSlotIcons.Length == 0)
        {
            Debug.LogError("[Hospital] 병원 슬롯이 연결되지 않았습니다.");
            return;
        }

        if (characterListContent == null || characterPrefab == null)
        {
            Debug.LogError("[Hospital] 캐릭터 목록 UI가 연결되지 않았습니다.");
            return;
        }

        if (DataManager.Instance == null ||
            DataManager.Instance.GetAllData == null ||
            DataManager.Instance.GetAllData.main_data == null)
        {
            Debug.LogError("[Hospital] DataManager 데이터가 없습니다.");
            return;
        }

        // 슬롯 배열 초기화
        hospitalCharacters =
            new PlayerCharacterData[hospitalSlotIcons.Length];

        // 캐릭터 데이터 가져오기
        characterList.Clear();

        characterList.AddRange(
            DataManager.Instance.GetAllData.main_data.player_character_data_list
        );

        // 초기 상태에서는 모든 슬롯 이벤트 제거
        ResetSlotColors();

        // 오른쪽 캐릭터 목록 생성
        CreateCharacterList();

        // 왼쪽 슬롯 갱신
        RefreshHospitalSlots();

        Debug.Log("[Hospital] 초기화 완료");
    }


    // ========================================
    // 캐릭터 목록 생성
    // ========================================

    private void CreateCharacterList()
    {
        foreach (Transform child in characterListContent)
        {
            Destroy(child.gameObject);
        }

        characterUIList.Clear();

        foreach (PlayerCharacterData character in characterList)
        {
            if (character == null)
                continue;

            CharacterHeal ui = Instantiate(
                characterPrefab,
                characterListContent
            );

            ui.SetCharacter(
                character,
                OnCharacterSelected,
                IsHealing(character)
            );

            characterUIList.Add(ui);
        }
    }


    // ========================================
    // 캐릭터 선택
    // ========================================

    private void OnCharacterSelected(PlayerCharacterData character)
    {
        if (character == null)
            return;

        Debug.Log(
            $"[Hospital] 캐릭터 선택: {character.player_character_info.character_name}"
        );

        // 기존 슬롯 이벤트 제거
        ResetSlotColors();

        selectedCharacter = character;

        // 오른쪽 선택 색상 갱신
        foreach (CharacterHeal ui in characterUIList)
        {
            bool isSelected =
                ui.GetCharacterData() == selectedCharacter;

            ui.SetSelected(
                isSelected,
                selectedColor,
                normalColor
            );
        }

        // 치료가 필요한 캐릭터이고 아직 치료 중이 아니라면
        if (NeedsHealing(character) && !IsHealing(character))
        {
            RegisterEmptySlotEvents();
        }
        else
        {
            if (!NeedsHealing(character))
            {
                Debug.Log("[Hospital] 치료가 필요하지 않은 캐릭터입니다.");
            }
            else if (IsHealing(character))
            {
                Debug.Log("[Hospital] 이미 치료 중인 캐릭터입니다.");
            }
        }
    }


    // ========================================
    // 빈 슬롯 이벤트 등록
    // ========================================

    private void RegisterEmptySlotEvents()
    {
        Debug.Log("[Hospital] 빈 슬롯 이벤트 등록 시작");

        for (int i = 0; i < hospitalSlotIcons.Length; i++)
        {
            if (hospitalSlotIcons[i] == null)
                continue;

            int index = i;

            // 이미 캐릭터가 배정된 슬롯은 제외
            bool isEmpty = hospitalCharacters[i] == null;

            if (!isEmpty)
            {
                hospitalSlotIcons[i].ActionRemove();
                hospitalSlotIcons[i].ToggleButtonActive(false);
                hospitalSlotIcons[i].SetColor(normalColor);
                continue;
            }

            // 빈 슬롯에만 클릭 이벤트 등록
            hospitalSlotIcons[i].ActionAdd(() =>
            {
                Debug.Log($"[Hospital] 좌측 슬롯 클릭: {index}");

                OnHospitalSlotClick(index);
            });

            hospitalSlotIcons[i].ToggleButtonActive(true);
            hospitalSlotIcons[i].SetColor(highlightColor);

            Debug.Log($"[Hospital] 슬롯 {index} 이벤트 등록 완료");
        }
    }


    // ========================================
    // 슬롯 이벤트 제거 및 색상 초기화
    // ========================================

    private void ResetSlotColors()
    {
        for (int i = 0; i < hospitalSlotIcons.Length; i++)
        {
            if (hospitalSlotIcons[i] == null)
                continue;

            // 클릭 이벤트 제거
            hospitalSlotIcons[i].ActionRemove();

            // 슬롯 비활성화
            hospitalSlotIcons[i].ToggleButtonActive(false);

            // 색상 초기화
            hospitalSlotIcons[i].SetColor(normalColor);
        }
    }


    // ========================================
    // 슬롯 클릭 및 캐릭터 배정
    // ========================================

    private void OnHospitalSlotClick(int index)
    {
        Debug.Log($"[Hospital] 슬롯 클릭됨: {index}");

        if (selectedCharacter == null)
        {
            Debug.Log("[Hospital] 선택된 캐릭터가 없습니다.");
            return;
        }

        if (index < 0 || index >= hospitalCharacters.Length)
        {
            Debug.LogError("[Hospital] 잘못된 슬롯 인덱스입니다.");
            return;
        }

        if (hospitalSlotIcons[index] == null)
            return;

        if (hospitalCharacters[index] != null)
        {
            Debug.Log("[Hospital] 이미 사용 중인 슬롯입니다.");
            return;
        }

        if (!NeedsHealing(selectedCharacter))
        {
            Debug.Log("[Hospital] 치료가 필요하지 않은 캐릭터입니다.");
            ResetSlotColors();
            return;
        }

        if (IsHealing(selectedCharacter))
        {
            Debug.Log("[Hospital] 이미 치료 중인 캐릭터입니다.");
            ResetSlotColors();
            return;
        }

        // 캐릭터 배정
        PlayerCharacterData assignedCharacter = selectedCharacter;

        hospitalCharacters[index] = assignedCharacter;

        Debug.Log(
            $"[Hospital] {assignedCharacter.player_character_info.character_name} " +
            $"슬롯 {index} 배정 완료"
        );

        // 선택 초기화
        selectedCharacter = null;

        // 남아 있는 슬롯 이벤트 제거
        ResetSlotColors();

        // UI 갱신
        RefreshHospitalSlots();
        RefreshCharacterList();
        ClearCharacterSelection();
    }


    // ========================================
    // 캐릭터 선택 표시 초기화
    // ========================================

    private void ClearCharacterSelection()
    {
        foreach (CharacterHeal ui in characterUIList)
        {
            ui.SetSelected(
                false,
                selectedColor,
                normalColor
            );
        }
    }


    // ========================================
    // 치료 슬롯 UI 갱신
    // ========================================

    private void RefreshHospitalSlots()
    {
        for (int i = 0; i < hospitalSlotIcons.Length; i++)
        {
            if (hospitalSlotIcons[i] == null)
                continue;

            PlayerCharacterData character =
                hospitalCharacters[i];

            if (character != null)
            {
                // 캐릭터가 배정된 슬롯
                hospitalSlotIcons[i].SetCharacterData(character);
            }
            else
            {
                // 빈 슬롯
                hospitalSlotIcons[i].SetCharacterData(
                    (PlayerCharacterData)null
                );
            }

            hospitalSlotIcons[i].SetSelected(false);
            hospitalSlotIcons[i].SetColor(normalColor);
            hospitalSlotIcons[i].ToggleButtonActive(false);
        }
    }


    // ========================================
    // 오른쪽 캐릭터 목록 UI 갱신
    // ========================================

    private void RefreshCharacterList()
    {
        foreach (CharacterHeal ui in characterUIList)
        {
            PlayerCharacterData character =
                ui.GetCharacterData();

            ui.SetHealing(IsHealing(character));
        }
    }


    // ========================================
    // 치료 여부 확인
    // ========================================

    private bool IsHealing(PlayerCharacterData character)
    {
        if (character == null || hospitalCharacters == null)
            return false;

        foreach (PlayerCharacterData hospitalCharacter in hospitalCharacters)
        {
            if (hospitalCharacter == character)
                return true;
        }

        return false;
    }


    // ========================================
    // 치료 필요 여부 확인
    // ========================================

    private bool NeedsHealing(PlayerCharacterData character)
    {
        if (character == null ||
            character.player_character_info == null)
            return false;

        int maxHP = (int)character.player_character_info.GetStatValue(
            CharacterBaseStatType.MaxHealth
        );

        return character.current_health < maxHP;
    }
}
