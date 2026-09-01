using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RelicSort
{
    Common,
    Rare,
    Epic
}

[CreateAssetMenu(fileName = "Relic", menuName = "Relic/Relic Info")]
public class RelicInfo : DataEntity
{
    public string relic_name;                           // 유물 이름
    public Sprite relic_image;                          // 유물 이미지
    [TextArea]
    public string relic_script;                         // 유물 설명
    [TextArea]
    public string relic_story;                          // 유물 이야기
    public RelicSort sort;
}