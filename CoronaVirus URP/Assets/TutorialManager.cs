using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using GameAnalyticsSDK;

[System.Serializable]
public enum TutorialPhase{
    CollectPhase,
    DodgeAndCollectPhase,
    ExitPhase
}

public class TutorialManager : MonoBehaviour
{
    public Camera mainCam;
    public Transform playerTransform;
    public Animator playerObj;
    public FloatingJoystick joystick;
    public Animator tutorialAnim, avoidPeopleAnim;
    public GameObject findExitGate;
    public Transform exitGate;
    public GameObject playButton;
    public GameObject skipButton;

    [Header("Enemies")]
    public List<Tutorial_EnemyScript> TotalEnemies = new List<Tutorial_EnemyScript>();

    [Header("Phase Vars")]
    [SerializeField] bool isPhaseOneDone;

    [Header("Walk vars")]
    public float moveSpeed;
    public float transitionDelay;

    [Header("VFX")]
    public GameObject collectableVfx;
    public GameObject enemyHitTextVfx, enemyHitVirusVfx;
    public GameObject collideWithWallVfx;
    public GameObject starCollectVfx, fireworkTrials, flying_ember, environment_Bubble;

    bool disableJoystickGuide;
    Rigidbody playerRb;

    [Header("Serialize Field")]
    public bool isTutorialWin;
    bool skipButtonPressed;
    [SerializeField] TutorialPhase currentPhase;
    [SerializeField] bool isPlayerStopped;
    [SerializeField] float moveSpeedTemp;
    [SerializeField] float transitionDelayTemp;
    [SerializeField] Vector3 camOffset;
    public Vector3 posCheckPoint;

    public static TutorialManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerObj.SetBool("Stop", true);
        isPlayerStopped = true;
        isTutorialWin = false;
        playButton.SetActive(false);

        StartGameNow();

        CheckForSkipButton();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTutorialWin)
        {
            mainCam.transform.RotateAround(playerTransform.position, Vector3.up, 0.4f);

           if(!skipButton) playerTransform.position = new Vector3(exitGate.position.x , playerTransform.position.y , exitGate.position.z);

            return;
        }

        //Camera Follow 
        mainCam.transform.position = playerTransform.position + camOffset;

        //Joystick Control
        if (Input.GetMouseButton(0))
        {
            if (!disableJoystickGuide) {
                disableJoystickGuide = true;

                //Hide Guide
                tutorialAnim.SetBool("HideGuide", true);
            }

            if (isPlayerStopped)
            {
                playerObj.SetBool("Stop", false);
                isPlayerStopped = false;
            }

            if (transitionDelayTemp < transitionDelay)
            {
                transitionDelayTemp += Time.deltaTime;
                moveSpeed = moveSpeedTemp * transitionDelayTemp / transitionDelay;
                playerObj.speed = Mathf.Clamp(playerObj.speed, 0.5f, transitionDelayTemp / transitionDelay * 2f);
            }

            Vector3 lookDir = (Vector3.forward * joystick.Vertical + Vector3.right * joystick.Horizontal) * 100;
            playerTransform.LookAt(lookDir);
            //To Increase Speed
            playerRb.velocity = playerTransform.forward * moveSpeed;
        }
        else if (!isPlayerStopped)
        {
            playerObj.SetBool("Stop", true);
            isPlayerStopped = true;

            moveSpeed = 0f;
            transitionDelayTemp = 0f;

            //Show guide
            if (disableJoystickGuide)
            {
                tutorialAnim.SetBool("HideGuide", false);
                disableJoystickGuide = false;
            }
        }

    }

    public void CheckForSkipButton() {
        if (PlayerPrefsX.GetBool("isTutorialComplete", false)) {
            skipButton.SetActive(true);
        }
    }

    public void HitWall()
    {
        playerObj.speed = 0;
        playerRb.velocity = Vector3.zero;

        moveSpeed = 0f;
        transitionDelayTemp = 0f;
    }

    public void StartGameNow()
    {
        posCheckPoint = playerTransform.position;
        moveSpeedTemp = moveSpeed;

        playerRb = playerTransform.GetComponent<Rigidbody>();

        moveSpeed = 0f;
        transitionDelayTemp = 0f;

        disableJoystickGuide = false;

        //get camera and playerOffsetNow
        camOffset = mainCam.transform.position - playerTransform.position;

        //Analytics call
        AnalyticsCall("Tutorial Started");
    }

    public void CreateParticleEffect(int effectType, float timer, Vector3 pos)
    {

        GameObject particleEffect;

        switch (effectType)
        {
            case 0:
                particleEffect = collectableVfx;
                break;
            case 1:
                particleEffect = enemyHitTextVfx;
                break;
            case 2:
                particleEffect = enemyHitVirusVfx;
                break;
            case 3:
                particleEffect = collideWithWallVfx;
                break;
            case 4:
                particleEffect = starCollectVfx;
                break;
            case 5:
                particleEffect = fireworkTrials;
                break;
            case 6:
                particleEffect = flying_ember;
                break;
            case 7:
                particleEffect = environment_Bubble;
                break;
            default:
                particleEffect = collectableVfx;
                break;
        }

        StartCoroutine(CreateParticleEffectDelayer(particleEffect, timer, pos));
    }

    IEnumerator CreateParticleEffectDelayer(GameObject effect, float timer, Vector3 pos)
    {

        yield return new WaitForSeconds(timer);

        Transform particleEffectPos = Instantiate(effect).transform;
        particleEffectPos.position = pos;

        yield return null;
    }

    public void AvoidPeopleHighlight(float timer) {
        StartCoroutine(AvoidPeople(timer));

        for (int i = 0; i < TotalEnemies.Count; i++)
        {
            TotalEnemies[i].ResetEnemyPosition();
        }
    }

    IEnumerator AvoidPeople(float timer) {

        bool isExitGateEnabled = findExitGate.activeInHierarchy;
        yield return new WaitForEndOfFrame();
        avoidPeopleAnim.gameObject.SetActive(true);
        findExitGate.SetActive(false);

        avoidPeopleAnim.Play("Tut_AvoidPeopleTxt");
        yield return new WaitForSeconds(timer);
        avoidPeopleAnim.Play("Idle");

        avoidPeopleAnim.gameObject.SetActive(!isExitGateEnabled);
        findExitGate.SetActive(isExitGateEnabled);
    }

    public void TutorialWin() {
        CreateParticleEffect(5 , 0f , playerTransform.position);

        for (int i = 0; i < TotalEnemies.Count; i++)
        {
            TotalEnemies[i].StopEnemies();
        }

        isTutorialWin = true;
        joystick.gameObject.SetActive(false);
        playButton.SetActive(true);

        PlayerPrefsX.SetBool("isTutorialComplete", true);

        //Analytics Call
        AnalyticsCall("Tutorial Ended");
    }

    public void SkippingTutorial() {
        CreateParticleEffect(5, 0f, playerTransform.position);

        if (TotalEnemies[0].transform.root.gameObject.activeSelf)
        {
            for (int i = 0; i < TotalEnemies.Count; i++)
            {
                TotalEnemies[i].StopEnemies();
            }
        }

        isTutorialWin = true;
        skipButtonPressed = true;
        joystick.gameObject.SetActive(false);
        playButton.SetActive(true);

        //Analytics Call
        AnalyticsCall("SkippingTutorial");
    }

    public void MoveToMainGame() {
        SceneManager.LoadScene("Level_1");
    }

    public void AnalyticsCall(string eventName) {

        //Send tutorial start //analytics
        #region Game Analytics
        GameAnalytics.NewDesignEvent(eventName, Time.timeSinceLevelLoad);
        #endregion

        #region Unity Analytics
        AnalyticsEvent.Custom(eventName, new Dictionary<string, object>
        {
            { "StartTime", Time.timeSinceLevelLoad},
        });
        #endregion

    }
}
