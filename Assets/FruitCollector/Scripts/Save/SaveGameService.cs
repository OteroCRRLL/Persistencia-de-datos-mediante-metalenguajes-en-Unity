using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class SaveGameService : MonoBehaviour
{
    private enum SaveFormat { JSON, XML }
    [SerializeField] private SaveFormat currentFormat = SaveFormat.JSON;

    private const string SAVE_FILE_NAME = "savegame";
    private string GetSaveFilePath() => Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}.{currentFormat.ToString().ToLower()}");
    private string GetBackupFilePath() => Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}_backup.{currentFormat.ToString().ToLower()}");

    // Static instance to easily retrieve playtime
    public static SaveGameService Instance { get; private set; }
    public float currentSessionPlayTime { get; private set; } = 0f;
    private float previousPlayTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadGame();
    }

    private void Update()
    {
        currentSessionPlayTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.L))
        {
            DeleteSaveData();
            ResetGameToDefault();
            previousPlayTime = 0f;
            currentSessionPlayTime = 0f;
        }
    }

    private void DeleteSaveData()
    {
        string savePathJson = Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}.json");
        string backupPathJson = Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}_backup.json");
        string savePathXml = Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}.xml");
        string backupPathXml = Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_NAME}_backup.xml");

        if (File.Exists(savePathJson)) File.Delete(savePathJson);
        if (File.Exists(backupPathJson)) File.Delete(backupPathJson);
        if (File.Exists(savePathXml)) File.Delete(savePathXml);
        if (File.Exists(backupPathXml)) File.Delete(backupPathXml);

        Debug.Log("Save data files deleted. Game reset.");
    }

    public void SaveGame()
    {
        Debug.Log($"Saving in format {currentFormat}...");
        
        SaveData data = GatherSaveData();
        
        string savePath = GetSaveFilePath();
        string backupPath = GetBackupFilePath();

        // 1. Backup if exists
        if (File.Exists(savePath))
        {
            try
            {
                File.Copy(savePath, backupPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating backup: {e.Message}");
            }
        }

        // 2. Save
        try
        {
            if (currentFormat == SaveFormat.JSON)
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(savePath, json);
            }
            else if (currentFormat == SaveFormat.XML)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }
            }
            Debug.Log($"Game saved successfully at {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game! Trying to restore backup... Error: {e.Message}");
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, savePath, true);
                Debug.LogWarning("Backup restored.");
            }
        }
    }

    private void LoadGame()
    {
        string savePath = GetSaveFilePath();
        
        if (!File.Exists(savePath))
        {
            Debug.Log("First time playing or no save found. Creating new empty state.");
            ResetGameToDefault();
            return;
        }

        SaveData data = null;
        try
        {
            if (currentFormat == SaveFormat.JSON)
            {
                string json = File.ReadAllText(savePath);
                data = JsonUtility.FromJson<SaveData>(json);
            }
            else if (currentFormat == SaveFormat.XML)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                using (FileStream stream = new FileStream(savePath, FileMode.Open))
                {
                    data = (SaveData)serializer.Deserialize(stream);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game from {savePath}: {e.Message}");
            ResetGameToDefault();
            return;
        }

        if (data != null)
        {
            ApplySaveData(data);
            CalculateAndShowTimes(data);
        }
    }

    private void ResetGameToDefault()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.transform.position = new Vector3(0, -2, 0);
            player.GetComponent<PlayerInventory>()?.Inventory.slots.ForEach(s => s.Clear());
        }

        Chest[] chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        foreach (Chest c in chests)
        {
            c.Inventory.slots.ForEach(s => s.Clear());
        }
    }

    private void ApplySaveData(SaveData data)
    {
        previousPlayTime = data.totalPlayTimeSeconds;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.transform.position = data.playerPosition;
            var playerInv = player.GetComponent<PlayerInventory>();
            if (playerInv != null && data.playerInventory != null)
            {
                playerInv.Inventory.slots = data.playerInventory.slots;
                playerInv.Inventory.capacity = data.playerInventory.capacity;
            }
        }

        Chest[] chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        foreach (Chest c in chests)
        {
            ChestData savedChest = data.chests.FirstOrDefault(sc => sc.chestId == c.ChestId);
            if (savedChest != null)
            {
                c.Inventory.slots = savedChest.inventory.slots;
                c.Inventory.capacity = savedChest.inventory.capacity;
            }
        }

        // Clean current fruits and respawn
        Fruit[] fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        foreach (Fruit f in fruits) Destroy(f.gameObject);

        FruitFactory factory = FindFirstObjectByType<FruitFactory>();
        FruitSelector selector = FindFirstObjectByType<FruitSelector>();
        
        if (factory != null && selector != null && data.droppedFruits != null)
        {
            foreach (var savedFruit in data.droppedFruits)
            {
                FruitData fData = selector.GetFruitDataById(savedFruit.fruitId);
                if (fData != null)
                {
                    factory.Create(fData, savedFruit.position, Quaternion.identity);
                }
            }
        }
    }

    private void CalculateAndShowTimes(SaveData data)
    {
        TimeSpan totalPlayTime = TimeSpan.FromSeconds(data.totalPlayTimeSeconds);
        Debug.Log($"Tiempo total de juego: {totalPlayTime.Hours} horas y {totalPlayTime.Minutes} minutos");

        if (DateTime.TryParse(data.saveDate, out DateTime lastSaveDate))
        {
            TimeSpan timeSinceLastPlay = DateTime.Now - lastSaveDate;
            Debug.Log($"Hace {timeSinceLastPlay.Days} días, {timeSinceLastPlay.Hours} horas y {timeSinceLastPlay.Minutes} minutos desde tu última sesión de juego.");
        }
    }

    private SaveData GatherSaveData()
    {
        SaveData data = new SaveData();
        
        data.saveDate = DateTime.Now.ToString("O");
        data.totalPlayTimeSeconds = previousPlayTime + currentSessionPlayTime; // We will add the previous playtime when loading

        // Add player data
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            data.playerPosition = player.transform.position;
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                data.playerInventory = inventory.Inventory;
            }
        }

        // Add chests
        Chest[] chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        foreach (Chest c in chests)
        {
            data.chests.Add(new ChestData(c.ChestId, c.Inventory));
        }

        // Add fruits
        Fruit[] fruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        foreach (Fruit f in fruits)
        {
            data.droppedFruits.Add(new FruitWorldData(f.Id, f.transform.position));
        }

        return data;
    }
}