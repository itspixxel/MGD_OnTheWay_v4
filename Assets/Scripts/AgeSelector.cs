using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class AgeSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField ageInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Settings")]
    [SerializeField] private int minAge = 13;
    [SerializeField] private int maxAge = 120;

    private async void Awake()
    {
        confirmButton.interactable = false;
        ageInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        ageInputField.onValueChanged.AddListener(ValidateAgeInput);
        confirmButton.onClick.AddListener(OnConfirmClicked);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        try
        {
            // Initialize cloud services
            await CloudGameProgress.Instance.Initialize();
        }
        catch
        {
            Debug.LogWarning("Cloud initialization failed, using local storage fallback");
        }
    }

    private void ValidateAgeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            confirmButton.interactable = false;
            return;
        }

        if (!int.TryParse(input, out int age))
        {
            confirmButton.interactable = false;
            return;
        }

        if (age < minAge || age > maxAge)
        {
            confirmButton.interactable = false;
            return;
        }

        confirmButton.interactable = true;
    }

    private async void OnConfirmClicked()
    {
        if (!int.TryParse(ageInputField.text, out int age))
            return;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        confirmButton.interactable = false;

        try
        {
            // Use CloudGameProgress for centralized management
            await CloudGameProgress.Instance.SaveUserData(age);
            Debug.Log($"Age {age} saved to cloud");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save age: {e.Message}");

            // Fallback to PlayerPrefs
            PlayerPrefs.SetInt("FirstLaunch", 0);
            PlayerPrefs.SetInt("UserAge", age);
            PlayerPrefs.Save();
            Debug.Log($"Age {age} saved locally");
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            // Notify AgeSelectionManager that age has been selected
            AgeSelectionManager ageManager = FindFirstObjectByType<AgeSelectionManager>();
            if (ageManager != null)
            {
                ageManager.OnAgeSelected(age);
            }

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        ageInputField.onValueChanged.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
    }
}