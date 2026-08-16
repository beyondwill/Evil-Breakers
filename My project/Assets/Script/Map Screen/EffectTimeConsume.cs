using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EventEffect/Time Consume")]
public class EffectTimeConsume : EventEffect
{
    public int amount;

    public override void Execute()
    {
        DataManager.Instance.GetBattleData.ReduceTime(amount);
    }
}