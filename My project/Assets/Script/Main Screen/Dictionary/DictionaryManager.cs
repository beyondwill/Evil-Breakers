using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DictionaryManager : MonoBehaviour
{
    [Header("Dictionary UI")]
    [SerializeField] private DictionaryUI dictionary_ui;

    [Header("Dictionary Data")]
    [SerializeField] private List<DictionaryDataSO> dictionary_data_list;

    [Header("Left List")]
    [SerializeField] private Transform dictionary_content;
    [SerializeField] private GameObject dictionary_button_prefab;

    [Header("Category Button")]
    [SerializeField] private Button location_button;
    [SerializeField] private Button monster_button;
    [SerializeField] private Button npc_button;
    [SerializeField] private Button attribute_button;

    private DictionaryType current_type;

    private void Start()
    {
        location_button.onClick.AddListener(
            () => ShowDictionary(DictionaryType.Location));

        monster_button.onClick.AddListener(
            () => ShowDictionary(DictionaryType.Monster));

        npc_button.onClick.AddListener(
            () => ShowDictionary(DictionaryType.NPC));

        attribute_button.onClick.AddListener(
            () => ShowDictionary(DictionaryType.Attribute));

        // 처음에는 지역 표시
        ShowDictionary(DictionaryType.Location);
    }

    // 카테고리 변경
    public void ShowDictionary(DictionaryType type)
    {
        current_type = type;

        // 기존 왼쪽 목록 삭제
        ClearDictionaryButton();

        // 오른쪽 정보 초기화
        dictionary_ui.ResetUI();

        // 해당 카테고리만 표시
        foreach (DictionaryDataSO data in dictionary_data_list)
        {
            if (data == null)
                continue;

            if (data.dictionary_type != current_type)
                continue;

            CreateDictionaryButton(data);
        }
    }

    public void ShowDictionary()
    {
        ShowDictionary(current_type);
    }

    public void Add(DictionaryDataSO DDSO)
    {
        dictionary_data_list.Add(DDSO);
    }

    // 왼쪽 버튼 생성
    private void CreateDictionaryButton(DictionaryDataSO data)
    {
        GameObject buttonObject =
            Instantiate(
                dictionary_button_prefab,
                dictionary_content);

        Button button =
            buttonObject.GetComponent<Button>();

        TextMeshProUGUI buttonText =
            buttonObject.GetComponentInChildren<TextMeshProUGUI>();

        // dictionary_name은 왼쪽 버튼에만 사용
        if (buttonText != null)
        {
            buttonText.text = data.dictionary_name;
        }

        button.onClick.AddListener(
            () => ShowDictionaryInfo(data));
    }

    // 오른쪽 정보 표시
    private void ShowDictionaryInfo(DictionaryDataSO data)
    {
        dictionary_ui.Show(data);
    }

    // 기존 버튼 제거
    private void ClearDictionaryButton()
    {
        for (int i = dictionary_content.childCount - 1; i >= 0; i--)
        {
            Destroy(
                dictionary_content
                    .GetChild(i)
                    .gameObject);
        }
    }
}