using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public GameObject buttonObject;
        public int levelNumber;
        public bool isUnlocked;
        public bool isCompleted;
    }

    [Header("Level Select Settings")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private int numberOfLevels = 12;
    [SerializeField] private int columns = 4;
    [SerializeField] private float spacing = 200f;
    [SerializeField] private string levelScenePrefix = "Level_";

    private LevelButton[] levelButtons;

    private void Start()
    {
        InitializeLevelButtons();
    }

    private void InitializeLevelButtons()
    {
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

            // Create level button data
            LevelButton levelButton = new LevelButton
            {
                buttonObject = buttonObj,
                levelNumber = i + 1,
                isUnlocked = IsLevelUnlocked(i + 1),
                isCompleted = IsLevelCompleted(i + 1)
            };

            // Setup the button UI
            buttonUI.SetupButton(levelButton.levelNumber, levelButton.isUnlocked, levelButton.isCompleted);

            // Add click listener
            Button button = buttonObj.GetComponent<Button>();
            int levelIndex = i + 1; // Capture the level number for the lambda
            button.onClick.AddListener(() => LoadLevel(levelIndex));

            levelButtons[i] = levelButton;
        }
    }

    private bool IsLevelUnlocked(int levelNumber)
    {
        // By default, level 1 is always unlocked
        if (levelNumber == 1) return true;

        // Check if the previous level has been completed
        return PlayerPrefs.GetInt($"Level_{levelNumber - 1}_Completed", 0) == 1;
    }

    private bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Completed", 0) == 1;
    }

    private void LoadLevel(int levelNumber)
    {
        // Load the corresponding level scene
        string sceneName = levelScenePrefix + levelNumber;
        SceneManager.LoadScene(sceneName);
    }

    // Call this method when a level is completed to unlock the next level
    public void UnlockNextLevel(int completedLevelNumber)
    {
        PlayerPrefs.SetInt($"Level_{completedLevelNumber}_Completed", 1);
        PlayerPrefs.Save();
    }
} 