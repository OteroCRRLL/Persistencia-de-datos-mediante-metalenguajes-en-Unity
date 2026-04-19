using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryContainer
{
    public List<InventorySlot> slots;
    public int capacity;

    public InventoryContainer(int capacity)
    {
        this.capacity = capacity;
        slots = new List<InventorySlot>(capacity);
        for (int i = 0; i < capacity; i++)
        {
            slots.Add(new InventorySlot(string.Empty, string.Empty, 0, 0));
        }
    }

    public bool TryAdd(string id, string displayName, int amount, int maxStack)
    {
        int remainingAmount = amount;

        // Try to find existing slots with the same item that are not full
        foreach (var slot in slots)
        {
            if (slot.ItemId == id && !slot.IsFull)
            {
                remainingAmount = slot.Add(remainingAmount);
                if (remainingAmount <= 0) return true;
            }
        }

        // Find empty slots for the remaining amount
        foreach (var slot in slots)
        {
            if (slot.IsEmpty || string.IsNullOrEmpty(slot.ItemId))
            {
                slot.ItemId = id;
                slot.DisplayName = displayName;
                slot.MaxStackSize = maxStack;
                slot.Count = 0;
                
                remainingAmount = slot.Add(remainingAmount);
                if (remainingAmount <= 0) return true;
            }
        }

        
        return remainingAmount == 0;
    }

    public void RemoveItem(int index, int amount)
    {
        if (index >= 0 && index < slots.Count)
        {
            slots[index].Remove(amount);
            if (slots[index].IsEmpty)
            {
                slots[index].Clear();
            }
        }
    }
}