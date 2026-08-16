using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowCharacterInfo : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private TextMeshProUGUI character_text;
    [SerializeField] private TextMeshProUGUI character_count;

    // 정보 보여주기
    public void ShowInfo(string text, string count)
    {
        character_text.text = text;
        character_count.text = count;
    }

    // 수치 바꾸기
    public void ShowInfo(int count)
    {
        character_count.text = count.ToString();
    }

    public void ShowInfo(float count)
    {
        character_count.text = count.ToString();
    }
}
