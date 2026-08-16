using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Visual/Target Impact")]
public class TargetImpactVisual : CardVisual
{
    public GameObject effectPrefab;

    public override void Play(
        CharacterVariable caster,
        List<CharacterVariable> targets)
    {
        foreach (CharacterVariable target in targets)
        {
            if (target == null ||
                target.characterView == null)
                continue;

            EffectManager.Instance.PlayEffect(
                effectPrefab,
                target.characterView.GetEffectPosition()
            );
        }
    }
}