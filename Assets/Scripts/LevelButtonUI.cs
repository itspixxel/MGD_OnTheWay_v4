using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButtonUI : MonoBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject completionStar;

    [Header("Colors")]
    [SerializeField] private Color unlockedColor = new Color(1f, 1f, 0.8f); // Light yellow
    [SerializeField] private Color lockedColor = new Color(0.7f, 0.7f, 0.7f); // Gray
    
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Ensure lock icon and completion star are hidden by default
        if (lockIcon != null)
        {
            lockIcon.SetActive(false);
        }
        if (completionStar != null)
        {
            completionStar.SetActive(false);
        }
    }

    public void SetupButton(int levelNumber, bool isUnlocked, bool isCompleted)
    {
        // Set level number
        if (levelNumberText != null)
        {
            levelNumberText.text = levelNumber.ToString();
        }

        // Update visuals based on unlock status
        if (backgroundImage != null)
        {
            backgroundImage.color = isUnlocked ? unlockedColor : lockedColor;
        }

        // Show/hide lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // Show/hide completion star
        if (completionStar != null)
        {
            completionStar.SetActive(isUnlocked && isCompleted);
        }

        // Update button interactability
        button.interactable = isUnlocked;
    }

    public void SetHighlighted(bool highlighted)
    {
        // Add visual feedback when the button is highlighted
        if (backgroundImage != null)
        {
            backgroundImage.transform.localScale = highlighted ? 
                new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
        }
    }
} 