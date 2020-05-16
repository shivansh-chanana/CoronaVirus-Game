using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public bool isRandomJumpDistance, isRandomSpeed;
    public float moveSpeed;
    public float chaseDistance, jumpDistance, offscreenDistance;
    public Vector2 randomJumpDistanceValues, randomMoveSpeed;
    public float resetTimer;
    public Animator objAnim;
    public SkinnedMeshRenderer skinnedMeshRenderer, headSMR;
    public Outline[] outlinesScript;
    public ParticleSystem bubbleSpeech;

    Transform target;
    Vector3 targetPos;
    Rigidbody rb;
    NavMeshAgent navMesh;

    [Header("SerializeField")]
    [SerializeField] bool hasGameStarted;
    [SerializeField] bool isGameOver, isEnemyKinematic;
    [SerializeField] float startPosY;
    [SerializeField] bool isPlayerSpoted;
    [SerializeField] float distanceFromTarget;
    [SerializeField] bool hasJumped;
    [SerializeField] float resetTimerTemp;
    [SerializeField] Rigidbody[] rigidbodies;
    public bool isSelected;
    public int enemyNum;
    public GameObject indicatorIcon;

    void Start()
    {
        hasJumped = false;
        resetTimerTemp = resetTimer;
        startPosY = transform.position.y;
        objAnim.SetBool("PlayerFound", true);

        target = GameController.instance.playerTransform;
        rb = GetComponent<Rigidbody>();
        navMesh = GetComponent<NavMeshAgent>();

        isSelected = true;

        GameController.instance.DressMeUp(skinnedMeshRenderer.material);
        headSMR.material.mainTexture = skinnedMeshRenderer.material.mainTexture;
        RandomValues();

        for (int i = 0; i < outlinesScript.Length; i++)
        {
            outlinesScript[i].enabled = false;
        }

        rigidbodies = GetComponentsInChildren<Rigidbody>();

        GameController.instance.totalEnemies.Add(transform.parent.gameObject);
        GameController.instance.availableEnemyList.Add(this);
        transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isEnemyKinematic)
        {
            return;
        }

        distanceFromTarget = Vector3.Distance(target.position, transform.position);
        hasGameStarted = GameController.instance.hasGameStarted;
        isGameOver = GameController.instance.isGameOver;

        if (distanceFromTarget < offscreenDistance && hasGameStarted)
            indicatorIcon.SetActive(true);
        else
            indicatorIcon.SetActive(false);

        if (distanceFromTarget < chaseDistance && !hasJumped && hasGameStarted)
        {
            navMesh.SetDestination(GameController.instance.playerTransform.position);

            // Player is near enemy , so chase him
            if (!isPlayerSpoted)
            {
                isPlayerSpoted = true;

                //Play Spotted Sound
                SoundManager.instance.PlayPlayerFoundByEnemy(transform);

                //Play BubbleSpeech
                bubbleSpeech.Play();
                GameController.instance.PlayPlayerBubble();
            }
        }
        else
        {
            //stop chasing
            if (isPlayerSpoted)
            {
                //   objAnim.SetBool("PlayerFound", false);
                isPlayerSpoted = false;
                //   rb.velocity = Vector3.zero;

                //Play BubbleSpeech
                bubbleSpeech.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                GameController.instance.StopPlayerBubble();
            }
            do
            {
                RandomPosition();
                distanceFromTarget = Vector3.Distance(targetPos, transform.position);
            } while (distanceFromTarget > 7f);

            navMesh.SetDestination(targetPos);
        }
    }

    void RandomValues()
    {
        if (isRandomJumpDistance) jumpDistance = Random.Range(randomJumpDistanceValues.x, randomJumpDistanceValues.y);
        if (isRandomSpeed) moveSpeed = Random.Range(randomMoveSpeed.x, randomMoveSpeed.y);
    }

    void RandomPosition()
    {
        targetPos = GameController.instance.RandomPosition();
    }

    public void KinematicsToggle(bool isKinematic)
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = isKinematic;
        }

        isEnemyKinematic = isKinematic;
    }

    private void OnEnable()
    {
        for (int i = 0; i < outlinesScript.Length; i++)
        {
            outlinesScript[i].enabled = true;
        }

        objAnim.SetBool("PlayerFound", true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Shield"))
        {
            rb.AddForce(transform.forward * -100f, ForceMode.VelocityChange);
            Debug.Log("shield spotted", collision.gameObject);
        }
    }
}
