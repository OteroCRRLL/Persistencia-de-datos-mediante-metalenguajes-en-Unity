using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour, IStorable
{
    [SerializeField] private int maxInventorySlots = 10;
    public InventoryContainer Inventory { get; private set; }

    private void Awake()
    {
        Inventory = new InventoryContainer(maxInventorySlots);
    }

    public void Store(IPickable item)
    {
        bool stored = Inventory.TryAdd(item.Id, item.DisplayName, 1, item.MaxStackSize);
        if (stored)
        {
            Debug.Log($"Stored 1x {item.DisplayName} into inventory. Slots used: {GetUsedSlotsCount()}");
        }
        else
        {
            Debug.LogWarning($"Inventory is full! Could not store {item.DisplayName}.");
        }
    }

    private int GetUsedSlotsCount()
    {
        int count = 0;
        foreach (var slot in Inventory.slots)
        {
            if (!string.IsNullOrEmpty(slot.ItemId))
            {
                count++;
            }
        }
        return count;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        if (!other.TryGetComponent<IPickable>(out var pickable))
            return;

        // The pickable decides what happens on pick.
        pickable.Pick(this);
    }
}
