#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapDataBuilder
{
#if UNITY_EDITOR
    [MenuItem("Tools/Hex/Build Map Data From CSV")]
    public static void Build()
    {
        List<HexMapData> datas =
            CSVLoader.LoadMapData("MapData");

        foreach (var data in datas)
        {
            CreateSO(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void CreateSO(HexMapData data)
    {
        HexMapDataSO so =
            ScriptableObject.CreateInstance<HexMapDataSO>();

        so.data_code = data.id;
        so.location_name = data.location_name;

        so.targetNodeCount = data.targetNodeCount;
        so.extraEdgeCount = data.extraEdgeCount;

        so.lessLenFromBoss = data.lessLenFromBoss;
        so.lessLenEliteBattle = data.lessLenEliteBattle;
        so.lessLenShop = data.lessLenShop;

        so.minShopDistance = data.minShopDistance;

        so.normalBattleCount = data.normalBattleCount;
        so.eliteBattleCount = data.eliteBattleCount;

        so.shopCount = data.shopCount;
        so.obstacleCount = data.obstacleCount;
        so.bossCount = data.bossCount;
        so.mask = NormalizeMask(data.mask);

        string path = $"Assets/MapData_{data.id}.asset";

        AssetDatabase.CreateAsset(so, path);
    }

    // 마스크 정규화
    static string NormalizeMask(string mask, int expectedLines = -1)
    {
        if (string.IsNullOrEmpty(mask))
            return mask;

        mask = mask.Replace("|", "\n");

        var lines = mask.Split('\n')
                         .Select(l => l.Trim())
                         .ToArray();

        if (expectedLines > 0 && lines.Length != expectedLines)
        {
            Debug.LogError($"Mask line count mismatch: {lines.Length} / {expectedLines}");
        }

        return string.Join("\n", lines);
    }
#endif
}