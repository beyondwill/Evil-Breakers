using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DictionaryUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI firstText;
    [SerializeField] private TextMeshProUGUI secondText;
    [SerializeField] private TextMeshProUGUI thirdText;
    [SerializeField] private TextMeshProUGUI forthText;
    [SerializeField] private TextMeshProUGUI fifthText;
    [SerializeField] private GameObject lowestBar;
    [SerializeField] private List<GameObject> bars;

    void OnEnable()
    {
        ShowInit();
    }

    // 지역 보여주기
    public void ShowLocationInfo()
    {
        foreach (GameObject bar in bars)
        {
            bar.SetActive(true);
        }
        lowestBar.SetActive(true);
    }

    // 괴물 보여주기
    public void ShowMonsterInfo()
    {
        foreach (GameObject bar in bars)
        {
            bar.SetActive(true);
        }
        lowestBar.SetActive(true);
    }

    // NPC 보여주기
    public void ShowNPCInfo()
    {
        foreach (GameObject bar in bars)
        {
            bar.SetActive(true);
        }
        lowestBar.SetActive(false);
    }

    // 속성 보여주기
    public void ShowAttributeInfo()
    {
        foreach (GameObject bar in bars)
        {
            bar.SetActive(true);
        }
        lowestBar.SetActive(true);
    }

    public void ShowInit()
    {
        ResetUI();
    }

    public void ResetUI()
    {
        firstText.text = "";
        secondText.text = "";
        thirdText.text = "";
        forthText.text = "";
        fifthText.text = "";
        foreach(GameObject bar in bars)
        {
            bar.SetActive(false);
        }
    }
}