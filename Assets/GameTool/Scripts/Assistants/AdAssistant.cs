using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GoogleMobileAds.Api;
using Berry.Utils;

public class AdAssistant : SingletonMonoBehaviour<AdAssistant>
{

    // Settings
    // AdMob
    public string AdMob_Interstitial_Android = "";
    public string AdMob_Interstitial_iOS = "";

    public string AdMob_Baner_Android = "";
    public string AdMob_Baner_iOS = "";


    public override void Awake()
    {
        RequestBanner();
        RequestInterstitial();
        ShowBanner();
    }


    string GetAdMobIDs(AdType adType)
    {
        switch (adType)
        {
            case AdType.Interstitial:
                switch (Application.platform)
                {
                    case RuntimePlatform.Android: return AdMob_Interstitial_Android;
                    case RuntimePlatform.IPhonePlayer: return AdMob_Interstitial_iOS;
                }
                break;
            case AdType.Banner:
                switch (Application.platform)
                {
                    case RuntimePlatform.Android: return AdMob_Baner_Android;
                    case RuntimePlatform.IPhonePlayer: return AdMob_Baner_iOS;
                }
                break;
            default:
                break;
        }
        return "";
    }


    public void ShowAds(AdType adType)
    {
        switch (adType)
        {
            case AdType.Interstitial:
                ShowInterstitial();
                break;
            case AdType.Banner:
                ShowBanner();
                break;
            default:
                break;
        }

    }


    BannerView bannerView;

    private void RequestBanner()
    {
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(GetAdMobIDs(AdType.Banner), AdSize.SmartBanner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    InterstitialAd interstitial;

    private void RequestInterstitial()
    {
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(GetAdMobIDs(AdType.Interstitial));
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
    }

    public void ShowInterstitial()
    {
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
            RequestInterstitial();
        }
    }

    public void ShowBanner()
    {
        if (Utilities.IsInternetAvailable())
        {
            bannerView.Show();
            Debug.Log("Show");
        }
    }

    public void DestroyBanner()
    {
        bannerView.Destroy();
    }

}

public enum AdType
{
    Interstitial,
    Banner
}