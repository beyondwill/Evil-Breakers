using System.Collections.Generic;
using UnityEngine;

public class TurnOrderUIManager : MonoBehaviour
{
    public static TurnOrderUIManager Instance;


    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private CharacterIcon characterIconPrefab;


    private List<CharacterIcon> icons = new();



    private void Awake()
    {
        Instance = this;
    }



    // 턴 순서 생성
    public void RefreshTurnOrder(
        List<CharacterVariable> turnOrderList)
    {
        Clear();


        foreach (CharacterVariable character in turnOrderList)
        {
            CreateIcon(character);
        }
    }



    private void CreateIcon(
        CharacterVariable character)
    {
        CharacterIcon icon =
            Instantiate(
                characterIconPrefab,
                content
            );


        icon.Init(character);


        icons.Add(icon);
    }



    // 현재 턴 강조
    public void SetCurrentTurn(
        CharacterVariable character)
    {
        foreach (CharacterIcon icon in icons)
        {
            if (icon == null)
                continue;

            icon.SetCurrent(
                icon.Character == character
            );
        }
    }



    // 죽은 캐릭터 제거
    public void RemoveCharacter(
        CharacterVariable character)
    {
        for (int i = icons.Count - 1; i >= 0; i--)
        {
            CharacterIcon icon = icons[i];

            if (icon == null)
            {
                icons.RemoveAt(i);
                continue;
            }


            if (icon.Character == character)
            {
                // 리스트에서 먼저 제거
                icons.RemoveAt(i);

                // 제거 애니메이션
                icon.FadeAndShrinkIcon();

                return;
            }
        }
    }



    // 전체 삭제
    private void Clear()
    {
        foreach (CharacterIcon icon in icons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }

        icons.Clear();
    }
}