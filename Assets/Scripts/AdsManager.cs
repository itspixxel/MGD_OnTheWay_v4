using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    [SerializeField]
    private AdsInitializer adsInitializer;

    [SerializeField]
    private float inactivityThreshold = 30f; // Time in seconds before showing banner ad

    private float lastActivityTime;
    private bool isBannerShowing = false;

    private void Awake()
    {
        if (adsInitializer != null)
        {
            adsInitializer.InitializeAds();
        }

        lastActivityTime = Time.time;
    }

    private void Update()
    {
        // Check for player activity
        if (Input.anyKeyDown || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            lastActivityTime = Time.time;

            // Hide banner if showing
            if (isBannerShowing)
            {
                HideBannerAd();
                isBannerShowing = false;
            }
        }

        // Show banner after inactivity period
        if (!isBannerShowing && Time.time - lastActivityTime >= inactivityThreshold)
        {
            ShowBannerAd();
        }
    }

    public void PlayInterstitialAd()
    {
        adsInitializer.GetComponent<InterstitialAds>().ShowAd();
    }

    private void ShowBannerAd()
    {
        var bannerAd = adsInitializer.GetComponent<BannerAd>();
        if (bannerAd != null)
        {
            bannerAd.LoadBanner();

            // This assumes the BannerAd class has a way to immediately show the banner
            // If not, you might need to modify the BannerAd class or add a callback
            bannerAd.ShowBannerAd();
            isBannerShowing = true;
        }
    }

    private void HideBannerAd()
    {
        var bannerAd = adsInitializer.GetComponent<BannerAd>();
        if (bannerAd != null)
        {
            bannerAd.HideBannerAd();
        }
    }
}