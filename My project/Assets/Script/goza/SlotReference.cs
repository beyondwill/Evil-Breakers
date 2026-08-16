using System;

[Serializable]
public struct SlotReference
{
    public Inventory inventory;

    public int slotIndex;

    public SlotReference(Inventory inventory, int slotIndex)
    {
        this.inventory = inventory;
        this.slotIndex = slotIndex;
    }

    public bool IsValid()
    {
        return inventory != null;
    }
}