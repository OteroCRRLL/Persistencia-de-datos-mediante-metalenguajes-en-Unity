using System;

[Serializable]
public class InventorySlot
{
    public string ItemId;
    public string DisplayName;
    public int Count;
    public int MaxStackSize;

    public InventorySlot(string itemId, string displayName, int count, int maxStackSize)
    {
        ItemId = itemId;
        DisplayName = displayName;
        Count = count;
        MaxStackSize = maxStackSize;
    }

    public bool IsFull => Count >= MaxStackSize;
    public bool IsEmpty => Count <= 0;

    public int Add(int amount)
    {
        int spaceLeft = MaxStackSize - Count;
        if (amount <= spaceLeft)
        {
            Count += amount;
            return 0; // all added
        }
        else
        {
            Count = MaxStackSize;
            return amount - spaceLeft; // amount remaining
        }
    }

    public void Remove(int amount)
    {
        Count -= amount;
        if (Count < 0) Count = 0;
    }

    public void Clear()
    {
        ItemId = string.Empty;
        DisplayName = string.Empty;
        Count = 0;
        MaxStackSize = 0;
    }
}