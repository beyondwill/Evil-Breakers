using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Auto Generate")]
    [SerializeField] private Transform slotParent;
    [SerializeField] private InventorySlotUI slotPrefab;


    private readonly List<InventorySlotUI> slotUIs = new();


    private void Start()
    {
        Init();
    }


    public void Init()
    {
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }


        slotUIs.Clear();


        int count =
            DataManager.Instance.GetBattleData.slots.Count;


        for (int i = 0; i < count; i++)
        {
            InventorySlotUI slot =
                Instantiate(slotPrefab, slotParent);


            slot.Init(i);


            slotUIs.Add(slot);
        }
    }


    public void Refresh()
    {
        foreach (InventorySlotUI slot in slotUIs)
        {
            slot.Refresh();
        }
    }
}