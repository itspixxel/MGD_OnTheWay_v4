using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LevelSelect : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public GameObject buttonObject;
        public int levelNumber;
        public bool isUnlocked;
        public bool isCompleted;
        public int stars;
    }

    [Header("Level Select Settings")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private int numberOfLevels = 12;
    [SerializeField] private int columns = 4;
    [SerializeField] private float spacing = 200f;
    [SerializeField] private string levelScenePrefix = "Level_";

    // Loading state management
    [SerializeField] private GameObject loadingIndicator;

    private LevelButton[] levelButtons;
    private bool isInitialized = false;

    private async void Start()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        try
        {
            await CloudSaveInitializer.Initialize();
            await InitializeLevelButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize with cloud data: {e.Message}");
            // Fallback to local data
            InitializeLevelButtonsWithLocalData();
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }
    }

    private async Task InitializeLevelButtons()
    {
        levelButtons = new LevelButton[numberOfLevels];

        // First, load all level data from cloud
        Dictionary<string, string> cloudData;
        try
        {
            cloudData = await CloudSaveInitializer.LoadAllData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load cloud data: {e.Message}");
            // Fallback to local data
            InitializeLevelButtonsWithLocalData();
            return;
        }

        for (int i = 0; i < numberOfLevels; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 position = new Vector3(
                col * spacing - (spacing * (columns - 1) / 2f),
                -row * spacing,
                0
            );

            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            buttonObj.GetComponent<RectTransform>().anchoredPosition = position;

            // Get the LevelButtonUI component
            LevelButtonUI buttonUI = buttonObj.GetComponent<LevelButtonUI>();
            if (buttonUI == null)
            {
                Debug.LogError("LevelButtonUI component not found on button prefab!");
                continue;
            }

            int levelNumber = i + 1;
            bool isUnlocked = IsLevelUnlocked(levelNumber, cloudData);
            bool isCompleted = IsLevelCompleted(levelNumber, cloudData);
            int stars = GetLevelStars(levelNumber, cloudData);

            // Create level button data
            LevelButton levelButton = new LevelButton
            {
                buttonObject = buttonObj,
                levelNumber = levelNumber,
                isUnlocked = isUnlocked,
                isCompleted = isCompleted,
                stars = stars
            };

            // Setup the button UI
            buttonUI.SetupButton(levelButton.levelNumber, levelButton.isUnlocked, levelButton.isCompleted, levelButton.stars);

            // Add click listener
            Button button = buttonObj.GetComponent<Button>();
            int levelIndex = levelNumber; // Capture the level number for the lambda
            button.onClick.AddListener(() => LoadLevel(levelIndex));

            levelButtons[i] = levelButton;
        }

        isInitialized = true;
    }

    private void InitializeLevelButtonsWithLocalData()
    {
        // Fallback initialization using PlayerPrefs
        levelButtons = new LevelButton[numberOfLevels];

        for (int i = 0; i < numberOfLevels; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 position = new Vector3(
                col * spacing - (spacing * (columns - 1) / 2f),
                -row * spacing,
                0
            );

            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            buttonObj.GetComponent<RectTransform>().anchoredPosition = position;

            // Get the LevelButtonUI component
            LevelButtonUI buttonUI = buttonObj.GetComponent<LevelButtonUI>();
            if (buttonUI == null)
            {
                Debug.LogError("LevelButtonUI component not found on button prefab!");
                continue;
            }

            int levelNumber = i + 1;
            bool isUnlocked = IsLevelUnlockedLocal(levelNumber);
            bool isCompleted = IsLevelCompletedLocal(levelNumber);
            int stars = GetLevelStarsLocal(levelNumber);

            // Create level button data
            LevelButton levelButton = new LevelButton
            {
                buttonObject = buttonObj,
                levelNumber = levelNumber,
                isUnlocked = isUnlocked,
                isCompleted = isCompleted,
                stars = stars
            };

            // Setup the button UI
            buttonUI.SetupButton(levelButton.levelNumber, levelButton.isUnlocked, levelButton.isCompleted, levelButton.stars);

            // Add click listener
            Button button = buttonObj.GetComponent<Button>();
            int levelIndex = levelNumber; // Capture the level number for the lambda
            button.onClick.AddListener(() => LoadLevel(levelIndex));

            levelButtons[i] = levelButton;
        }

        isInitialized = true;
    }

    private bool IsLevelUnlocked(int levelNumber, Dictionary<string, string> cloudData)
    {
        // By default, level 1 is always unlocked
        if (levelNumber == 1) return true;

        string unlockedKey = $"Level_{levelNumber}_Unlocked";

        // Check if this level is explicitly unlocked in cloud data
        if (cloudData.TryGetValue(unlockedKey, out string unlockedValue) &&
            int.TryParse(unlockedValue, out int unlocked) &&
            unlocked == 1)
            return true;

        // Check if the previous level has been completed
        string prevCompletedKey = $"Level_{levelNumber - 1}_Completed";
        if (cloudData.TryGetValue(prevCompletedKey, out string completedValue) &&
            int.TryParse(completedValue, out int completed) &&
            completed == 1)
            return true;

        // Fall back to PlayerPrefs if not in cloud data
        return IsLevelUnlockedLocal(levelNumber);
    }

    private bool IsLevelUnlockedLocal(int levelNumber)
    {
        // By default, level 1 is always unlocked
        if (levelNumber == 1) return true;

        // Check if this level is explicitly unlocked
        if (PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", 0) == 1)
            return true;

        // Check if the previous level has been completed
        if (PlayerPrefs.GetInt($"Level_{levelNumber - 1}_Completed", 0) == 1)
            return true;

        return false;
    }

    private bool IsLevelCompleted(int levelNumber, Dictionary<string, string> cloudData)
    {
        string completedKey = $"Level_{levelNumber}_Completed";

        // Check if level is completed in cloud data
        if (cloudData.TryGetValue(completedKey, out string completedValue) &&
            int.TryParse(completedValue, out int completed) &&
            completed == 1)
            return true;

        // Fall back to PlayerPrefs if not in cloud data
        return IsLevelCompletedLocal(levelNumber);
    }

    private bool IsLevelCompletedLocal(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Completed", 0) == 1;
    }

    private int GetLevelStars(int levelNumber, Dictionary<string, string> cloudData)
    {
        string starsKey = $"Level_{levelNumber}_Stars";

        // Check if stars are in cloud data
        if (cloudData.TryGetValue(starsKey, out string starsValue) &&
            int.TryParse(starsValue, out int stars))
            return stars;

        // Fall back to PlayerPrefs if not in cloud data
        return GetLevelStarsLocal(levelNumber);
    }

    private int GetLevelStarsLocal(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Stars", 0);
    }

    private void LoadLevel(int levelNumber)
    {
        // Load the corresponding level scene
        string sceneName = levelScenePrefix + levelNumber;
        SceneManager.LoadScene(sceneName);
    }

    // Call this method when a level is completed to unlock the next level
    public async void UnlockNextLevel(int completedLevelNumber)
    {
        try
        {
            // Mark current level as completed and unlock the next level in cloud
            await CloudSaveInitializer.SaveLevelData(completedLevelNumber, true, true);
            await CloudSaveInitializer.SaveLevelData(completedLevelNumber + 1, true, false);

            Debug.Log($"Level {completedLevelNumber} marked as completed and Level {completedLevelNumber + 1} unlocked in cloud");

            // Also update PlayerPrefs as backup
            PlayerPrefs.SetInt($"Level_{completedLevelNumber}_Completed", 1);
            PlayerPrefs.SetInt($"Level_{completedLevelNumber + 1}_Unlocked", 1);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save level data to cloud: {e.Message}");

            // Fallback to PlayerPrefs only
            PlayerPrefs.SetInt($"Level_{completedLevelNumber}_Completed", 1);
            PlayerPrefs.SetInt($"Level_{completedLevelNumber + 1}_Unlocked", 1);
            PlayerPrefs.Save();

            Debug.Log($"Fallback: Level {completedLevelNumber} marked as completed and Level {completedLevelNumber + 1} unlocked in PlayerPrefs");
        }
    }

    // New method to mark a level as completed with stars and unlock the next level
    public async void MarkLevelAsCompleted(int levelNumber, int stars = 0)
    {
        if (levelNumber <= 0 || levelNumber > numberOfLevels) return;

        try
        {
            // Save to cloud
            await CloudSaveInitializer.SaveLevelData(levelNumber, true, true, stars);

            // Unlock the next level if it exists
            if (levelNumber < numberOfLevels)
            {
                await CloudSaveInitializer.SaveLevelData(levelNumber + 1, true, false);
            }

            Debug.Log($"Cloud: Level {levelNumber} marked as completed with {stars} stars");

            // Also update PlayerPrefs as backup
            PlayerPrefs.SetInt($"Level_{levelNumber}_Completed", 1);
            PlayerPrefs.SetInt($"Level_{levelNumber}_Stars", stars);

            if (levelNumber < numberOfLevels)
            {
                PlayerPrefs.SetInt($"Level_{levelNumber + 1}_Unlocked", 1);
            }

            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save level completion to cloud: {e.Message}");

            // Fallback to PlayerPrefs only
            PlayerPrefs.SetInt($"Level_{levelNumber}_Completed", 1);
            PlayerPrefs.SetInt($"Level_{levelNumber}_Stars", stars);

            if (levelNumber < numberOfLevels)
            {
                PlayerPrefs.SetInt($"Level_{levelNumber + 1}_Unlocked", 1);
            }

            PlayerPrefs.Save();
        }

        // If we're in the level select scene and UI is initialized, update the UI
        if (SceneManager.GetActiveScene().name == "LevelSelect" && isInitialized)
        {
            // Update current level button
            if (levelNumber <= levelButtons.Length)
            {
                LevelButton button = levelButtons[levelNumber - 1];
                button.isCompleted = true;
                button.stars = stars;

                // Update the UI
                LevelButtonUI buttonUI = button.buttonObject.GetComponent<LevelButtonUI>();
                if (buttonUI != null)
                {
                    buttonUI.SetupButton(levelNumber, true, true, stars);
                }

                // Update the next button if it exists
                if (levelNumber < levelButtons.Length)
                {
                    LevelButton nextButton = levelButtons[levelNumber];
                    nextButton.isUnlocked = true;

                    // Update button UI
                    LevelButtonUI nextButtonUI = nextButton.buttonObject.GetComponent<LevelButtonUI>();
                    if (nextButtonUI != null)
                    {
                        nextButtonUI.SetupButton(levelNumber + 1, true, nextButton.isCompleted, nextButton.stars);
                    }
                }
            }
        }
    }

    public void ReturnToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    // Method to refresh level buttons with latest cloud data
    public async void RefreshLevelData()
    {
        if (!isInitialized || levelButtons == null) return;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        try
        {
            Dictionary<string, string> cloudData = await CloudSaveInitializer.LoadAllData();

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelNumber = i + 1;
                LevelButton button = levelButtons[i];

                button.isUnlocked = IsLevelUnlocked(levelNumber, cloudData);
                button.isCompleted = IsLevelCompleted(levelNumber, cloudData);
                button.stars = GetLevelStars(levelNumber, cloudData);

                // Update button UI
                LevelButtonUI buttonUI = button.buttonObject.GetComponent<LevelButtonUI>();
                if (buttonUI != null)
                {
                    buttonUI.SetupButton(levelNumber, button.isUnlocked, button.isCompleted, button.stars);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to refresh level data from cloud: {e.Message}");
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }
    }
}