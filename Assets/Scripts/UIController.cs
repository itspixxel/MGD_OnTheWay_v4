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

    [Header("Coin UI")]
    public Image[] starImages;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    public Color outlineColor;
    List<Button> buttonList;

    private CameraViewSwitcher cameraSwitcher;
    private AiDirector aiDirector;

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
}
