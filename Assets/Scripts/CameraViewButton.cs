using UnityEngine;
using UnityEngine.UI;

public class CameraViewButton : MonoBehaviour
{
    [SerializeField] private CameraViewSwitcher cameraSwitcher;
    [SerializeField] private Button switchButton;
    [SerializeField] private Text buttonText;

    private void Start()
    {
        if (switchButton == null)
        {
            switchButton = GetComponent<Button>();
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<Text>();
        }

        if (cameraSwitcher == null)
        {
            cameraSwitcher = FindObjectOfType<CameraViewSwitcher>();
        }

        switchButton.onClick.AddListener(OnButtonClick);
        UpdateButtonText();
    }

    private void OnButtonClick()
    {
        cameraSwitcher.ToggleCameraView();
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = "Switch to " + (cameraSwitcher.IsTopDownView ? "Angled" : "Top-Down") + " View";
        }
    }
} 