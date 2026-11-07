using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item Effect/More time")]
public class MoreTimeEffect : ItemEffect
{
    // 회복 정도
    [SerializeField] private int timeValue;

    public override void Execute()
    {
        DataManager.Instance.GetBattleData.AddTime(timeValue);
    }
}
