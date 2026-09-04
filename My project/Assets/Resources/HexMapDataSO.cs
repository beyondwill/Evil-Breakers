using UnityEngine;

[CreateAssetMenu(menuName = "Hex/Map Data")]
public class HexMapDataSO : DataEntity
{
    public string location_name;

    public int targetNodeCount;
    public int extraEdgeCount;

    public int lessLenFromBoss;
    public int lessLenEliteBattle;
    public int lessLenShop;

    public int minShopDistance;

    public int normalBattleCount;
    public int eliteBattleCount;

    public int shopCount;
    public int obstacleCount;
    public int bossCount;

    public string mask;
}