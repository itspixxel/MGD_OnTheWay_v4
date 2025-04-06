using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton; 
    [SerializeField] private Button shopButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // Add button click listeners
        playButton.onClick.AddListener(OnPlayButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        shopButton.onClick.AddListener(OnShopButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        // Load the game scene - assuming build index 1 is the game scene
        SceneManager.LoadScene(1);
    }

    private void OnSettingsButtonClicked()
    {
        // Load the settings scene - assuming build index 2 is the settings scene
        SceneManager.LoadScene(2);
    }

    private void OnShopButtonClicked()
    {
        // Load the shop scene - assuming build index 3 is the shop scene
        SceneManager.LoadScene(3);
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
