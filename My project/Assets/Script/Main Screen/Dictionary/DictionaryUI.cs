using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DictionaryUI : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image main_image;
    [SerializeField] private Image side_image;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI firstText;
    [SerializeField] private TextMeshProUGUI secondText;
    [SerializeField] private TextMeshProUGUI thirdText;
    [SerializeField] private TextMeshProUGUI forthText;
    [SerializeField] private TextMeshProUGUI fifthText;

    [Header("Bar")]
    [SerializeField] private List<GameObject> bars;

    [Header("Layout")]
    [SerializeField] private RectTransform info_box;

    private readonly List<TextMeshProUGUI> text_list =
        new List<TextMeshProUGUI>();

    private void Awake()
    {
        text_list.Add(firstText);
        text_list.Add(secondText);
        text_list.Add(thirdText);
        text_list.Add(forthText);
        text_list.Add(fifthText);

        // 이미지 원본 비율 유지
        if (main_image != null)
        {
            main_image.preserveAspect = true;
        }

        if (side_image != null)
        {
            side_image.preserveAspect = true;
        }
    }

    private void OnEnable()
    {
        ResetUI();
    }

    public void Show(DictionaryDataSO data)
    {
        if (data == null)
        {
            ResetUI();
            return;
        }

        main_image.sprite = data.main_image;
        side_image.sprite = data.side_image;

        main_image.gameObject.SetActive(data.main_image != null);
        side_image.gameObject.SetActive(data.side_image != null);

        string[] values =
        {
            data.first_text,
            data.second_text,
            data.third_text,
            data.forth_text,
            data.fifth_text
        };

        int last_text_index = -1;

        // 텍스트 표시
        for (int i = 0; i < text_list.Count; i++)
        {
            TextMeshProUGUI text = text_list[i];

            if (text == null)
                continue;

            bool has_text =
                !string.IsNullOrWhiteSpace(values[i]);

            text.text = values[i];
            text.gameObject.SetActive(has_text);

            if (has_text)
            {
                ContentSizeFitter fitter =
                    text.GetComponent<ContentSizeFitter>();

                if (fitter != null)
                {
                    // 가로는 부모 Layout Group이 결정
                    // 세로만 텍스트 내용에 맞게 조절
                    fitter.horizontalFit =
                        ContentSizeFitter.FitMode.Unconstrained;

                    fitter.verticalFit =
                        ContentSizeFitter.FitMode.PreferredSize;
                }

                // 부모 가로 영역 안에서 자동 줄바꿈
                text.enableWordWrapping = true;

                last_text_index = i;
            }
        }

        // Bar 처리
        for (int i = 0; i < bars.Count; i++)
        {
            if (bars[i] == null)
                continue;

            // 마지막 텍스트보다 Bar를 하나 더 표시
            bool show_bar =
                i <= last_text_index + 1 &&
                i < bars.Count;

            bars[i].SetActive(show_bar);
        }

        RebuildLayout();
    }

    public void ResetUI()
    {
        foreach (TextMeshProUGUI text in text_list)
        {
            if (text == null)
                continue;

            text.text = "";
            text.gameObject.SetActive(false);
        }

        foreach (GameObject bar in bars)
        {
            if (bar != null)
            {
                bar.SetActive(false);
            }
        }

        main_image.sprite = null;
        side_image.sprite = null;

        main_image.gameObject.SetActive(false);
        side_image.gameObject.SetActive(false);

        RebuildLayout();
    }

    private void RebuildLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (info_box != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(info_box);
        }
    }
}