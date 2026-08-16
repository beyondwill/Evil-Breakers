using TMPro;
using UnityEngine;

public class Person : MonoBehaviour
{
    // 외부 요소
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI dayCount;
    [SerializeField] private TextMeshProUGUI leftCharacterCount;
    [SerializeField] private TextMeshProUGUI deadCharacterCount;


    private void Update()
    {
        dayCount.text = "D - "+ DataManager.Instance.GetAllData.main_data.day.ToString();

        var characterList =
            DataManager.Instance.GetAllData.main_data.player_character_data_list;


        // 전체 캐릭터 수
        int count = characterList.Count;

        // 현재 체력이 0인 캐릭터 수
        int deadCount = 0;

        foreach (var character in characterList)
        {
            if (character.current_health <= 0)
            {
                deadCount++;
            }
        }


        leftCharacterCount.text =
            (count- deadCount).ToString();

        deadCharacterCount.text =
            deadCount.ToString();
    }
}