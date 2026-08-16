using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FightManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;

        // 적이 하나도 없으면: 맵으로
        if (DataManager.Instance.GetBattleData.enemyCharacterList.Count == 0)
        {
            //SceneLoader.Load(SceneType.Map);
            SceneManager.LoadScene("Map Scene");
        }

        StartFight();
    }

    public void StartFight()
    {
        BattleCharacterManager.Instance.InitializeBattleCharacters();
        TurnManager.Instance.StartBattle();
    }

    public void EndFight(bool win)
    {

    }
}