using System;

public enum InventoryType
{
    Character,
    Shared
}

[Serializable]
public class InventoryItem
{
    public ItemData item;
    public int amount;
    public bool IsEmpty => item == null;

    public InventoryItem()
    {

    }

    public InventoryItem(ItemData item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public void Set(ItemData item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }

    public InventoryItem Clone()
    {
        return new InventoryItem()
        {
            item = item,
            amount = amount
        };
    }
}