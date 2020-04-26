using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public Camera mainCam;
    public Transform mainCamStartVal , mainCamGameVal;
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
    public GameObject enemyHitTextVfx , enemyHitVirusVfx;
    public GameObject collideWithWallVfx;
    public GameObject starCollectVfx , fireworkTrials , flying_ember , environment_Bubble;

    [Header("UI")]
    public float totalFillbarTime;
    public Image fillbarImg;
    public GameObject starPrefab;
    public Transform startHolder , starWinPanelHolder;
    public Animator gotInfectedAnims , fillBarAnims , findExitAnim , gameWinAnim;
    public GameObject GameWinPanel;
    public List<RectTransform> starsInWinPanel = new List<RectTransform>();

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
                playerObj.speed = Mathf.Clamp(playerObj.speed,0.5f, transitionDelayTemp / transitionDelay * 2f);
            }

            Vector3 lookDir = (Vector3.forward * joystick.Vertical + Vector3.right * joystick.Horizontal) * 100;
            playerTransform.LookAt(lookDir);
            //To Increase Speed
            playerRb.velocity = playerTransform.forward * moveSpeed;
        }
        else if (!isPlayerStopped) {
            playerObj.SetBool("Stop" , true);
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
        Invoke("DisableCameraAnimNow" , 1f);

        moveSpeed = 0f;
        transitionDelayTemp = 0f;

        //FillEnemyChaseList();
        ToggleEnemiesOnOrOff(true);
        FillCollectables();
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
        material.mainTexture = enemyClothes[Random.Range(0 , enemyClothes.Length)];
    }

    public void ToggleEnemiesOnOrOff(bool isEnable) {

        totalEnemiesInLevel = Mathf.Clamp(totalEnemiesInLevel , 0 , totalEnemies.Count);

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
        exitGateChosen.y = 1.1f;
        exitGate.transform.position = exitGateChosen;
        exitGate.SetActive(false);
    }

    public void StarFiller() {
        for (int i = 0; i < starsToMake; i++)
        {
            starChilds.Add(Instantiate(starPrefab, startHolder).transform.GetChild(0).gameObject);
            starChildScripts.Add(starChilds[i].GetComponent<StarCollectedScript>());
        }
    }

    public void StarCollected() {
        if (starChilds.Count - 1 >= 0 && !isTimeOver)
        {
            starChilds[starChilds.Count - 1].SetActive(true);
            starChilds.RemoveAt(starChilds.Count - 1);
        }
    }

    public void GameOverAnimations() {
        fillBarAnims.Play("FillbarAnimUp");
        gotInfectedAnims.Play("GotInfectedAnim");
    }

    public void GameRestartButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public Vector3 RandomPosition() {

        Vector3 randomDirection;

        do
        {
            randomDirection = new Vector3(SpawnableArea.position.x + Random.Range(SpawnableArea.localScale.x / 2 * -1, SpawnableArea.localScale.x / 2 * 1),
            1f, SpawnableArea.position.z + Random.Range(SpawnableArea.localScale.z / 2 * -1, SpawnableArea.localScale.z / 2 * 1));
        } while (Vector3.Distance(randomDirection , playerTransform.position) < 7f && !hasGameStarted);

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

    public void CreateParticleEffect(int effectType , float timer ,Vector3 pos) {

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

        StartCoroutine(CreateParticleEffectDelayer(particleEffect , timer , pos));
    }

    public void GameWinFunction(float delayTime) {
        StartCoroutine(GameWinDelay(delayTime));
        findExitAnim.gameObject.SetActive(false);
    }

    IEnumerator CreateParticleEffectDelayer(GameObject effect , float timer , Vector3 pos) {

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

    }

    IEnumerator MoveStarsScript(float starTimeInterval) {
        for (int i = 0; i < starChildScripts.Count; i++)
        {
         //   starChildScripts[i].targetPos =
            yield return new WaitForSeconds(starTimeInterval);
        }
    }
}
