using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image button_image;
    [SerializeField] private Image background_image;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject unselectableImage;
    [SerializeField] private TextMeshProUGUI specialText;

    [Header("Info")]
    [SerializeField] private int button_index;
    [SerializeField] private Sprite empty_image;


    private Action clickAction;
    private Action pointerEnterAction;
    private Action pointerExitAction;



    private void Awake()
    {
        ToggleButtonActive(true);
    }



    #region Init

    public void ButtonInit(
        int index,
        Color color,
        Sprite sprite,
        bool interactable = true)
    {
        button_index = index;

        SetColor(color);

        if (sprite == null)
            SetEmpty();
        else
            SetImage(sprite);


        SetAmount(0);

        ToggleButtonActive(interactable);

        button.onClick.RemoveAllListeners();
    }

    #endregion



    #region Click

    public void ActionAdd(Action action)
    {
        clickAction += action;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            clickAction?.Invoke();
        });
    }


    public void ActionRemove()
    {
        clickAction = null;

        button.onClick.RemoveAllListeners();
    }

    #endregion



    #region Pointer

    public void PointerEnterAdd(Action action)
    {
        pointerEnterAction += action;
    }


    public void PointerExitAdd(Action action)
    {
        pointerExitAction += action;
    }


    public void PointerEnterRemove()
    {
        pointerEnterAction = null;
    }


    public void PointerExitRemove()
    {
        pointerExitAction = null;
    }



    public void OnPointerEnter(
        PointerEventData eventData)
    {
        pointerEnterAction?.Invoke();
    }



    public void OnPointerExit(
        PointerEventData eventData)
    {
        pointerExitAction?.Invoke();
    }

    #endregion



    #region UI

    public void SetColor(Color color)
    {
        background_image.color = color;
    }



    public void SetImage(Sprite sprite)
    {
        button_image.enabled = true;
        button_image.sprite = sprite;
    }



    public void SetAmount(int amount)
    {
        if (amountText == null)
            return;


        amountText.text =
            amount > 1
            ? amount.ToString()
            : "";
    }

    public void ToggleButtonActive(bool value)
    {
        button.interactable = value;
        unselectableImage.SetActive(!value);
    }



    public void SetEmpty()
    {
        button_image.enabled = false;

        if (empty_image != null)
            button_image.sprite = empty_image;

        SetAmount(0);
    }

    public void SetText(string s)
    {
        specialText.text = s;
    }

    #endregion
}