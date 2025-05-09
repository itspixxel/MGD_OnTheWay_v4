using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LevelCompleteStarCounter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] coinStarImages = new Image[3];
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Settings")]
    [SerializeField] private bool animateStars = true;
    [SerializeField] private float delayBetweenStars = 0.3f;
    [SerializeField] private bool useCloudSave = true;

    private CoinManager coinManager;
    private LevelSelect levelSelect;
    private int currentLevel;
    private int coinsCollected;
    private bool isSaving = false;

    private void Awake()
    {
        // Initialize all stars to empty
        for (int i = 0; i < coinStarImages.Length; i++)
        {
            if (coinStarImages[i] != null)
            {
                coinStarImages[i].sprite = emptyStarSprite;
            }
        }

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    private async void OnEnable()
    {
        // Find the coin manager and update the UI when the panel becomes active
        FindCoinManager();
        UpdateStarUI();

        // Get current level number from scene name
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level_") && int.TryParse(sceneName.Substring(6), out currentLevel))
        {
            // Find LevelSelect to mark level as completed
            levelSelect = FindFirstObjectByType<LevelSelect>();

            // Initialize cloud connectivity
            if (useCloudSave)
            {
                try
                {
                    await CloudSaveInitializer.Initialize();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Cloud initialization failed: {e.Message}");
                    useCloudSave = false; // Fall back to local storage
                }
            }
        }
    }

    private void FindCoinManager()
    {
        coinManager = FindFirstObjectByType<CoinManager>();
        if (coinManager == null)
        {
            Debug.LogWarning("CoinManager not found in the scene!");
        }
    }

    public void UpdateStarUI()
    {
        if (coinManager == null)
        {
            FindCoinManager();
            if (coinManager == null) return;
        }

        coinsCollected = coinManager.GetCoinsCollected();
        int totalCoins = coinManager.GetTotalCoins();

        // Update coin count text if it exists
        if (coinCountText != null)
        {
            coinCountText.text = $"{coinsCollected}/{totalCoins}";
        }

        if (animateStars)
        {
            StartCoroutine(AnimateStars(coinsCollected));
        }
        else
        {
            // Update stars immediately
            for (int i = 0; i < coinStarImages.Length; i++)
            {
                if (coinStarImages[i] != null)
                {
                    coinStarImages[i].sprite = (i < coinsCollected) ? filledStarSprite : emptyStarSprite;
                }
            }
        }

        // Save star count for this level
        if (currentLevel > 0)
        {
            SaveStarCount(currentLevel, coinsCollected);
        }
    }

    private async void SaveStarCount(int level, int stars)
    {
        // Check if we have more stars than before
        int previousStars = PlayerPrefs.GetInt($"Level_{level}_Stars", 0);
        if (stars <= previousStars) return;

        // Save to PlayerPrefs
        PlayerPrefs.SetInt($"Level_{level}_Stars", stars);
        PlayerPrefs.Save();

        // Save to cloud
        if (useCloudSave)
        {
            try
            {
                await CloudSaveInitializer.SaveLevelData(level, true, false, stars);
                Debug.Log($"Saved {stars} stars for level {level} to cloud");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save star count to cloud: {e.Message}");
            }
        }
    }

    private System.Collections.IEnumerator AnimateStars(int coinsCollected)
    {
        // First make sure all stars are empty
        for (int i = 0; i < coinStarImages.Length; i++)
        {
            if (coinStarImages[i] != null)
            {
                coinStarImages[i].sprite = emptyStarSprite;
            }
        }

        // Then fill them one by one with a delay
        for (int i = 0; i < coinsCollected && i < coinStarImages.Length; i++)
        {
            yield return new WaitForSeconds(delayBetweenStars);

            if (coinStarImages[i] != null)
            {
                coinStarImages[i].sprite = filledStarSprite;

                // Optional - play a sound effect here
                // AudioManager.Instance.PlaySound("star_unlock");
            }
        }
    }

    // You can call this method from a button to continue to the next level
    public async void ContinueToNextLevel()
    {
        if (isSaving) return;
        isSaving = true;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Mark level as complete if player has at least 1 star
        if (coinsCollected >= 1 && currentLevel > 0)
        {
            try
            {
                if (levelSelect != null)
                {
                    // Cloud save is handled inside the MarkLevelAsCompleted method
                    levelSelect.MarkLevelAsCompleted(currentLevel, coinsCollected);
                    Debug.Log($"Level {currentLevel} completed with {coinsCollected} stars. Marked as completed.");
                }
                else if (useCloudSave)
                {
                    // Direct cloud save if LevelSelect not found
                    await CloudSaveInitializer.SaveLevelData(currentLevel, true, true, coinsCollected);
                    await CloudSaveInitializer.SaveLevelData(currentLevel + 1, true, false);

                    // Backup to PlayerPrefs
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                    PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                    PlayerPrefs.Save();

                    Debug.Log($"Level {currentLevel} completed with {coinsCollected} stars. Saved directly to cloud.");
                }
                else
                {
                    // Fallback to PlayerPrefs only
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                    PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                    PlayerPrefs.Save();

                    Debug.Log($"Level {currentLevel} completed with {coinsCollected} stars. Saved to PlayerPrefs.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save level completion: {e.Message}");

                // Fallback to PlayerPrefs
                PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                PlayerPrefs.Save();
            }
            finally
            {
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
            }
        }

        // Play interstitial ad before loading the next level
        AdsManager adsManager = GameObject.FindFirstObjectByType<AdsManager>();
        if (adsManager != null)
        {
            adsManager.PlayInterstitialAd();
        }

        // Reset coins for the next level
        if (coinManager != null)
        {
            coinManager.ResetCoins();
        }

        if (currentLevel > 0)
        {
            string nextLevelName = $"Level_{currentLevel + 1}";

            // Check if the next level exists
            if (Application.CanStreamedLevelBeLoaded(nextLevelName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // Return to level select
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
            }
        }
        else
        {
            // If we couldn't determine the current level, go back to level select
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
        }

        isSaving = false;
    }

    // You can call this method from a button to return to level select
    public async void ReturnToLevelSelect()
    {
        if (isSaving) return;
        isSaving = true;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        // Mark level as complete if player has collected stars
        if (coinsCollected > 0 && currentLevel > 0)
        {
            try
            {
                if (levelSelect != null)
                {
                    // Cloud save is handled inside the MarkLevelAsCompleted method
                    levelSelect.MarkLevelAsCompleted(currentLevel, coinsCollected);
                }
                else if (useCloudSave)
                {
                    // Direct cloud save if LevelSelect not found
                    await CloudSaveInitializer.SaveLevelData(currentLevel, true, true, coinsCollected);

                    // If all stars collected, also unlock next level
                    if (coinsCollected >= 3 && currentLevel < 100) // Assuming max 100 levels
                    {
                        await CloudSaveInitializer.SaveLevelData(currentLevel + 1, true, false);
                    }

                    // Backup to PlayerPrefs
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Stars", coinsCollected);

                    if (coinsCollected >= 3)
                    {
                        PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                    }

                    PlayerPrefs.Save();
                }
                else
                {
                    // Fallback to PlayerPrefs only
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                    PlayerPrefs.SetInt($"Level_{currentLevel}_Stars", coinsCollected);

                    if (coinsCollected >= 3)
                    {
                        PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                    }

                    PlayerPrefs.Save();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save level completion: {e.Message}");

                // Fallback to PlayerPrefs
                PlayerPrefs.SetInt($"Level_{currentLevel}_Completed", 1);
                PlayerPrefs.SetInt($"Level_{currentLevel}_Stars", coinsCollected);

                if (coinsCollected >= 3)
                {
                    PlayerPrefs.SetInt($"Level_{currentLevel + 1}_Unlocked", 1);
                }

                PlayerPrefs.Save();
            }
            finally
            {
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
            }
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");

        if (coinManager != null)
        {
            coinManager.ResetCoins();
        }

        isSaving = false;
    }

    // Optional: Call this to replay the current level
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        if (coinManager != null)
        {
            coinManager.ResetCoins();
        }
    }
}