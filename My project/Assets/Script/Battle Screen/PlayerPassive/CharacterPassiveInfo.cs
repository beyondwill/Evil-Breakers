using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Passive", menuName = "Passive/Character Passive Info")]

public class CharacterPassiveInfo : ScriptableObject
{
    public List<PassiveEvent> passiveEventList;
}
