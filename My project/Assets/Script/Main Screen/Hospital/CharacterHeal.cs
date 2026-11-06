
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHeal : MonoBehaviour
{
    [Header("Character Icon")]
    [SerializeField] private IconButton characterIcon;
    [SerializeField] private Image hospitalBox;

    [Header("Character Information")]
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI statusText;

    private PlayerCharacterData characterData;

    private Action<PlayerCharacterData> onSelected;

    private bool isHealing;

    // ========================================
    // 캐릭터 설정
    // ========================================

    public void SetCharacter(
        PlayerCharacterData data,
        Action<PlayerCharacterData> callback,
        bool healing)
    {
        characterData = data;
        onSelected = callback;
        isHealing = healing;

        characterIcon.SetCharacterData(characterData);

        characterIcon.ActionAdd(OnClick);

        UpdateUI();
    }

    // ========================================
    // 클릭
    // ========================================

    private void OnClick()
    {
        if (characterData == null)
            return;

        onSelected?.Invoke(characterData);
    }

    // ========================================
    // UI 갱신
    // ========================================

    public void UpdateUI()
    {
        if (characterData == null)
            return;

        int currentHP = characterData.current_health;

        int maxHP = (int)characterData.player_character_info
            .GetStatValue(CharacterBaseStatType.MaxHealth);

        if (characterName != null)
        {
            characterName.text =
                characterData.player_character_info.character_name;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHP} / {maxHP}";
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        if (statusText != null)
        {
            statusText.text = isHealing
                ? "치료 중"
                : "부상";
        }
    }

    // ========================================
    // 선택 색상 변경
    // ========================================

    public void SetSelected(
        bool selected,
        Color selectedColor,
        Color normalColor)
    {
        if (characterIcon == null)
            return;

        characterIcon.SetColor(
            selected ? selectedColor : normalColor
        );

        characterIcon.SetSelected(selected);

        hospitalBox.color = selected ? Color.gray : Color.white;
    }

    // ========================================
    // 치료 상태 변경
    // ========================================

    public void SetHealing(bool healing)
    {
        isHealing = healing;
        UpdateUI();
    }

    // ========================================
    // 데이터 반환
    // ========================================

    public PlayerCharacterData GetCharacterData()
    {
        return characterData;
    }
}
