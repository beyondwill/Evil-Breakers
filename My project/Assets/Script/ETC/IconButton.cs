
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

    [Header("Selection")]
    [SerializeField] private Image selectedFrame;

    [Header("Info")]
    [SerializeField] private int button_index;
    [SerializeField] private Sprite empty_image;

    [Header("Config")]
    [SerializeField] private ItemColorConfig itemColorConfig;

    private Action clickAction;
    private Action pointerEnterAction;
    private Action pointerExitAction;

    private ItemData currentItem;
    private CharacterInfo currentCharacter;


    private void Awake()
    {
        ToggleButtonActive(true);
        SetSelected(false);
    }


    #region Init

    public void ButtonInit(
        int index,
        Color color,
        Sprite sprite,
        bool interactable = true)
    {
        button_index = index;

        currentItem = null;
        currentCharacter = null;

        SetColor(color);

        if (sprite == null)
            SetEmpty();
        else
            SetImage(sprite);

        SetAmount(0);
        SetText("");

        ToggleButtonActive(interactable);

        ActionRemove();

        PointerEnterRemove();
        PointerExitRemove();

        SetSelected(false);
    }

    #endregion


    #region Click

    public void ActionAdd(Action action)
    {
        clickAction = action;

        //Debug.Log(
        //    $"[IconButton] 이벤트 등록: {gameObject.name}, ID: {GetInstanceID()}"
        //);

        //if (button == null)
        //{
        //    Debug.LogError(
        //        $"[IconButton] Button이 연결되지 않았습니다: {gameObject.name}"
        //    );
        //    return;
        //}

        // 기존 IconButton 이벤트만 제거
        button.onClick.RemoveListener(OnButtonClicked);

        // 클릭 이벤트 등록
        button.onClick.AddListener(OnButtonClicked);
    }


    private void OnButtonClicked()
    {
        Debug.Log(
            $"[IconButton] 클릭: {gameObject.name}, ID: {GetInstanceID()}"
        );

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();

        if (clickAction == null)
        {
            Debug.LogError(
                $"[IconButton] clickAction이 null입니다: {gameObject.name}"
            );
            return;
        }

        Debug.Log("[IconButton] 콜백 실행 직전");

        clickAction.Invoke();

        Debug.Log("[IconButton] 콜백 실행 완료");
    }


    public void ActionRemove()
    {
        clickAction = null;

        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
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


    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnterAction?.Invoke();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        pointerExitAction?.Invoke();
    }

    #endregion


    #region Data

    // ===================================
    // 아이템 데이터 연결
    // ===================================

    public void SetItemData(ItemData item)
    {
        currentItem = item;
        currentCharacter = null;

        SetColor(itemColorConfig.FindRarityColor(currentItem.itemRarity));

        PointerEnterRemove();
        PointerExitRemove();

        SetText("");

        if (item == null)
        {
            SetEmpty();
            return;
        }

        SetImage(item.icon);

        PointerEnterAdd(() =>
        {
            if (TooltipUI.Instance != null)
            {
                TooltipUI.Instance.Show(
                    currentItem,
                    transform as RectTransform
                );
            }
        });

        PointerExitAdd(() =>
        {
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Hide();
        });
    }


    // ===================================
    // 캐릭터 정보 연결
    // ===================================

    public void SetCharacterData(CharacterInfo character)
    {
        currentCharacter = character;
        currentItem = null;

        PointerEnterRemove();
        PointerExitRemove();

        SetAmount(0);

        if (character == null)
        {
            SetEmpty();
            SetText("");
            return;
        }

        SetImage(character.character_icon);
    }


    // ===================================
    // 플레이어 캐릭터 데이터 연결
    // ===================================

    public void SetCharacterData(PlayerCharacterData character)
    {
        if (character == null)
        {
            SetCharacterData((CharacterInfo)null);
            return;
        }

        SetCharacterData(character.player_character_info);
    }


    // 현재 아이템 반환
    public ItemData GetCurrentItem()
    {
        return currentItem;
    }


    // 현재 캐릭터 반환
    public CharacterInfo GetCurrentCharacter()
    {
        return currentCharacter;
    }

    #endregion


    #region UI

    public void SetColor(Color color)
    {
        if (background_image != null)
            background_image.color = color;
    }


    public void SetImage(Sprite sprite)
    {
        if (button_image == null)
            return;

        button_image.enabled = sprite != null;
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
        if (button != null)
            button.interactable = value;

        if (unselectableImage != null)
            unselectableImage.SetActive(!value);

        //Debug.Log(
        //    $"[IconButton] {gameObject.name} / Interactable: " +
        //    (button != null ? button.interactable.ToString() : "Button Null")
        //);
    }


    public void SetEmpty()
    {
        button_image.enabled = true;

        button_image.sprite = empty_image;

        SetAmount(0);
    }

    public void SetNone()
    {
        button_image.enabled = true;

        button_image.sprite = null;

        SetAmount(0);
    }


    public void SetText(string s)
    {
        if (specialText != null)
        {
            specialText.text = s;

            //Debug.Log(
            //    $"[IconButton] {gameObject.name} 텍스트 설정: {s}"
            //);
        }
        else
        {
            Debug.LogError(
                $"[IconButton] specialText가 연결되지 않았습니다: {gameObject.name}"
            );
        }
    }


    public void SetSelected(bool value)
    {
        if (selectedFrame == null)
            return;

        selectedFrame.enabled = value;
    }

    #endregion
}
