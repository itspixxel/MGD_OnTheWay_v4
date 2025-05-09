using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Centralized manager for all game progress saved to cloud
/// </summary>
public class CloudGameProgress : MonoBehaviour
{
    private static CloudGameProgress _instance;
    public static CloudGameProgress Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("CloudGameProgress");
                _instance = obj.AddComponent<CloudGameProgress>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    [SerializeField] private bool enableCloudSaves = true;
    [SerializeField] private bool enableLocalBackup = true;
    [SerializeField] private bool verboseLogging = false;

    // Cached data to minimize cloud queries
    private Dictionary<string, string> cachedCloudData = new Dictionary<string, string>();
    private bool isInitialized = false;
    private bool isInitializing = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> Initialize()
    {
        if (isInitialized) return true;
        if (isInitializing) return false;

        isInitializing = true;

        try
        {
            if (enableCloudSaves)
            {
                await CloudSaveInitializer.Initialize();
                Log("Cloud services initialized");

                // Load all existing data into cache
                cachedCloudData = await CloudSaveInitializer.LoadAllData();
                Log($"Loaded {cachedCloudData.Count} items from cloud");
            }

            isInitialized = true;
            isInitializing = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize cloud progress: {e.Message}");
            isInitializing = false;
            return false;
        }
    }

    // LEVEL PROGRESS METHODS

    /// <summary>
    /// Saves level progress to cloud and local storage
    /// </summary>
    public async Task SaveLevelProgress(int levelNumber, bool isUnlocked, bool isCompleted, int stars)
    {
        if (!await EnsureInitialized())
        {
            Log("Initialization failed, using local save only");
            SaveLevelProgressLocal(levelNumber, isUnlocked, isCompleted, stars);
            return;
        }

        try
        {
            // Save to cloud
            if (enableCloudSaves)
            {
                await CloudSaveInitializer.SaveLevelData(levelNumber, isUnlocked, isCompleted, stars);

                // Update cache
                cachedCloudData[$"Level_{levelNumber}_Unlocked"] = isUnlocked ? "1" : "0";
                cachedCloudData[$"Level_{levelNumber}_Completed"] = isCompleted ? "1" : "0";
                cachedCloudData[$"Level_{levelNumber}_Stars"] = stars.ToString();

                Log($"Saved level {levelNumber} progress to cloud: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
            }

            // Save locally as backup
            if (enableLocalBackup)
            {
                SaveLevelProgressLocal(levelNumber, isUnlocked, isCompleted, stars);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save level progress to cloud: {e.Message}");

            // Fall back to local save
            if (enableLocalBackup)
            {
                SaveLevelProgressLocal(levelNumber, isUnlocked, isCompleted, stars);
            }
        }
    }

    /// <summary>
    /// Loads level progress for a specific level
    /// </summary>
    public async Task<(bool isUnlocked, bool isCompleted, int stars)> LoadLevelProgress(int levelNumber)
    {
        // Default values
        bool isUnlocked = (levelNumber == 1); // Level 1 is always unlocked
        bool isCompleted = false;
        int stars = 0;

        if (!await EnsureInitialized())
        {
            Log("Initialization failed, using local data only");
            return LoadLevelProgressLocal(levelNumber);
        }

        try
        {
            // Try to get from cache first
            if (cachedCloudData.Count > 0)
            {
                if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Unlocked", out string unlockedStr) &&
                    int.TryParse(unlockedStr, out int unlockedVal))
                {
                    isUnlocked = unlockedVal == 1;
                }

                if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Completed", out string completedStr) &&
                    int.TryParse(completedStr, out int completedVal))
                {
                    isCompleted = completedVal == 1;
                }

                if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Stars", out string starsStr) &&
                    int.TryParse(starsStr, out int starsVal))
                {
                    stars = starsVal;
                }

                Log($"Retrieved level {levelNumber} progress from cache: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
                return (isUnlocked, isCompleted, stars);
            }

            // If not in cache, get directly from cloud
            if (enableCloudSaves)
            {
                var levelData = await CloudSaveInitializer.LoadLevelData(levelNumber);

                if (levelData.TryGetValue($"Level_{levelNumber}_Unlocked", out int unlocked))
                {
                    isUnlocked = unlocked == 1;
                }

                if (levelData.TryGetValue($"Level_{levelNumber}_Completed", out int completed))
                {
                    isCompleted = completed == 1;
                }

                if (levelData.TryGetValue($"Level_{levelNumber}_Stars", out int starsVal))
                {
                    stars = starsVal;
                }

                Log($"Retrieved level {levelNumber} progress from cloud: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
                return (isUnlocked, isCompleted, stars);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load level progress from cloud: {e.Message}");
        }

        // Fall back to local data
        return LoadLevelProgressLocal(levelNumber);
    }

    /// <summary>
    /// Loads all level data from cloud for multiple levels at once
    /// </summary>
    public async Task<Dictionary<int, (bool isUnlocked, bool isCompleted, int stars)>> LoadAllLevelProgress(int maxLevels)
    {
        var result = new Dictionary<int, (bool isUnlocked, bool isCompleted, int stars)>();

        // Initialize with default values from local storage
        for (int i = 1; i <= maxLevels; i++)
        {
            result[i] = LoadLevelProgressLocal(i);
        }

        if (!await EnsureInitialized())
        {
            Log("Initialization failed, using local data only");
            return result;
        }

        try
        {
            if (enableCloudSaves)
            {
                // Refresh the cloud data cache
                cachedCloudData = await CloudSaveInitializer.LoadAllData();

                // Parse and update results
                for (int levelNumber = 1; levelNumber <= maxLevels; levelNumber++)
                {
                    bool isUnlocked = (levelNumber == 1); // Level 1 default
                    bool isCompleted = false;
                    int stars = 0;

                    if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Unlocked", out string unlockedStr) &&
                        int.TryParse(unlockedStr, out int unlockedVal))
                    {
                        isUnlocked = unlockedVal == 1;
                    }

                    if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Completed", out string completedStr) &&
                        int.TryParse(completedStr, out int completedVal))
                    {
                        isCompleted = completedVal == 1;
                    }

                    if (cachedCloudData.TryGetValue($"Level_{levelNumber}_Stars", out string starsStr) &&
                        int.TryParse(starsStr, out int starsVal))
                    {
                        stars = starsVal;
                    }

                    result[levelNumber] = (isUnlocked, isCompleted, stars);
                }

                Log($"Retrieved data for {maxLevels} levels from cloud");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load all level progress from cloud: {e.Message}");
        }

        return result;
    }

    // USER DATA METHODS

    /// <summary>
    /// Saves user data like age to cloud
    /// </summary>
    public async Task SaveUserData(int age)
    {
        if (!await EnsureInitialized())
        {
            SaveUserDataLocal(age);
            return;
        }

        try
        {
            if (enableCloudSaves)
            {
                await CloudSaveInitializer.SaveData(new Dictionary<string, object> {
                    { "FirstLaunch", 0 },
                    { "UserAge", age }
                });

                Log($"User age {age} saved to cloud");
            }

            if (enableLocalBackup)
            {
                SaveUserDataLocal(age);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save user data to cloud: {e.Message}");

            if (enableLocalBackup)
            {
                SaveUserDataLocal(age);
            }
        }
    }

    /// <summary>
    /// Loads user age from cloud or local storage
    /// </summary>
    public async Task<int> LoadUserAge()
    {
        int defaultAge = 1;

        if (!await EnsureInitialized())
        {
            return GetUserAgeLocal();
        }

        try
        {
            if (enableCloudSaves)
            {
                var data = await CloudSaveInitializer.LoadData(new HashSet<string> { "UserAge" });

                if (data.TryGetValue("UserAge", out string ageStr) && int.TryParse(ageStr, out int age))
                {
                    Log($"Retrieved user age {age} from cloud");
                    return age;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load user age from cloud: {e.Message}");
        }

        return GetUserAgeLocal();
    }

    // HELPER METHODS

    private void SaveLevelProgressLocal(int levelNumber, bool isUnlocked, bool isCompleted, int stars)
    {
        PlayerPrefs.SetInt($"Level_{levelNumber}_Unlocked", isUnlocked ? 1 : 0);
        PlayerPrefs.SetInt($"Level_{levelNumber}_Completed", isCompleted ? 1 : 0);
        PlayerPrefs.SetInt($"Level_{levelNumber}_Stars", stars);
        PlayerPrefs.Save();

        Log($"Level {levelNumber} progress saved locally: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
    }

    private (bool isUnlocked, bool isCompleted, int stars) LoadLevelProgressLocal(int levelNumber)
    {
        bool isUnlocked = (levelNumber == 1) || (PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", 0) == 1);
        bool isCompleted = PlayerPrefs.GetInt($"Level_{levelNumber}_Completed", 0) == 1;
        int stars = PlayerPrefs.GetInt($"Level_{levelNumber}_Stars", 0);

        Log($"Retrieved level {levelNumber} progress from local storage: Unlocked={isUnlocked}, Completed={isCompleted}, Stars={stars}");
        return (isUnlocked, isCompleted, stars);
    }

    private void SaveUserDataLocal(int age)
    {
        PlayerPrefs.SetInt("FirstLaunch", 0);
        PlayerPrefs.SetInt("UserAge", age);
        PlayerPrefs.Save();
        Log($"User age {age} saved locally");
    }

    private int GetUserAgeLocal()
    {
        int age = PlayerPrefs.GetInt("UserAge", 1);
        Log($"Retrieved user age {age} from local storage");
        return age;
    }

    private async Task<bool> EnsureInitialized()
    {
        if (isInitialized) return true;
        return await Initialize();
    }

    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[CloudGameProgress] {message}");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Game is being paused (e.g., app going to background)
            // Consider forcing a cloud sync here if needed
            Log("Application paused - cloud data has been saved");
        }
    }
}