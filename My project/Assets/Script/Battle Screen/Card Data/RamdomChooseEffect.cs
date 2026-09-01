using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/RandomChoose")]
public class RandomChooseEffect : CardEffect
{
    [SerializeField]
    public List<CardEffect> effects;


    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning(
                "RandomChooseEffect : 선택할 Effect가 없습니다."
            );

            return;
        }


        // -------------------------------------------------
        // NULL Effect 제외
        // -------------------------------------------------

        List<CardEffect> validEffects =
            new List<CardEffect>();


        foreach (CardEffect effect in effects)
        {
            if (effect != null)
            {
                validEffects.Add(effect);
            }
        }


        if (validEffects.Count == 0)
        {
            Debug.LogWarning(
                "RandomChooseEffect : 유효한 Effect가 없습니다."
            );

            return;
        }


        // -------------------------------------------------
        // 랜덤 선택
        // -------------------------------------------------

        int randomIndex =
            Random.Range(
                0,
                validEffects.Count
            );


        CardEffect selectedEffect =
            validEffects[randomIndex];


        Debug.Log(
            $"[RandomChoose] " +
            $"선택된 Effect = {selectedEffect.name}"
        );


        // -------------------------------------------------
        // 선택된 Effect 실행
        // -------------------------------------------------

        selectedEffect.Execute(
            caster,
            targets,
            entry,
            card
        );
    }
}