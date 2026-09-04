using System;
using System.Collections.Generic;
using UnityEngine;


// =========================================================
// 직업별 색상
// =========================================================

[Serializable]
public class ClassAndColor
{
    public CharacterClass characterClass;
    public Color color = Color.white;
}


// =========================================================
// 캐릭터 색상 설정
// =========================================================

[CreateAssetMenu(
    menuName = "Config/CharacterColorConfig"
)]
public class CharacterColorConfig : ScriptableObject
{
    // =====================================================
    // 캐릭터 직업별 색상
    // =====================================================

    [Header("캐릭터 직업별 색상")]
    public List<ClassAndColor> classColorList = new();


    // =====================================================
    // Find
    // =====================================================

    /// <summary>
    /// 해당 캐릭터 직업의 색상을 가져온다.
    /// </summary>
    public Color FindClassColor(
        CharacterClass characterClass)
    {
        ClassAndColor result =
            classColorList.Find(
                x =>
                    x != null &&
                    x.characterClass == characterClass
            );


        if (result != null)
            return result.color;


        // 해당 직업이 등록되어 있지 않을 경우
        return Color.white;
    }


    // =====================================================
    // Validate
    // =====================================================

    private void OnValidate()
    {
        ValidateClassColor();
    }


    // =====================================================
    // 직업 색상 검증
    // =====================================================

    private void ValidateClassColor()
    {
        if (classColorList == null)
        {
            classColorList =
                new List<ClassAndColor>();
        }


        // -------------------------------------------------
        // 잘못된 Enum 값 제거
        // -------------------------------------------------

        classColorList.RemoveAll(
            x =>
                x == null ||
                !Enum.IsDefined(
                    typeof(CharacterClass),
                    x.characterClass)
        );


        // -------------------------------------------------
        // 모든 CharacterClass 자동 추가
        // -------------------------------------------------

        foreach (
            CharacterClass characterClass
            in Enum.GetValues(
                typeof(CharacterClass)))
        {
            if (classColorList.Exists(
                    x =>
                        x.characterClass ==
                        characterClass))
            {
                continue;
            }


            classColorList.Add(
                new ClassAndColor
                {
                    characterClass =
                        characterClass,

                    color =
                        Color.white
                }
            );
        }


        // -------------------------------------------------
        // 직업 순서 정렬
        // -------------------------------------------------

        classColorList.Sort(
            (a, b) =>
                a.characterClass.CompareTo(
                    b.characterClass)
        );
    }
}