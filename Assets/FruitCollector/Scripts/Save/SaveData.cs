using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Date and time of save
    public string saveDate;
    public float totalPlayTimeSeconds;

    // Player position
    public Vector3 playerPosition;

    // Player Inventory
    public InventoryContainer playerInventory;

    // Chests Inventories (mapped by chestId)
    public List<ChestData> chests;

    // Dropped fruits in the world
    public List<FruitWorldData> droppedFruits;

    public SaveData()
    {
        chests = new List<ChestData>();
        droppedFruits = new List<FruitWorldData>();
    }
}

[Serializable]
public class ChestData
{
    public string chestId;
    public InventoryContainer inventory;

    public ChestData(string id, InventoryContainer inv)
    {
        chestId = id;
        inventory = inv;
    }
}

[Serializable]
public class FruitWorldData
{
    public string fruitId;
    public Vector3 position;

    public FruitWorldData(string id, Vector3 pos)
    {
        fruitId = id;
        position = pos;
    }
}