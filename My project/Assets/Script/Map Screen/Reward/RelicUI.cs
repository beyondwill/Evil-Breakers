using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RelicUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public static Action<int> OnRelicClicked;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI relicNameText;
    [SerializeField] private TextMeshProUGUI relicRankText;
    [SerializeField] private Image relicImage;
    [SerializeField] private TextMeshProUGUI relicScriptText;
    [SerializeField] private TextMeshProUGUI relicStoryText;

    [SerializeField] private string common;
    [SerializeField] private string rare;
    [SerializeField] private string epic;

    private int index;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.15f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);

        OnRelicClicked?.Invoke(index);
    }

    public void RelicShowInfo(RelicInfo info, int index)
    {
        this.index = index;

        relicNameText.text = info.relic_name;
        relicImage.sprite = info.relic_image;
        relicScriptText.text = info.relic_script;
        relicStoryText.text = "\" " + info.relic_story + "\"";

        relicRankText.text = "[ ";

        switch (info.sort)
        {
            case RelicSort.Common: relicRankText.text += common; break;
            case RelicSort.Rare: relicRankText.text += rare; break;
            case RelicSort.Epic: relicRankText.text += epic; break;
        }

        relicRankText.text += " ]";
    }
}