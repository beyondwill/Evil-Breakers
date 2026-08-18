using UnityEngine;

public enum DictionaryType
{
    Location,
    Monster,
    NPC,
    Attribute
}

[CreateAssetMenu(fileName = "DictionaryData", menuName = "Dictionary/Dictionary Data")]
public class DictionaryDataSO : ScriptableObject
{
    [Header("분류")]
    public DictionaryType dictionary_type;

    [Header("왼쪽 버튼 이름")]
    public string dictionary_name;

    [Header("이미지")]
    public Sprite main_image;
    public Sprite side_image;

    [Header("정보")]
    [TextArea(1, 5)]
    public string first_text;

    [TextArea(1, 5)]
    public string second_text;

    [TextArea(1, 5)]
    public string third_text;

    [TextArea(1, 10)]
    public string forth_text;

    [TextArea(1, 10)]
    public string fifth_text;
}