using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro

public class TransferUIManager : MonoBehaviour
{
    public static TransferUIManager Instance { get; private set; }

    [SerializeField] private GameObject uiPanel;
    
    [Header("UI Slots (Assign TextMeshProUGUI components)")]
    [SerializeField] private TextMeshProUGUI[] playerSlotTexts;
    [SerializeField] private TextMeshProUGUI[] chestSlotTexts;

    private PlayerInventory currentPlayer;
    private Chest currentChest;

    public bool IsOpen => uiPanel != null && uiPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    public void OpenTransferUI(PlayerInventory player, Chest chest)
    {
        currentPlayer = player;
        currentChest = chest;
        
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            RefreshUI();
        }
        else
        {
            Debug.LogWarning("TransferUIManager UI Panel is not assigned! Doing quick transfer test via logs.");
            // If there's no UI built yet, just simulate an action for testing
            SimulateTransferForTesting();
        }
    }

    public void CloseTransferUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        currentPlayer = null;
        currentChest = null;
    }

    public void RefreshUI()
    {
        Debug.Log("UI Transfer refreshed. Player items: " + CountItems(currentPlayer.Inventory) + " | Chest items: " + CountItems(currentChest.Inventory));
        
        UpdateSlotTexts(currentPlayer.Inventory, playerSlotTexts);
        UpdateSlotTexts(currentChest.Inventory, chestSlotTexts);
    }

    private void UpdateSlotTexts(InventoryContainer inventory, TextMeshProUGUI[] textComponents)
    {
        if (textComponents == null || textComponents.Length == 0) return;

        for (int i = 0; i < textComponents.Length; i++)
        {
            if (i < inventory.slots.Count)
            {
                var slot = inventory.slots[i];
                if (slot.IsEmpty || string.IsNullOrEmpty(slot.ItemId))
                {
                    textComponents[i].text = "Empty";
                }
                else
                {
                    textComponents[i].text = $"{slot.DisplayName} x{slot.Count}";
                }
            }
            else
            {
                // UI has more texts than inventory slots
                textComponents[i].text = "LOCKED";
            }
        }
    }

    public void SimulateTransferForTesting()
    {
        if (currentPlayer == null || currentChest == null) return;
        
        // Find first item in player and move it to chest
        for (int i = 0; i < currentPlayer.Inventory.slots.Count; i++)
        {
            var slot = currentPlayer.Inventory.slots[i];
            if (!slot.IsEmpty)
            {
                // Try move
                bool success = currentChest.Inventory.TryAdd(slot.ItemId, slot.DisplayName, slot.Count, slot.MaxStackSize);
                if (success)
                {
                    currentPlayer.Inventory.RemoveItem(i, slot.Count);
                    Debug.Log($"Moved {slot.Count} {slot.DisplayName} to Chest.");
                    RefreshUI();
                    return;
                }
            }
        }
    }

    private int CountItems(InventoryContainer inv)
    {
        int total = 0;
        foreach(var slot in inv.slots)
        {
            if(!string.IsNullOrEmpty(slot.ItemId)) total += slot.Count;
        }
        return total;
    }

    public void MovePlayerToChest(int index)
    {
        if (currentPlayer == null || currentChest == null) return;
        var slot = currentPlayer.Inventory.slots[index];
        if (!slot.IsEmpty)
        {
            bool success = currentChest.Inventory.TryAdd(slot.ItemId, slot.DisplayName, 1, slot.MaxStackSize);
            if (success) currentPlayer.Inventory.RemoveItem(index, 1);
            RefreshUI();
        }
    }

    public void MoveChestToPlayer(int index)
    {
        if (currentPlayer == null || currentChest == null) return;
        var slot = currentChest.Inventory.slots[index];
        if (!slot.IsEmpty)
        {
            bool success = currentPlayer.Inventory.TryAdd(slot.ItemId, slot.DisplayName, 1, slot.MaxStackSize);
            if (success) currentChest.Inventory.RemoveItem(index, 1);
            RefreshUI();
        }
    }
}