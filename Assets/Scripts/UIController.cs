using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public Action OnRoadPlacement, OnHousePlacement, OnSpecialPlacement, OnBigStructurePlacement;
    public event Action OnClearRoads;
    public Button placeRoadButton, placeHouseButton, placeSpecialButton, placeBigStructureButton;

    [Header("Coin UI")]
    public TextMeshProUGUI coinText;
    public Image[] starImages;
    public Sprite starSprite;
    public Sprite emptyStarSprite;

    public Color outlineColor;
    List<Button> buttonList;

    private void Start()
    {
        buttonList = new List<Button> { placeHouseButton, placeRoadButton, placeSpecialButton, placeBigStructureButton };

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

        // Subscribe to coin collection events
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsCollectedChanged.AddListener(UpdateCoinText);
            CoinManager.Instance.OnStarsChanged.AddListener(UpdateStars);
            UpdateCoinText(CoinManager.Instance.GetCoinsCollected());
            UpdateStars(0);
        }
    }

    private void UpdateCoinText(int coinsCollected)
    {
        if (coinText != null && CoinManager.Instance != null)
        {
            coinText.text = $"{coinsCollected}/{CoinManager.Instance.GetTotalCoins()}";
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
        ResetButtonColor();
        placeRoadButton.GetComponent<Image>().color = Color.green;
        OnRoadPlacement?.Invoke();
    }

    public void OnClearRoadsButtonClicked()
    {
        OnClearRoads?.Invoke();
    }
}
