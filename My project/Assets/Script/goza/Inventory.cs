using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Info")]
    public InventoryType inventoryType;

    [SerializeField]
    private int slotCount = 12;

    public int SlotCount => slotCount;

    [SerializeField] private List<InventoryItem> slots;

    #region Init

    public void Init(int count)
    {
        slotCount = count;
    }

    private void Awake()
    {

    }

    private void Start()
    {
        slots = DataManager.Instance.GetBattleData.slots;
        Init(slotCount);
    }

    #endregion

    #region Getter

    public InventoryItem GetItem(int index)
    {
        if (!IsValidIndex(index))
            return null;


        return slots[index];
    }

    #endregion





    #region Setter

    public void SetItem(int index, InventoryItem item)
    {
        if (!IsValidIndex(index))
            return;


        if (item == null)
        {
            slots[index] = new InventoryItem();
            return;
        }


        slots[index] = item;
    }



    public void Clear(int index)
    {
        if (!IsValidIndex(index))
            return;


        slots[index].Clear();
    }

    #endregion


     


    #region Add

    public int Add(ItemData item, int amount = 1)
    {
        Debug.Log($"Add : {item.name} x{amount}");
        Debug.Log(System.Environment.StackTrace);

        if (item == null || amount <= 0)
            return amount;

        // 기존 스택 채우기
        if (item.maxStack > 1)
        {
            foreach (InventoryItem slot in slots)
            {
                if (slot.IsEmpty)
                    continue;

                if (slot.item != item)
                    continue;

                if (slot.amount >= item.maxStack)
                    continue;

                int remain = item.maxStack - slot.amount;
                int add = Mathf.Min(remain, amount);

                slot.amount += add;
                amount -= add;

                if (amount <= 0)
                    return 0;
            }
        }

        // 빈 슬롯에 새로 추가
        foreach (InventoryItem slot in slots)
        {
            if (!slot.IsEmpty)
                continue;

            int add = Mathf.Min(item.maxStack, amount);

            slot.Set(item, add);

            amount -= add;

            if (amount <= 0)
                return 0;
        }

        // 인벤토리가 부족해서 못 넣은 남은 개수 반환
        return amount;
    }

    #endregion





    #region Remove

    public bool Remove(int index, int amount = 1)
    {
        if (!IsValidIndex(index))
            return false;


        InventoryItem slot = slots[index];


        if (slot.IsEmpty)
            return false;



        slot.amount -= amount;



        if (slot.amount <= 0)
        {
            slot.Clear();
        }


        return true;
    }

    #endregion





    #region Swap

    public void Swap(int a, int b)
    {
        if (!IsValidIndex(a))
            return;


        if (!IsValidIndex(b))
            return;



        InventoryItem temp = slots[a];


        slots[a] = slots[b];


        slots[b] = temp;
    }

    #endregion





    #region Move

    public void Move(int from, int to)
    {
        if (!IsValidIndex(from))
            return;


        if (!IsValidIndex(to))
            return;



        if (!slots[to].IsEmpty)
            return;



        slots[to] = slots[from];


        slots[from] = new InventoryItem();
    }

    #endregion





    #region Helper

    public bool IsEmpty(int index)
    {
        if (!IsValidIndex(index))
            return true;


        return slots[index].IsEmpty;
    }



    public bool IsFull()
    {
        foreach (InventoryItem slot in slots)
        {
            if (slot.IsEmpty)
                return false;
        }


        return true;
    }



    public bool CanAdd(ItemData item)
    {
        if (!IsFull())
            return true;



        if (item.maxStack <= 1)
            return false;



        foreach (InventoryItem slot in slots)
        {
            if (slot.item == item &&
                slot.amount < item.maxStack)
            {
                return true;
            }
        }


        return false;
    }



    private bool IsValidIndex(int index)
    {
        return index >= 0 &&
               index < slotCount;
    }

    #endregion

    public List<InventoryItem> GetSlots()
    {
        return slots;
    }
}