using UnityEngine;

public enum ItemType
{
    Normal,
    Equipment,
    Relic,
    Consumable,
    Quest
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemData : DataEntity
{
    public string itemName;

    [TextArea]
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 1;
    public int sellPrice = 100;
}