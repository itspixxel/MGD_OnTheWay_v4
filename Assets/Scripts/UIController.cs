using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OnTheWay.AI;

public class UIController : MonoBehaviour
{
    public Action OnRoadPlacement, OnHousePlacement, OnSpecialPlacement, OnBigStructurePlacement;
    public event Action OnClearRoads;
    public Button placeRoadButton, placeHouseButton, placeSpecialButton, placeBigStructureButton;
    public Button switchCameraViewButton;
    public Button clearRoadsButton;
    public Button pauseButton; // New pause button reference

    [Header("Pause Menu")]
    public GameObject pauseMenuPrefab; // Reference to pause menu prefab
    private GameObject pauseMenuInstance; // Instance of the spawned pause menu

    [Header("Coin UI")]
    public Image[] starImages;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    public Color outlineColor;
    List<Button> buttonList;

    private CameraViewSwitcher cameraSwitcher;
    private AiDirector aiDirector;
    private PauseSystem pauseSystem; // Reference to the pause system component
    private bool isPaused = false;

    [SerializeField] private GameObject levelUI;

    private void Start()
    {
        buttonList = new List<Button> { placeHouseButton, placeRoadButton, placeSpecialButton, placeBigStructureButton };
        aiDirector = FindObjectOfType<AiDirector>();

        placeRoadButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeRoadButton);
            OnRoadPlacement?.Invoke();
        });
        placeHouseButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeHouseButton);
            OnHousePlacement?.Invoke();
        });
        placeSpecialButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeSpecialButton);
            OnSpecialPlacement?.Invoke();
        });
        placeBigStructureButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeBigStructureButton);
            OnBigStructurePlacement?.Invoke();
        });

        // Setup pause button listener
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePauseMenu);
        }

        // Initialize camera switcher
        cameraSwitcher = FindObjectOfType<CameraViewSwitcher>();
        if (cameraSwitcher == null)
        {
            GameObject cameraSwitcherObj = new GameObject("CameraViewSwitcher");
            cameraSwitcher = cameraSwitcherObj.AddComponent<CameraViewSwitcher>();
        }

        // Setup camera view switching button
        if (switchCameraViewButton != null)
        {
            switchCameraViewButton.onClick.AddListener(() =>
            {
                cameraSwitcher.ToggleCameraView();
                UpdateCameraButtonText();
            });
            UpdateCameraButtonText();
        }

        // Subscribe to coin collection events
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnStarsChanged.AddListener(UpdateStars);
            UpdateStars(0);
        }
    }

    private void Update()
    {
        if (aiDirector != null)
        {
            bool isCarActive = aiDirector.IsCarActive;
            foreach (Button button in buttonList)
            {
                if (button != null)
                {
                    button.interactable = !isCarActive;
                }
            }
            if (clearRoadsButton != null)
            {
                clearRoadsButton.interactable = !isCarActive;
            }
        }

        // Alternative way to toggle pause - ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void UpdateCameraButtonText()
    {
        if (switchCameraViewButton != null)
        {
            TextMeshProUGUI buttonText = switchCameraViewButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Switch to " + (cameraSwitcher.IsTopDownView ? "Angled" : "Top-Down") + " View";
            }
        }
    }

    private void UpdateStars(int stars)
    {
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].sprite = i < stars ? starSprite : emptyStarSprite;
            }
        }
    }

    private void ModifyOutline(Button button)
    {
        var outline = button.GetComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.enabled = true;
    }

    public void ResetButtonColor()
    {
        foreach (Button button in buttonList)
        {
            button.GetComponent<Outline>().enabled = false;
        }
    }

    public void OnRoadButtonClicked()
    {
        OnRoadPlacement?.Invoke();
    }

    public void OnClearRoadsButtonClicked()
    {
        OnClearRoads?.Invoke();
    }

    // Method to toggle pause menu
    public void TogglePauseMenu()
    {
        if (!isPaused)
        {
            // Open pause menu
            if (pauseMenuPrefab != null)
            {
                pauseMenuInstance = Instantiate(pauseMenuPrefab, transform.root);
                levelUI.SetActive(false); // Hide level UI when pause menu is open

                // Get the PauseSystem component from the instantiated prefab
                pauseSystem = pauseMenuInstance.GetComponent<PauseSystem>();
                if (pauseSystem != null)
                {
                    pauseSystem.Initialize(this);
                }
                else
                {
                    Debug.LogError("PauseSystem component not found on pause menu prefab!");
                }

                // Pause the game
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                Debug.LogError("Pause menu prefab is not assigned!");
            }
        }
        else
        {
            // Close existing pause menu
            if (pauseSystem != null)
            {
                pauseSystem.ClosePauseMenu();
            }
            else if (pauseMenuInstance != null)
            {
                Destroy(pauseMenuInstance);
                // Resume the game
                Time.timeScale = 1f;
                isPaused = false;
                pauseMenuInstance = null;
                pauseSystem = null;
            }
        }
    }

    // Called by PauseSystem when it self-destroys
    public void OnPauseMenuClosed()
    {
        pauseMenuInstance = null;
        pauseSystem = null;
        // Resume the game
        Time.timeScale = 1f;
        isPaused = false;
        levelUI.SetActive(true); // Show level UI when pause menu is closed
    }
}