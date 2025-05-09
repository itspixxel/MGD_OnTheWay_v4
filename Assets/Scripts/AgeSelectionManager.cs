using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AgeSelectionManager : MonoBehaviour
{
    public GameObject ageSelectorUIPrefab;
    public GameObject mainMenuCanvas; // Add reference to MainMenuCanvas
    public GameObject loadingIndicator;

    private const string FirstLaunchKey = "FirstLaunch";
    private const string UserAgeKey = "UserAge";
    private const int DefaultAgeValue = 1;

    private async void Awake()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        try
        {
            // Use CloudGameProgress for centralized management
            await CloudGameProgress.Instance.Initialize();
            await CheckFirstLaunch();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Initialization failed: {e.Message}");
            // Fallback to local check if cloud fails
            if (IsFirstLaunch())
            {
                ShowAgeSelector();
            }
        }
        finally
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }
    }

    private async Task CheckFirstLaunch()
    {
        try
        {
            // Try to load from cloud first
            var cloudData = await CloudSaveInitializer.LoadData(new HashSet<string> { FirstLaunchKey });

            if (!cloudData.ContainsKey(FirstLaunchKey))
            {
                Debug.Log("This is the first launch of the game");

                // Save to both PlayerPrefs and Cloud for compatibility
                PlayerPrefs.SetInt(FirstLaunchKey, 0);
                PlayerPrefs.Save();

                await CloudSaveInitializer.SaveData(
                    new Dictionary<string, object> {
                        { FirstLaunchKey, 0 }
                    });

                ShowAgeSelector();
            }
            else
            {
                Debug.Log("This is not the first launch");

                int age = await CloudGameProgress.Instance.LoadUserAge();
                Debug.Log($"Stored user age: {age}");

                // Ensure main menu is visible
                if (mainMenuCanvas != null)
                {
                    mainMenuCanvas.SetActive(true);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Cloud save error: {e.Message}");
            // Fallback to PlayerPrefs
            if (IsFirstLaunch())
            {
                ShowAgeSelector();
            }
            else if (mainMenuCanvas != null)
            {
                mainMenuCanvas.SetActive(true);
            }
        }
    }

    private bool IsFirstLaunch()
    {
        return !PlayerPrefs.HasKey(FirstLaunchKey);
    }

    private void ShowAgeSelector()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        if (ageSelectorUIPrefab != null)
        {
            Instantiate(ageSelectorUIPrefab);
        }
        else
        {
            Debug.LogError("Age Selector UI Prefab is not assigned in the inspector!");
        }
    }

    public async void OnAgeSelected(int age)
    {
        // Save using the centralized manager
        await CloudGameProgress.Instance.SaveUserData(age);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }
}