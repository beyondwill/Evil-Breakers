using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInfoManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI location_title_text;
    [SerializeField] private TextMeshProUGUI location_info_text;
    [SerializeField] private TextMeshProUGUI location_rank_text;
    [SerializeField] private TextMeshProUGUI essential_objects_text;
    [SerializeField] private TextMeshProUGUI reward_text;

    [SerializeField] private Image location_image;

    [SerializeField] private Button strike_button;


    [SerializeField] private Transform character_list_content;
    [SerializeField] private Transform party_content;

    [SerializeField] private GameObject character_icon_prefab;


    private const int MAX_CHARACTER_COUNT = 3;


    private List<PlayerCharacterData> selectedCharacters =
        new List<PlayerCharacterData>();


    private Dictionary<IconButton, PlayerCharacterData> characterIcons =
        new Dictionary<IconButton, PlayerCharacterData>();


    public void OnEnable()
    {
        strike_button.interactable = false;
    }


    public void OnDisable()
    {
        strike_button.interactable = false;
    }


    public void ShowMissionInfo(LocationInfo LI)
    {
        if (LI == null)
        {
            Debug.LogError("LocationInfo null");
            return;
        }

        gameObject.SetActive(true);

        location_title_text.text = LI.location_name;
        location_info_text.text = LI.location_info;
        location_rank_text.text = $"위험도 : {LI.location_rank}";
        location_image.sprite = LI.location_image;
        reward_text.text = $"보상 : {LI.reward_money} Gold";

        essential_objects_text.text =
            "목표\n- 모든 적 처치";

        selectedCharacters.Clear();

        RefreshCharacterUI();
    }


    private void RefreshCharacterUI()
    {
        CreateCharacterList();
        CreatePartyList();
    }


    private void CreateCharacterList()
    {
        foreach (Transform child in character_list_content)
        {
            Destroy(child.gameObject);
        }

        characterIcons.Clear();

        List<PlayerCharacterData> characterList =
            DataManager.Instance
            .GetAllData
            .main_data
            .player_character_data_list;


        foreach (PlayerCharacterData character in characterList)
        {
            if (selectedCharacters.Contains(character))
                continue;


            GameObject obj =
                Instantiate(
                    character_icon_prefab,
                    character_list_content
                );


            IconButton icon =
                obj.GetComponent<IconButton>();


            if (icon == null)
            {
                Debug.LogError("IconButton 없음");
                continue;
            }


            icon.SetColor(
                character.player_character_info.icon_background_color
            );

            icon.SetImage(
                character.player_character_info.character_icon
            );


            characterIcons.Add(
                icon,
                character
            );


            IconButton capturedIcon = icon;


            if (character.current_health <= 0)
            {
                icon.SetText("전투 불능");
                icon.ToggleButtonActive(false);
            }
            else
            {
                icon.ActionAdd(() =>
                {
                    AddCharacter(capturedIcon);
                });
            }
        }
    }
    private void AddCharacter(IconButton icon)
    {
        PlayerCharacterData character =
            characterIcons[icon];


        if (selectedCharacters.Count >= MAX_CHARACTER_COUNT)
        {
            Debug.Log("최대 3명 선택 가능");
            return;
        }

        selectedCharacters.Add(character);

        if (selectedCharacters.Count == 0)
        {
            strike_button.interactable = false;
        }
        else
        {
            strike_button.interactable = true;
        }

        RefreshCharacterUI();
    }


    private void CreatePartyList()
    {
        foreach (Transform child in party_content)
        {
            Destroy(child.gameObject);
        }


        foreach (PlayerCharacterData character in selectedCharacters)
        {
            GameObject obj =
                Instantiate(
                    character_icon_prefab,
                    party_content
                );


            IconButton icon =
                obj.GetComponent<IconButton>();


            icon.SetColor(
                character.player_character_info.icon_background_color
            );


            icon.SetImage(
                character.player_character_info.character_icon
            );


            PlayerCharacterData capturedCharacter =
                character;


            // 파티에서는 클릭 시 제거
            icon.ActionAdd(() =>
            {
                RemoveCharacter(capturedCharacter);
            });
        }
    }


    private void RemoveCharacter(PlayerCharacterData character)
    {
        selectedCharacters.Remove(character);

        if (selectedCharacters.Count == 0)
        {
            strike_button.interactable = false;
        }
        else
        {
            strike_button.interactable = true;
        }

        RefreshCharacterUI();
    }


    public List<PlayerCharacterData> GetSelectedCharacters()
    {
        return selectedCharacters;
    }
}