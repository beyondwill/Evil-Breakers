using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC", menuName = "Character/NPC")]
public class NPCInfo : DataEntity
{
    [Header("Image")]
    public Sprite sprite;                   // 이미지

    [Header("Info")]
    public string characterName;            // 캐릭터 이름
    public string attribute;                // 캐릭터 속성
    public string areas;                    // 캐릭터 출몰 지역

    [TextArea]
    public string characterInfo;            // 캐릭터 정보
}
