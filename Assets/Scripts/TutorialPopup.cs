using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TutorialPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform popupPanel;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Tutorial Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField, TextArea(3, 10)] private string tutorialText;
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private string playerPrefsKey = "TutorialComplete";

    [Header("Events")]
    [SerializeField] private UnityEvent onTutorialComplete;

    private bool isShowing = false;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        // Make entire popup clickable
        if (popupPanel != null)
        {
            // Add a button component if not already present
            Button panelButton = popupPanel.GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = popupPanel.gameObject.AddComponent<Button>();
            }

            // Add click listener
            panelButton.onClick.AddListener(HideTutorial);

            // Calculate positions
            visiblePosition = popupPanel.anchoredPosition;
            hiddenPosition = new Vector2(popupPanel.anchoredPosition.x, -popupPanel.rect.height);
            popupPanel.anchoredPosition = hiddenPosition; // Start hidden
        }
    }

    private void Start()
    {
        // Check if we should show the tutorial on start
        if (showOnStart && !HasCompletedTutorial())
        {
            ShowTutorial();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the tutorial popup
    /// </summary>
    public void ShowTutorial()
    {
        if (isShowing)
            return;

        gameObject.SetActive(true);
        UpdateTutorialContent();
        StartShowAnimation();
    }

    /// <summary>
    /// Hides the tutorial popup and marks it as completed
    /// </summary>
    public void HideTutorial()
    {
        if (!isShowing)
            return;

        StartHideAnimation();
        MarkTutorialAsCompleted();
        onTutorialComplete?.Invoke();
    }

    /// <summary>
    /// Updates the tutorial content
    /// </summary>
    private void UpdateTutorialContent()
    {
        if (instructionText != null)
        {
            instructionText.text = tutorialText;
        }
    }

    private void StartShowAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimatePopup(hiddenPosition, visiblePosition));
    }

    private void StartHideAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimatePopup(visiblePosition, hiddenPosition));
    }

    private IEnumerator AnimatePopup(Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0;
        isShowing = startPos == hiddenPosition;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            // Use an easing function for smoother animation
            t = Mathf.SmoothStep(0, 1, t);

            popupPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        popupPanel.anchoredPosition = endPos;

        if (!isShowing)
            gameObject.SetActive(false);

        animationCoroutine = null;
    }

    private bool HasCompletedTutorial()
    {
        return PlayerPrefs.GetInt(playerPrefsKey, 0) == 1;
    }

    private void MarkTutorialAsCompleted()
    {
        PlayerPrefs.SetInt(playerPrefsKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Resets the tutorial completion status
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        PlayerPrefs.Save();
    }
}