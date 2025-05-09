using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public static class CloudSaveInitializer
{
    private static bool _isInitialized = false;

    public static async Task Initialize()
    {
        if (_isInitialized) return;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            _isInitialized = true;
            Debug.Log("Unity Services initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            throw;
        }
    }

    public static async Task SaveData(Dictionary<string, object> data)
    {
        try
        {
            await Initialize();
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Data saved to cloud successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data to cloud: {e.Message}");
            throw;
        }
    }

    public static async Task<Dictionary<string, string>> LoadData(HashSet<string> keys = null)
    {
        try
        {
            await Initialize();

            // If no keys specified, load all data
            if (keys == null || keys.Count == 0)
            {
                var allData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
                return allData.ToDictionary(x => x.Key, x => x.Value.ToString());
            }

            // Load specific keys
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
            return data.ToDictionary(x => x.Key, x => x.Value.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load data from cloud: {e.Message}");
            throw;
        }
    }

    public static async Task<Dictionary<string, string>> LoadAllData()
    {
        try
        {
            await Initialize();
            var allData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            return allData.ToDictionary(x => x.Key, x => x.Value.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load all data from cloud: {e.Message}");
            throw;
        }
    }

    public static async Task SaveLevelData(int levelNumber, bool isUnlocked, bool isCompleted, int stars = 0)
    {
        try
        {
            await Initialize();
            var data = new Dictionary<string, object>
        {
            { $"Level_{levelNumber}_Unlocked", isUnlocked ? 1 : 0 },
            { $"Level_{levelNumber}_Completed", isCompleted ? 1 : 0 },
            { $"Level_{levelNumber}_Stars", stars }
        };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"Level {levelNumber} data saved to cloud: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save level data to cloud: {e.Message}");
            throw;
        }
    }

    public static async Task<Dictionary<string, int>> LoadLevelData(int levelNumber)
    {
        try
        {
            await Initialize();
            var keys = new HashSet<string>
        {
            $"Level_{levelNumber}_Unlocked",
            $"Level_{levelNumber}_Completed",
            $"Level_{levelNumber}_Stars"
        };

            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            var result = new Dictionary<string, int>();
            foreach (var key in keys)
            {
                if (data.TryGetValue(key, out var item))
                {
                    result[key] = int.Parse(item.Value.GetAsString());
                }
                else
                {
                    result[key] = 0; // Default value if not found
                }
            }

            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load level data from cloud: {e.Message}");
            throw;
        }
    }
}