using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllHeal : MonoBehaviour
{
    public void Heal()
    {
        Debug.Log("전원 회복!");
        foreach(PlayerCharacterData PCD in DataManager.Instance.GetAllData.main_data.player_character_data_list)
        {
            PCD.current_health = (int)PCD.player_character_info.GetStatValue(CharacterBaseStatType.MaxHealth);
        }

        DataManager.Instance.SaveData();
    }
}
