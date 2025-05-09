using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject completedIcon;
    [SerializeField] private GameObject[] starIcons = new GameObject[3];
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void SetupButton(int levelNumber, bool isUnlocked, bool isCompleted, int stars = 0)
    {
        // Set level number text
        if (levelNumberText != null)
        {
            levelNumberText.text = levelNumber.ToString();
        }

        // Set lock icon visibility
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // Set completed icon visibility
        if (completedIcon != null)
        {
            completedIcon.SetActive(isCompleted);
        }

        // Set button interactable state
        if (button != null)
        {
            button.interactable = isUnlocked;

            // Optional: Change the button color based on unlocked status
            ColorBlock colors = button.colors;
            colors.normalColor = isUnlocked ? unlockedColor : lockedColor;
            button.colors = colors;
        }

        // Set star icons
        if (starIcons != null && starIcons.Length > 0)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    // Only show stars if level is completed
                    starIcons[i].SetActive(isCompleted && i < stars);
                }
            }
        }

        // Get all image components on the button to adjust their color based on lock status
        if (!isUnlocked)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.gameObject != lockIcon)
                {
                    img.color = lockedColor;
                }
            }
        }
    }
}