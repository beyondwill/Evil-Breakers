using System.Collections.Generic;
using UnityEngine;

public static class CSVLoader
{
    public static List<HexMapData> LoadMapData(string path)
    {
        List<HexMapData> result = new();

        TextAsset csv = Resources.Load<TextAsset>(path);

        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] row = lines[i].Split(',');

            HexMapData data = new();

            data.id = row[0];

            data.targetNodeCount = int.Parse(row[1]);
            data.extraEdgeCount = int.Parse(row[2]);

            data.lessLenFromBoss = int.Parse(row[3]);
            data.lessLenEliteBattle = int.Parse(row[4]);
            data.lessLenShop = int.Parse(row[5]);

            data.minShopDistance = int.Parse(row[6]);

            data.normalBattleCount = int.Parse(row[7]);
            data.eliteBattleCount = int.Parse(row[8]);

            data.shopCount = int.Parse(row[9]);
            data.obstacleCount = int.Parse(row[10]);
            data.mask = row[11];

            result.Add(data);
        }

        return result;
    }
}
