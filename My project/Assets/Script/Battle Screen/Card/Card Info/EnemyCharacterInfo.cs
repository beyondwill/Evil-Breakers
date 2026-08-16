// PlayerCharacterInfo
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCharacter", menuName = "Character/Enemy Character")]
public class EnemyCharacterInfo : CharacterInfo
{
    public List<CardData> enemy_card_info_list;
}