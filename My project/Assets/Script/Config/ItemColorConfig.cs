
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity
{
    None,               // 없음
    Common,             // 일반
    Advanced,           // 고급
    Rare,               // 희귀
    Epic,               // 에픽
    Legendary,          // 전설
    Myth                // 신화
}

// =========================================================
// 희귀도별 색상
// =========================================================

[Serializable]
public class RarityAndColor
{
    public ItemRarity itemRarity;

    public Color color = Color.white;
}


// =========================================================
// 아이템 색상 설정
// =========================================================

[CreateAssetMenu(
    menuName = "Config/ItemColorConfig"
)]
public class ItemColorConfig : ScriptableObject
{
    // =====================================================
    // 희귀도별 색상
    // =====================================================

    [Header("희귀도별 색상")]
    public List<RarityAndColor> itemColorList = new();


    // =====================================================
    // Find
    // =====================================================

    /// <summary>
    /// 해당 아이템 희귀도의 색상을 가져온다.
    /// </summary>
    public Color FindRarityColor(
        ItemRarity itemRarity)
    {
        RarityAndColor result =
            itemColorList.Find(
                x =>
                    x != null &&
                    x.itemRarity == itemRarity
            );


        if (result != null)
            return result.color;


        // 해당 희귀도가 등록되어 있지 않을 경우
        return Color.white;
    }


    // =====================================================
    // Validate
    // =====================================================

    private void OnValidate()
    {
        ValidateRarityColor();
    }


    // =====================================================
    // 희귀도 색상 검증
    // =====================================================

    private void ValidateRarityColor()
    {
        if (itemColorList == null)
        {
            itemColorList =
                new List<RarityAndColor>();
        }


        // -------------------------------------------------
        // 잘못된 Enum 값 제거
        // -------------------------------------------------

        itemColorList.RemoveAll(
            x =>
                x == null ||
                !Enum.IsDefined(
                    typeof(ItemRarity),
                    x.itemRarity)
        );


        // -------------------------------------------------
        // 모든 ItemRarity 자동 추가
        // -------------------------------------------------

        foreach (
            ItemRarity itemRarity
            in Enum.GetValues(
                typeof(ItemRarity)))
        {
            if (itemColorList.Exists(
                    x =>
                        x.itemRarity ==
                        itemRarity))
            {
                continue;
            }


            itemColorList.Add(
                new RarityAndColor
                {
                    itemRarity =
                        itemRarity,

                    color =
                        Color.white
                }
            );
        }


        // -------------------------------------------------
        // 희귀도 순서 정렬
        // -------------------------------------------------

        itemColorList.Sort(
            (a, b) =>
                a.itemRarity.CompareTo(
                    b.itemRarity)
        );
    }
}
