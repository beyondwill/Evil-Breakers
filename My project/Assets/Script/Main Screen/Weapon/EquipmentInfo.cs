using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSort
{
    Weapon,
    Armor,
    Accessories
}

[CreateAssetMenu(menuName = "Data/Equipment")]
public class EquipmentInfo : ItemData
{
    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            itemType = ItemType.Equipment;
        }
    #endif

    public EquipmentSort equipment_sort;

    // 장비가 제공하는 기본 스탯
    public List<CharacterBaseStatValue> baseStatList = new();
}