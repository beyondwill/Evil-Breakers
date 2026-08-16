using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCharacter", menuName = "Character/Player Character")]
public class PlayerCharacterInfo : CharacterInfo
{
    public List<ItemData> weaponItemDataList;
    public List<ItemData> armorItemDataList;
    public List<Color> levelcolorList;


#if UNITY_EDITOR

    protected override void OnValidate()
    {
        // DataEntity의 OnValidate()도 실행
        base.OnValidate();

        // PlayerCharacterInfo의 대화 자동 생성
        InitializeAllDialogues();
    }


    private void InitializeAllDialogues()
    {
        if (dialogues == null)
            dialogues = new List<CharacterDialogue>();


        // Situation enum에 존재하는 모든 항목 확인
        foreach (Situation situation in Enum.GetValues(typeof(Situation)))
        {
            // 이미 해당 Situation이 있는지 확인
            bool exists = dialogues.Exists(
                x => x != null &&
                     x.situation == situation
            );


            // 없으면 자동 생성
            if (!exists)
            {
                dialogues.Add(
                    new CharacterDialogue
                    {
                        situation = situation,
                        dialogue = new List<string>()
                    }
                );
            }
        }
    }

#endif
}