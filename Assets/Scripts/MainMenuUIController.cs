using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Menu UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button[] backButtons;

    private GameObject currentActivePanel;

    private void Awake()
    {
        // Set initial state
        currentActivePanel = mainMenuPanel;
        
        // Make sure main menu is active on start
        mainMenuPanel.SetActive(true);
        playPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
    }

    private void Start()
    {
        // Add button listeners
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        // Main menu button clicks
        playButton.onClick.AddListener(() => SwitchPanel(playPanel));
        levelSelectButton.onClick.AddListener(() => SwitchPanel(levelSelectPanel));

        // Setup back buttons
        foreach (Button backButton in backButtons)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
    }

    /// <summary>
    /// Switches from the current panel to the target panel
    /// </summary>
    /// <param name="targetPanel">The panel to switch to</param>
    public void SwitchPanel(GameObject targetPanel)
    {
        // Disable current panel
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }

        // Enable target panel
        targetPanel.SetActive(true);
        currentActivePanel = targetPanel;
        
        // Optional animation could be added here
    }

    /// <summary>
    /// Returns to the main menu panel from any other panel
    /// </summary>
    public void BackToMainMenu()
    {
        SwitchPanel(mainMenuPanel);
    }

    /// <summary>
    /// Method to handle Play button functionality beyond UI switching
    /// </summary>
    public void StartGame()
    {
        // Add game start logic here
        Debug.Log("Starting game...");
        // Example: SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Public method that can be called from any UI element to quit the game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
