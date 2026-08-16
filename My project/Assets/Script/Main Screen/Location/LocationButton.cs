using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 범죄 분류
public enum CrimeCaseSort
{
    Murder,
    Disappear,
    Illusion,
    Infection,
    Rage,
    MentalCollision
}

public class LocationButton : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private TextMeshProUGUI location_text;
    [SerializeField] private GameObject[] case_arr;

    // 사건 보여주기
    public void ShowCase(CrimeCaseSort CCS)
    {
        case_arr[((int)CCS)].SetActive(true);
    }
}
