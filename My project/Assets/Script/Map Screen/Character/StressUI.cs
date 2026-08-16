using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StressUI : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image stressImage;                     // 스트레스 이미지

    [Header("Color")]
    [SerializeField] private Color non_stress_color;                // 스트레스 아닌 상태 색상
    [SerializeField] private Color stress_color;                    // 스트레스 상태 색상

    // 스트레스 색상 설정
    public void StressInit(bool is_stress)
    {
        stressImage.color = is_stress ? stress_color : non_stress_color;
    }
}