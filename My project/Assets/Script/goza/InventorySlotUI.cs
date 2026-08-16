using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public int Index { get; private set; }


    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;


    private InventoryItem currentItem;

    private InventoryUI inventoryUI;


    private void Awake()
    {
        // 부모의 InventoryUI 가져오기
        inventoryUI =
            GetComponentInParent<InventoryUI>();
    }


    public void Init(int index)
    {
        Index = index;

        Refresh();
    }


    public void Refresh()
    {
        var slots =
            DataManager.Instance.GetBattleData.slots;


        // 안전 체크
        if (Index < 0 ||
            Index >= slots.Count)
        {
            currentItem = null;

            icon.enabled = false;

            if (amountText != null)
                amountText.text = "";

            return;
        }


        currentItem = slots[Index];


        // 빈 슬롯
        if (currentItem == null ||
            currentItem.IsEmpty)
        {
            icon.enabled = false;

            if (amountText != null)
                amountText.text = "";

            return;
        }


        // 아이콘 표시
        icon.enabled = true;
        icon.sprite = currentItem.item.icon;


        // 수량 표시
        if (amountText != null)
        {
            amountText.text =
                currentItem.amount > 1
                ? currentItem.amount.ToString()
                : "";
        }
    }


    public void OnPointerClick(
        PointerEventData eventData)
    {
        // ==========================================
        // 오른쪽 클릭
        // ==========================================

        if (eventData.button ==
            PointerEventData.InputButton.Right)
        {
            DropItem();

            return;
        }


        // ==========================================
        // 왼쪽 클릭
        // ==========================================

        if (eventData.button ==
            PointerEventData.InputButton.Left)
        {
            // 장착 / 선택 / 이동 등 나중에 추가
        }
    }


    private void DropItem()
    {
        if (currentItem == null ||
            currentItem.IsEmpty)
            return;


        var slots =
            DataManager.Instance.GetBattleData.slots;

        slots.RemoveAt(Index);
        slots.Add(new InventoryItem());
        currentItem = null;
        TooltipUI.Instance.Hide();
        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }

        DataManager.Instance.SaveData();
    }


    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (currentItem == null ||
            currentItem.IsEmpty)
            return;


        TooltipUI.Instance.Show(
            currentItem.item,
            transform as RectTransform
        );
    }


    public void OnPointerExit(
        PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }
}