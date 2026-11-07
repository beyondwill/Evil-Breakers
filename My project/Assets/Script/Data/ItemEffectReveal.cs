using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Item Effect/Reveal")]
public class ItemEffectReveal : ItemEffect
{
    // 회복 정도
    [SerializeField] private int revealValue;

    public override void Execute()
    {
        MapManager.Instance.RevealNodesInRange(revealValue);
    }
}
