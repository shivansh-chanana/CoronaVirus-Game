using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;
using UnityEngine.Analytics;

public class GameController : MonoBehaviour
{
    public Camera mainCam;
    public Transform mainCamStartVal, mainCamGameVal;
    public Transform playerTransform;
    public GameObject playerGameStart;
    public FloatingJoystick joystick;
    public Animator playerObj;
    public Animator mainCamAnim;

    [Header("Walk vars")]
    public float moveSpeed;
    public float transitionDelay;

    [Header("Enemies")]
    public int totalEnemiesInLevel;
    public List<GameObject> totalEnemies = new List<GameObject>();
    public List<EnemyScript> availableEnemyList = new List<EnemyScript>();
    public int totalEnemiesChasing;
    public List<float> enemiesDistanceFromPlayer = new List<float>();
    public Texture[] enemyClothes;

    [Header("Collectables")]
    public int spawnItemsToCreate;
    public Transform SpawnableArea;
    public GameObject[] collectableObj;
    public Sprite[] collectableSpr;
    public GameObject exitGatePrefab;
    public Transform[] exitGateOptions;

    [Header("VFX")]
    public GameObject collectableVfx;
    public GameObject enemyHitTextVfx, enemyHitVirusVfx;
    public GameObject collideWithWallVfx;
    public GameObject starCollectVfx, fireworkTrials, flying_ember, environment_Bubble;

    [Header("UI")]
    public float totalFillbarTime;
    public Image fillbarImg;
    public GameObject starPrefab;
    public Transform startHolder;
    public Animator gotInfectedAnims, fillBarAnims, findExitAnim, gameWinAnim;
    public GameObject GameWinPanel;
    public List<RectTransform> starsInWinPanel = new List<RectTransform>();
    public GameObject costumeSelectPrefab;
    public Transform costumeSelectButtonHolder;
    public Sprite[] costumeButtonStateSprites;

    [Header("MysterChar")]
    public GameObject mysteryCharImg;
    public List<Sprite> allMysteryCharImg;
    public Material bodyMat;
    public List<Sprite> allMysteryCharSkins;
    public GameObject SetCostumeButton;

    [Header("Serialize Field")]
    [Space(10)]
    public bool hasGameStarted;
    public bool isGameOver;
    public bool isGameWin;
    public bool isTimeOver;
    [SerializeField] int collectablesCollected;
    [SerializeField] Vector3 camOffset;
    [SerializeField] bool isPlayerStopped;
    [SerializeField] float moveSpeedTemp;
    [SerializeField] float transitionDelayTemp;
    [SerializeField] Vector2 fillBarSize;
    [SerializeField] List<GameObject> starChilds = new List<GameObject>();
    [SerializeField] List<StarCollectedScript> starChildScripts = new List<StarCollectedScript>();
    public int currentChasingEnemies;

    int starsToMake;
    float totalTimeTemp;
    Rigidbody playerRb;
    GameObject exitGate;

    public static GameController instance;

    /* playerprefs
        CurrentWinPanelStars
        currentPlayerToUnlock
        currentPlayerSelected

        PlayerPrefsX.GetBool("GameFinished")
    */

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hasGameStarted = false;
        isGameOver = false;
        isTimeOver = false;
        collectablesCollected = 0;
        fillBarSize = fillbarImg.rectTransform.sizeDelta;

        mainCamAnim.enabled = false;
        mainCam.transform.position = mainCamStartVal.position;
        mainCam.transform.rotation = mainCamStartVal.rotation;
        playerObj.SetBool("Stop", true);
        isPlayerStopped = true;

        starsToMake = spawnItemsToCreate + 1;
        totalFillbarTime *= starsToMake;
        totalTimeTemp = totalFillbarTime;

        GameWinPanel.SetActive(false);
        StarFiller();
        FillCostumeSelectionScreen();
    }

    void Update()
    {
        if (!hasGameStarted) return;

        //StartRotatingCamera
        if (isGameOver || isGameWin)
        {
            mainCam.transform.RotateAround(playerTransform.position, Vector3.up, 0.4f);
            //mainCam.transform.LookAt(playerTransform);
        }
        else
        {
            //Camera Follow 
            mainCam.transform.position = playerTransform.position + camOffset;

            if (!isTimeOver)
            {
                //Fillbar size
                totalFillbarTime -= Time.deltaTime;

                fillbarImg.rectTransform.sizeDelta = new Vector2(totalFillbarTime / totalTimeTemp * fillBarSize.x, fillBarSize.y);

                if (totalFillbarTime <= 0) {
                    isTimeOver = true;
                    fillBarAnims.Play("FillbarAnimUp");
                    exitGate.GetComponent<TargetIndigator>().enabled = true;
                }
            }
        }

        //Joystick Control
        if (Input.GetMouseButton(0))
        {
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
        else if (!isPlayerStopped) {
            playerObj.SetBool("Stop", true);
            isPlayerStopped = true;

            moveSpeed = 0f;
            transitionDelayTemp = 0f;
        }
    }

    public void HitWall() {
        playerObj.speed = 0;
        playerRb.velocity = Vector3.zero;

        moveSpeed = 0f;
        transitionDelayTemp = 0f;
    }

    public void StartGameNow() {
        hasGameStarted = true;
        moveSpeedTemp = moveSpeed;
        playerGameStart.SetActive(false);
        playerTransform.root.gameObject.SetActive(true);

        playerRb = playerTransform.GetComponent<Rigidbody>();
        mainCamAnim.enabled = true;
        mainCamAnim.Play("CameraUp");
        Invoke("DisableCameraAnimNow", 1f);

        moveSpeed = 0f;
        transitionDelayTemp = 0f;

        //FillEnemyChaseList();
        ToggleEnemiesOnOrOff(true);
        FillCollectables();

        AnalyticsCall("Game Started");
    }

    public void CollectableCollected() {
        collectablesCollected++;

        if (collectablesCollected >= spawnItemsToCreate) {
            exitGate.SetActive(true);
            findExitAnim.Play("FindExitTxtAnim");
        }
    }

    void DisableCameraAnimNow() {
        mainCamAnim.enabled = false;
        mainCam.transform.position = mainCamGameVal.position;
        mainCam.transform.rotation = mainCamGameVal.rotation;

        //get camera and playerOffsetNow
        camOffset = mainCam.transform.position - playerTransform.position;
    }

    public void DressMeUp(Material material) {
        material.mainTexture = enemyClothes[Random.Range(0, enemyClothes.Length)];
    }

    public void ToggleEnemiesOnOrOff(bool isEnable) {

        totalEnemiesInLevel = Mathf.Clamp(totalEnemiesInLevel, 0, totalEnemies.Count);

        for (int i = 0; i < totalEnemiesInLevel; i++)
        {
            totalEnemies[i].SetActive(isEnable);
        }
    }

    public void ToggleEnemyKinematic(bool isEnable)
    {
        for (int i = 0; i < availableEnemyList.Count; i++)
        {
            availableEnemyList[i].KinematicsToggle(isEnable);
        }
    }

    void FillEnemyChaseList() {
        GameObject[] totalEnemies = GameObject.FindGameObjectsWithTag("Enemy_1");

        int currentNum = 0;
        foreach (GameObject enemy in totalEnemies) {
            enemy.GetComponent<EnemyScript>().enemyNum = currentNum;
            enemiesDistanceFromPlayer.Add(1000);
            currentNum++;
        }

        totalEnemies = null;
    }

    void FillCollectables() {

        for (int i = 0; i < spawnItemsToCreate; i++)
        {
            int randomCollectable = Random.Range(0, collectableObj.Length);

            Vector3 hit = RandomPosition();

            Transform collectable = Instantiate(collectableObj[randomCollectable]).transform;
            collectable.position = new Vector3(hit.x, 1f, hit.z);

            collectable.GetComponent<CollectableScript>().collectableNum = randomCollectable;
        }

        exitGate = Instantiate(exitGatePrefab);
        Vector3 exitGateChosen = exitGateOptions[Random.Range(0, exitGateOptions.Length)].position;
        exitGateChosen.y = 2.5f;
        exitGate.transform.position = exitGateChosen;
        exitGate.SetActive(false);
    }

    public void StarFiller() {
        for (int i = 0; i < starsToMake; i++)
        {
            starChilds.Add(Instantiate(starPrefab, startHolder).transform.GetChild(0).gameObject);
        }
    }

    public void StarCollected() {
        if (starChilds.Count - 1 >= 0 && !isTimeOver)
        {
            starChilds[starChilds.Count - 1].SetActive(true);
            starChildScripts.Add(starChilds[starChilds.Count - 1].transform.parent.GetComponent<StarCollectedScript>());
            starChilds.RemoveAt(starChilds.Count - 1);
        }
    }

    public void GameOverAnimations() {
        AnalyticsCall("Game Lose");
        fillBarAnims.Play("FillbarAnimUp");
        gotInfectedAnims.Play("GotInfectedAnim");
    }

    public void GameRestartButton() {
        AnalyticsCall("GameRestarted");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public Vector3 RandomPosition() {

        Vector3 randomDirection;

        do
        {
            randomDirection = new Vector3(SpawnableArea.position.x + Random.Range(SpawnableArea.localScale.x / 2 * -1, SpawnableArea.localScale.x / 2 * 1),
            1f, SpawnableArea.position.z + Random.Range(SpawnableArea.localScale.z / 2 * -1, SpawnableArea.localScale.z / 2 * 1));
        } while (Vector3.Distance(randomDirection, playerTransform.position) < 7f && !hasGameStarted);

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, SpawnableArea.transform.localScale.x, 1);

        return hit.position;
    }

    public void MakeEnemiesChase() {

        for (int i = 0; i < totalEnemiesChasing - currentChasingEnemies;)
        {
            currentChasingEnemies++;
        }
    }

    public void CreateParticleEffect(int effectType, float timer, Vector3 pos) {

        GameObject particleEffect;

        //Don't collect star if timer is over
        if (isTimeOver && effectType == 4) return;

        switch (effectType) {
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

    public void GameWinFunction(float delayTime) {
        StartCoroutine(GameWinDelay(delayTime));
        findExitAnim.gameObject.SetActive(false);

        AnalyticsCall("Game Won");
    }

    IEnumerator CreateParticleEffectDelayer(GameObject effect, float timer, Vector3 pos) {

        yield return new WaitForSeconds(timer);

        Transform particleEffectPos = Instantiate(effect).transform;
        particleEffectPos.position = pos;

        yield return null;
    }

    IEnumerator GameWinDelay(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        GameWinPanel.SetActive(true);
        gameWinAnim.Play("GameWinAnims");
        fillBarAnims.Play("FillbarAnimDown");
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveStarsScript(0.2f));
    }

    IEnumerator MoveStarsScript(float starTimeInterval) {

        int prevStars = PlayerPrefs.GetInt("CurrentWinPanelStars", 0);
        startHolder.GetComponent<HorizontalLayoutGroup>().enabled = false;

        //Activate collected stars
        for (int i = 0; i < prevStars; i++)
        {
            starsInWinPanel[i].transform.GetChild(0).gameObject.SetActive(true);
        }
        //remove collected stars from list
        for (int i = 0; i < prevStars; i++)
        {
            starsInWinPanel.RemoveAt(0);
        }

        //Store new collected stars
        for (int i = 0; i < starChildScripts.Count; i++)
        {
            starChildScripts[(starChildScripts.Count - 1) - i].targetPos = starsInWinPanel[i];
            starChildScripts[(starChildScripts.Count - 1) - i].isGoToTarget = true;

            if (i + 1 >= starsInWinPanel.Count)
            {
                StartCoroutine(UnlockMysteryChar(2f));
                prevStars = (starChildScripts.Count - (i + 1));
                PlayerPrefs.SetInt("CurrentWinPanelStars", prevStars);
                break;
            }

            prevStars++;

            yield return new WaitForSeconds(starTimeInterval);
        }
        PlayerPrefs.SetInt("CurrentWinPanelStars", prevStars);
    }

    IEnumerator UnlockMysteryChar(float delayTimer) {

        yield return new WaitForSeconds(delayTimer);

        int currentPlayerToUnlock = PlayerPrefs.GetInt("currentPlayerToUnlock", 0);
        mysteryCharImg.GetComponent<Image>().sprite = allMysteryCharImg[currentPlayerToUnlock];
        mysteryCharImg.SetActive(true);
        starsInWinPanel[0].parent.gameObject.SetActive(false);
        startHolder.gameObject.SetActive(false);
        SetCostumeButton.SetActive(true);

        int nextPlayerToUnlock = currentPlayerToUnlock + 1;
        if (nextPlayerToUnlock >= allMysteryCharImg.Count)
        {
            nextPlayerToUnlock = 0;
            PlayerPrefsX.SetBool("GameFinished" , true);

            AnalyticsCall("Game Finished");
        }

        PlayerPrefs.SetInt("currentPlayerToUnlock", nextPlayerToUnlock);

        if (PlayerPrefsX.GetBool("GameFinished")) AnalyticsCallForInt("Game Finished , Player Unlocked ", currentPlayerToUnlock);
        else AnalyticsCallForInt("Player Unlocked " , currentPlayerToUnlock);
    }

    public void CostumeSetButton() {

        int currentSelectedPlayer = PlayerPrefs.GetInt("currentPlayerToUnlock", 0) - 1;
        if (currentSelectedPlayer < 0) currentSelectedPlayer = 0;

        bodyMat.mainTexture = allMysteryCharSkins[currentSelectedPlayer].texture;

        PlayerPrefs.SetInt("currentPlayerSelected" , currentSelectedPlayer);

        GameRestartButton();
    }

    public void CostumeSetButtonFromCostumeScreen(int objName) {

        int prevSel = PlayerPrefs.GetInt("currentPlayerSelected", 0);
        costumeSelectButtonHolder.GetChild(prevSel).GetComponent<Image>().sprite = costumeButtonStateSprites[0];

        int childNum = objName;
        PlayerPrefs.SetInt("currentPlayerSelected", childNum);
        costumeSelectButtonHolder.GetChild(childNum).GetComponent<Image>().sprite = costumeButtonStateSprites[1];

        bodyMat.mainTexture = allMysteryCharSkins[childNum].texture;

        AnalyticsCallForInt("Costume Set ", childNum);
    }

    void FillCostumeSelectionScreen() {

        for (int i = 0; i < costumeSelectButtonHolder.childCount; i++)
        {
            Destroy(costumeSelectButtonHolder.GetChild(i).gameObject);
        }

        int currentUnlockedCostumes = PlayerPrefs.GetInt("currentPlayerToUnlock", 0);
        if(PlayerPrefsX.GetBool("GameFinished", false)) currentUnlockedCostumes = allMysteryCharImg.Count;

        int currentSelectedPlayer = PlayerPrefs.GetInt("currentPlayerSelected", 0);

        for (int i = 0; i < allMysteryCharImg.Count; i++)
        {
            GameObject costumeButton = Instantiate(costumeSelectPrefab , costumeSelectButtonHolder);

            if (i < currentUnlockedCostumes) {
                costumeButton.transform.GetChild(0).GetComponent<Image>().sprite = allMysteryCharImg[i];
                costumeButton.GetComponent<Button>().interactable = true;
                costumeButton.name = i.ToString();
                Debug.Log("button unlocked : " + i);
            }
        }

        int childNum = PlayerPrefs.GetInt("currentPlayerSelected", 0);
        costumeSelectButtonHolder.GetChild(childNum).GetComponent<Image>().sprite = costumeButtonStateSprites[1];
    }

    public void SetPlayerMaterialTex(int sprNum) {
        bodyMat.mainTexture = allMysteryCharSkins[sprNum].texture;
    }

    public void AnalyticsCall(string eventName)
    {

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

    public void AnalyticsCallForInt(string eventName , int value) {
        //Send tutorial start //analytics
        #region Game Analytics
        GameAnalytics.NewDesignEvent(eventName, value);
        #endregion

        #region Unity Analytics
        AnalyticsEvent.Custom(eventName, new Dictionary<string, object>
        {
            { "Value", value},
        });
        #endregion
    }
}
