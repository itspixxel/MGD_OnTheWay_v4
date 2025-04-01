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
        if (coinsCollected >= totalCoinsInLevel)
            return 3;
        else if (coinsCollected >= totalCoinsInLevel * 0.5f)
            return 2;
        else if (coinsCollected > 0)
            return 1;
        return 0;
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