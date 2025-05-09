using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject levelUI;
    [SerializeField] private GameObject levelCompleteScreen;

    [Header("References")]
    [SerializeField] private CarAI carAI;

    private void Awake()
    {
        // Ensure level UI is shown and level complete screen is hidden at start
        if (levelUI != null)
        {
            levelUI.SetActive(true);
        }

        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(false);
        }
    }

    private void Start()
    {
        // Connect to car AI event if available
        if (carAI != null)
        {
            carAI.OnCarReachedDestination.AddListener(ShowLevelCompleteScreen);
        }
        else
        {
            Debug.LogWarning("CarAI reference not set in LevelUIManager. Please assign it in the inspector.");
        }
    }

    /// <summary>
    /// Show the level complete screen and hide the level UI
    /// </summary>
    public void ShowLevelCompleteScreen()
    {
        Debug.Log("Showing level complete screen");

        // Hide level UI
        if (levelUI != null)
        {
            levelUI.SetActive(false);
        }

        // Show level complete screen
        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(true);
        }
        else
        {
            Debug.LogError("Level complete screen reference is missing!");
        }
    }

    /// <summary>
    /// Find and automatically connect to the car AI in the scene
    /// </summary>
    public void FindAndConnectToCarAI()
    {
        if (carAI == null)
        {
            carAI = FindObjectOfType<CarAI>();

            if (carAI != null)
            {
                carAI.OnCarReachedDestination.AddListener(ShowLevelCompleteScreen);
                Debug.Log("Successfully connected to CarAI automatically");
            }
            else
            {
                Debug.LogWarning("Could not find CarAI in the scene");
            }
        }
    }

    /// <summary>
    /// Manually connect to a specific car AI
    /// </summary>
    public void ConnectToCarAI(CarAI carAIInstance)
    {
        if (carAIInstance != null)
        {
            // Remove previous listener if it exists
            if (carAI != null)
            {
                carAI.OnCarReachedDestination.RemoveListener(ShowLevelCompleteScreen);
            }

            // Set new car AI and add listener
            carAI = carAIInstance;
            carAI.OnCarReachedDestination.AddListener(ShowLevelCompleteScreen);
        }
    }

    /// <summary>
    /// Reset the UI to initial state
    /// </summary>
    public void ResetUI()
    {
        if (levelUI != null)
        {
            levelUI.SetActive(true);
        }

        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(false);
        }
    }
}