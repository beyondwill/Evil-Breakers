using System.Collections.Generic;
using UnityEngine;

// 소모 아이템 분류
public enum RelicItemSort
{
    InBattle,           // 전투에서 사용 가능
    InMap               // 맵에서 사용 가능
}


[CreateAssetMenu(menuName = "Data/Relic Item")]
public class RelicItemInfo : ItemData
{
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        itemType = ItemType.Relic;
    }
#endif

    // 소모 아이템 분류
    public RelicItemSort relicSort;
}