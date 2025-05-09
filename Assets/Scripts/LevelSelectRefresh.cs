using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectRefresh : MonoBehaviour
{
    private void Awake()
    {
        // This script ensures levels are properly unlocked when entering the level select screen
        VerifyLevelUnlocks();
    }

    private void VerifyLevelUnlocks()
    {
        int highestCompletedLevel = 0;

        // Find the highest completed level
        for (int i = 1; i <= 20; i++) // Assuming max of 20 levels
        {
            if (PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 1)
            {
                highestCompletedLevel = i;
            }
        }

        // Make sure all levels up to the highest completed level + 1 are unlocked
        for (int i = 1; i <= highestCompletedLevel + 1; i++)
        {
            PlayerPrefs.SetInt($"Level_{i}_Unlocked", 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"Verified level unlocks. Highest completed: {highestCompletedLevel}, Next unlocked: {highestCompletedLevel + 1}");
    }

    // For debugging - attach to a button in level select if needed
    public void ForceUnlockNextLevel()
    {
        int highestCompletedLevel = 0;

        // Find the highest completed level
        for (int i = 1; i <= 20; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 1)
            {
                highestCompletedLevel = i;
            }
        }

        // Unlock the next level
        if (highestCompletedLevel > 0)
        {
            PlayerPrefs.SetInt($"Level_{highestCompletedLevel + 1}_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"Forced unlock of Level {highestCompletedLevel + 1}");

            // Reload the scene to refresh UI
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // For debugging - attach to a button in level select if needed
    public void ResetAllLevels()
    {
        for (int i = 2; i <= 20; i++)
        {
            PlayerPrefs.DeleteKey($"Level_{i}_Completed");
            PlayerPrefs.DeleteKey($"Level_{i}_Unlocked");
            PlayerPrefs.DeleteKey($"Level_{i}_Stars");
        }

        // Always keep level 1 unlocked
        PlayerPrefs.SetInt("Level_1_Unlocked", 1);
        PlayerPrefs.Save();
        Debug.Log("Reset all level progress");

        // Reload the scene to refresh UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}