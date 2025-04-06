using UnityEngine;
using UnityEngine.Events;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private int totalCoinsInLevel = 3;
    [SerializeField] private int coinsCollected = 0;

    public UnityEvent<int> OnCoinsCollectedChanged;
    public UnityEvent<int> OnStarsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectCoin()
    {
        coinsCollected++;
        OnCoinsCollectedChanged?.Invoke(coinsCollected);
        
        // Calculate stars based on coins collected
        int stars = CalculateStars();
        OnStarsChanged?.Invoke(stars);
    }

    private int CalculateStars()
    {
        // Each coin equals one star
        return Mathf.Min(coinsCollected, 3);
    }

    public int GetCoinsCollected()
    {
        return coinsCollected;
    }

    public int GetTotalCoins()
    {
        return totalCoinsInLevel;
    }

    public void ResetCoins()
    {
        coinsCollected = 0;
        OnCoinsCollectedChanged?.Invoke(coinsCollected);
        OnStarsChanged?.Invoke(0);
    }
} 