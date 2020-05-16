using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hellmade.Sound;
using GoogleMobileAds.Api;

public class SoundManager : MonoBehaviour
{
    public int RestartNum = 0;
    public int myLevel;

    [Header("Background Music")]
    [SerializeField] AudioClip bgMusicAudioClip;
    [SerializeField] [Range(0.0f, 1f)] float bgMusicVolume;

    [Space]

    [Header("Enemy Found Player")]
    [SerializeField] List<AudioClip> playerFoundByEnemySound;
    [SerializeField] [Range(0.0f, 1f)] float pfbeAudioVolume;

    [Space]

    [Header("Pickup")]
    [SerializeField] AudioClip pickupAudioClip;
    [SerializeField] AudioClip starCollected;
    [SerializeField] [Range(0.0f, 1f)] float pickUpVolume;

    [Space]

    [Header("UI")]
    [SerializeField] AudioClip uiTapSoundClip;

    [Space]

    [Header("Misc")]
    [SerializeField] AudioClip wallHitAudioClip;
    [SerializeField] AudioClip peopleCollideAudioClip;
    [SerializeField] [Range(0.0f, 1f)] float miscVolume;

    [Header("Private")]
    [SerializeField] List<AudioClip> pfbeSoundsPlayed = new List<AudioClip>();
    [SerializeField] bool isSoundOn;

    public static SoundManager instance;

    public RewardedAd rewardedAd;
    public BannerView bannerView;
    public InterstitialAd interstitial;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        myLevel = PlayerPrefs.GetInt("MyLevel", 0);
    }

    private void Start()
    {
        CreateAndLoadRewardedAd();

        ShowInterstitialAd();
    }

    public void BgMusicPlay() {

        if(CheckSoundStatus())EazySoundManager.PlayMusic(bgMusicAudioClip, bgMusicVolume, true, true, 1f, 1f);
    }

    public void PlayPlayerFoundByEnemy(Transform enemyTransform)
    {
        int currentAudioClipNum = Random.Range(0, playerFoundByEnemySound.Count-1);
        EazySoundManager.PlaySound(playerFoundByEnemySound[currentAudioClipNum],
            pfbeAudioVolume, false , enemyTransform);
        pfbeSoundsPlayed.Add(playerFoundByEnemySound[currentAudioClipNum]);
        playerFoundByEnemySound.Remove(playerFoundByEnemySound[currentAudioClipNum]);

        if (playerFoundByEnemySound.Count <= 0) {
            playerFoundByEnemySound = pfbeSoundsPlayed;
            pfbeSoundsPlayed = new List<AudioClip>();
        }
    }

    public void PickUpSound(bool isPickup , float time) {
       if(isPickup) EazySoundManager.PlaySound(pickupAudioClip, miscVolume, false, null);

        StartCoroutine(PlaySoundNow(starCollected , time , miscVolume));
    }

    IEnumerator PlaySoundNow(AudioClip audioClip , float time , float volume) {
        yield return new WaitForSeconds(time);
        EazySoundManager.PlaySound(audioClip, miscVolume, false, null);
    }

    public void MiscSound(int soundId) {
        switch (soundId)
        {
            case 0://wall hit
                EazySoundManager.PlaySound(wallHitAudioClip, miscVolume, false, null);
                break;
            case 1:// people hit
                EazySoundManager.StopAllMusic();
                EazySoundManager.StopAllSounds();
                EazySoundManager.PlaySound(peopleCollideAudioClip, miscVolume, false, null);
                break;
            default:
                break;
        }
    }

    public void UiTapSound() {
        EazySoundManager.PlayUISound(uiTapSoundClip,100f);
    }

    bool CheckSoundStatus() {
         isSoundOn = PlayerPrefsX.GetBool("SoundStatus" , true);

        if (isSoundOn)
        {
            EazySoundManager.GlobalVolume = 1;
            GameController.instance.soundButton.sprite = GameController.instance.soundSpr[0];
        }
        else
        {
            EazySoundManager.GlobalVolume = 0;
            GameController.instance.soundButton.sprite = GameController.instance.soundSpr[1];
        }

        return isSoundOn;
    }

    public bool ToggleSound() {
        if (isSoundOn)
        {
            isSoundOn = false;
            PlayerPrefsX.SetBool("SoundStatus", false);
            EazySoundManager.GlobalVolume = 0;
        }
        else {
            isSoundOn = true;
            PlayerPrefsX.SetBool("SoundStatus", true);
            EazySoundManager.GlobalVolume = 1;
        }

        return isSoundOn;
    }

    //OTHER THAN SOUND

    public void IncreaseMyLevel() {
        PlayerPrefs.SetInt("MyLevel", myLevel+1);
        myLevel = PlayerPrefs.GetInt("MyLevel", 0);
    }

    //ADVERTISEMENT RELATED

    void CreateAndLoadRewardedAd() {
        #region rewarded ad 
        //rewardedAd = new RewardedAd("ca-app-pub-3940256099942544/5224354917"); //test id
        rewardedAd = new RewardedAd("ca-app-pub-8136549304982140/6616527702");

        // Called when the user should be rewarded for interacting with the ad.
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);
        #endregion
    }

    public void ShowBannerAd()
    {

        //For test devices//
        //List<string> deviceIds = new List<string>();
        //deviceIds.Add("59F44F659D5B6214");
        //RequestConfiguration requestConfigurationBuilder = new RequestConfiguration
        //    .Builder()
        //    .SetTestDeviceIds(deviceIds)
        //    .build();

        //MobileAds.SetRequestConfiguration(requestConfigurationBuilder);

        string adUnitId; // = "ca-app-pub-3940256099942544/6300978111";  //Demo ads
        adUnitId = "ca-app-pub-8136549304982140/2516666447";

        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    public void HandleRewardedAdClosed(object sender, System.EventArgs args)
    {
        CreateAndLoadRewardedAd();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        GameController.instance.isShieldOn = true;
        GameController.instance.shield.SetActive(true);
    }

    public void ShowInterstitialAd()
    {
        //For test devices//
        //List<string> deviceIds = new List<string>();
        //deviceIds.Add("59F44F659D5B6214");
        //RequestConfiguration requestConfigurationBuilder = new RequestConfiguration
        //    .Builder()
        //    .SetTestDeviceIds(deviceIds)
        //    .build();

        //MobileAds.SetRequestConfiguration(requestConfigurationBuilder);

        string adUnitId;// = "ca-app-pub-3940256099942544/1033173712";  //Demo ads
        adUnitId = "ca-app-pub-8136549304982140/3088346239";

        interstitial = new InterstitialAd(adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the Interstitial with the request.
        interstitial.LoadAd(request);
    }
}