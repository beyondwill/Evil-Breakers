using System.Collections.Generic;
using UnityEngine;

// 소모 아이템 분류
public enum ConsumptionItemSort
{
    InBattle,           // 전투에서 사용 가능
    InMap,              // 맵에서 사용 가능
    Both                // 둘 다 사용 가능
}

// 타겟 필요 여부
public enum TargetNeed
{
    None,               // 타겟 필요 없음
    Need                // 타겟 필요함
}

[CreateAssetMenu(menuName = "Data/Consumption Item")]
public class ConsumptionItemInfo : ItemData
{
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        itemType = ItemType.Consumable;
    }
#endif

    // 소모 아이템 분류
    public ConsumptionItemSort consumptionSort;
    public TargetNeed targetNeed;

    // 발동 효과들
    public List<ItemEffect> itemEffectList;
}