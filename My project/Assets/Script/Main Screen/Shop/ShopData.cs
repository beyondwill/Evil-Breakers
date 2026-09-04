using System.Collections.Generic;

[System.Serializable]
public class ShopData
{
    public List<InventoryItem> shop_items = new();
    public List<CardData> card_items = new();

    public void Clear()
    {
        shop_items.Clear();
    }
}