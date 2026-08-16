using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DictionaryData
{
    public DataEntity dictionary_data;
    public bool is_recovered;
}

public class DictionaryManager : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private GameObject dictionary_content;
    [SerializeField] private GameObject character_icon;

    [SerializeField] private Image enemy_character_image;
    [SerializeField] private TextMeshProUGUI enemy_character_name;
    [SerializeField] private TextMeshProUGUI enemy_character_info;

    // 변수
    [SerializeField] private List<EnemyCharacterInfo> enemy_character_info_list;

    void Start()
    {
        MonsterDictionaryInit();
    }

    // 몬스터 사전 초기화
    public void MonsterDictionaryInit()
    {
        for (int i = 0; i < enemy_character_info_list.Count; i++)
        {
            int index = i;
            EnemyCharacterInfo ECI = enemy_character_info_list[i];
            GameObject icon = Instantiate(character_icon, dictionary_content.transform);
            IconButton IB = icon.GetComponent<IconButton>();
            IB.SetImage(ECI.character_icon);
            IB.SetColor(ECI.icon_background_color);
            IB.ActionAdd(() => ShowMonsterInfo(index));
        }
    }

    // 몬스터 정보 보여주기
    public void ShowMonsterInfo(int index)
    {
        EnemyCharacterInfo ECI = enemy_character_info_list[index];
        enemy_character_image.sprite = ECI.character_full_art;
        enemy_character_name.text = ECI.character_name;
        enemy_character_info.text = MakeMonsterInfoText(ECI);
    }

    public string MakeMonsterInfoText(EnemyCharacterInfo ECI)
    {
        string result = ""; 
        //result += "생명력: " + ECI.max_health + '\n';
        //result += "공격력: " + 100 + '\n' + '\n';
        result += "<i>\"" + ECI.character_story + "</i>\"";

        return result;
    }
}
