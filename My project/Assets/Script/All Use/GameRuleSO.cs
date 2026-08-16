using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Rule")]
public class GameRuleSO : ScriptableObject
{
    public List<int> expPerLevelList;           // 레벨 별 exp
    public List<string> NamePerLevelList;       // 레벨 별 호칭
}