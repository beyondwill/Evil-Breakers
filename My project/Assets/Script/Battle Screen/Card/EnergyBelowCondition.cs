using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnergyBelowCondition",
    menuName = "Card/Conditions/Energy Below"
)]
public class EnergyBelowCondition : CardCondition
{
    [SerializeField]
    private int energyThreshold = 2;


    public override bool Check(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardData card)
    {
        if (caster == null)
            return false;

        if (caster is not PlayerCharacterVariable player)
            return false;

        return player.current_energy <= energyThreshold;
    }
}