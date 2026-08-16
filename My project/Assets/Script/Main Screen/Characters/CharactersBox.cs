using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersBox : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private MainCharacterSlot characterSlot;               // 메인 캐릭터 슬롯

    [Header("Transform")]
    [SerializeField] private Transform charactersBox;                       // 캐릭터 박스

    public void Start()
    {
        AllCharacterCreate();
    }


    public void AllCharacterCreate()
    {
        List<PlayerCharacterData> PCDList = DataManager.Instance.GetMainData.player_character_data_list;

        for (int i = 0; i < PCDList.Count; i++)
        Instantiate(characterSlot, charactersBox).ShowCharacterSlot(PCDList[i]);
    }
}
