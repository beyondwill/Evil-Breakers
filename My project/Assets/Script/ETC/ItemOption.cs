using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemOption : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private TextMeshProUGUI option_text;
    [SerializeField] private TextMeshProUGUI count_text;


    // 아이템 보여주는거 초기화
    public void ItemOptionInit(string option, string count)
    {
        option_text.text = option;
        count_text.text = count;
    }
}
