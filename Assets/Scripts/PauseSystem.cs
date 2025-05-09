using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    private UIController uiController;

    public void Initialize(UIController controller)
    {
        uiController = controller;
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void ResumeGame()
    {
        ClosePauseMenu();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        // Load main menu scene - replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }

    public void ClosePauseMenu()
    {
        // Notify the UIController that we're closed
        if (uiController != null)
        {
            uiController.OnPauseMenuClosed();
        }

        // Destroy the pause menu object
        Destroy(gameObject);
    }
}