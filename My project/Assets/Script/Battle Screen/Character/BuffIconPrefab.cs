using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconPrefab : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private Image buff_image;
    [SerializeField] private TextMeshProUGUI buff_count;

    [SerializeField] private StatConfig statConfig;

    // 기본 텍스트 색상
    [SerializeField] private Color normalColor = Color.white;

    // 디버프 텍스트 색상
    [SerializeField] private Color negativeColor = Color.blue;


    public void BuffInit(CharacterBuffValue CBV)
    {
        // 버프 아이콘
        buff_image.sprite =
            statConfig.FindBuff(CBV.type).buffIcon;


        // 버프 수치
        buff_count.text =
            CBV.value.ToString();


        // 음수면 파란색
        if (CBV.value < 0)
        {
            buff_count.color =
                negativeColor;
        }
        else
        {
            buff_count.color =
                normalColor;
        }
    }
}