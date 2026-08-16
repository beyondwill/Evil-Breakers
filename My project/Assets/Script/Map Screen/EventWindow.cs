using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventWindow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextMeshProUGUI eventScript;
    [SerializeField] private Image eventImage;

    [Header("Buttons")]
    [SerializeField] private Transform eventInfoBox;

    [SerializeField] private EventButton eventButtonPrefab;

    public void InitEventWindow(EventInfo eventInfo)
    {
        // 기존 버튼 삭제
        foreach (Transform child in eventInfoBox)
        {
            Destroy(child.gameObject);
        }

        // 제목, 내용
        eventTitle.text = eventInfo.event_title;
        eventScript.text = eventInfo.event_script;
        eventImage.sprite = eventInfo.event_image;

        // 선택지 생성
        foreach (EventChoice choice in eventInfo.choices)
        {
            EventButton btn = Instantiate(eventButtonPrefab, eventInfoBox);
            btn.Init(choice);
        }

        // 선택지가 하나도 없으면 자동으로 끝내기 버튼 생성
        if (eventInfo.choices.Count == 0)
        {
            EventButton btn = Instantiate(eventButtonPrefab, eventInfoBox);
            btn.InitCloseButton();
        }
    }
}