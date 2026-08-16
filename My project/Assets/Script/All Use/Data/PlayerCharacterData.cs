using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
// 장비칸
public class EquipmentSlot
{
    public EquipmentSort equipment_sort;
    public EquipmentInfo equipment_info = null;

    public EquipmentSlot(EquipmentSort ES)
    {
        equipment_sort = ES;
    }
}

[System.Serializable]
// 캐릭터 데이터
public class PlayerCharacterData
{
    // 변수
    public PlayerCharacterInfo player_character_info;                               // 캐릭터 정보
    public int player_character_level;                                              // 플레이어 레벨
    public int current_health;                                                      // 현재 생명력
    public int current_stress;                                                      // 현재 스트레스
    public int current_weapon_level;                                                // 현재 무기 레벨
    public int current_armor_level;                                                 // 현재 방어도 레벨
    public int current_exp;                                                         // 현재 경험치
    public List<RelicInfo> relicInfoList = new List<RelicInfo>();                   // 유물 리스트
    public List<CardData> player_character_deck = new List<CardData>();             // 플레이어 덱 리스트 (덱 구현)
    public List<EquipmentSlot> player_equipment_list = new List<EquipmentSlot>();   // 장비 리스트

    // 빈 생성자
    public PlayerCharacterData()
    {

    }

    // 생성자 (캐릭터 최초 획득 시)
    public PlayerCharacterData(PlayerCharacterInfo info)
    {
        player_character_info = info;
        current_health = 100;
    }

    // 생성자 (복사용)
    public PlayerCharacterData(PlayerCharacterData other)
    {
        player_character_info = other.player_character_info;
        player_character_level = other.player_character_level;
        current_health = other.current_health;
        current_stress = other.current_stress;
        current_weapon_level = other.current_weapon_level;
        current_armor_level = other.current_armor_level;

        relicInfoList =
            new List<RelicInfo>(other.relicInfoList);

        player_character_deck =
            new List<CardData>(other.player_character_deck);

        player_equipment_list =
            new List<EquipmentSlot>(other.player_equipment_list);
    }
}