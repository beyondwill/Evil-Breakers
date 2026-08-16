using System.Collections.Generic;
using UnityEngine;

public class CharactersStand : MonoBehaviour
{
    [SerializeField] private List<CharacterView> playerCharacterViewList = new();
    [SerializeField] private List<CharacterView> enemyCharacterViewList = new();


    public List<CharacterView> CCA => playerCharacterViewList;


    public void AddCharacterView(CharacterView view)
    {
        if (view.GetCharacterVariable.is_player_character)
        {
            playerCharacterViewList.Add(view);
        }
        else
        {
            enemyCharacterViewList.Add(view);
        }
    }


    public List<CharacterView> GetCharacterViewList(CardTarget cardTarget)
    {
        switch (cardTarget)
        {
            case CardTarget.Ally:
                return playerCharacterViewList;

            case CardTarget.Enemy:
                return enemyCharacterViewList;

            case CardTarget.Any:
                {
                    List<CharacterView> list = new();

                    list.AddRange(playerCharacterViewList);
                    list.AddRange(enemyCharacterViewList);

                    return list;
                }

            default:
                return null;
        }
    }
}