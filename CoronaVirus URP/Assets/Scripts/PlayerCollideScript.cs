using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollideScript : MonoBehaviour
{
    public Rigidbody rb;

    public GameObject[] trails;
    public GameObject smoke;

    [SerializeField] Rigidbody[] rigidbodies;
    [SerializeField] GameObject airCamera;
    [SerializeField] bool areTrailsEnabled, isSmokeEnabled, isAirCameraEnabled;

    bool isTouchedEnemy;

    private void Start()
    {
        isTouchedEnemy = false;

        rigidbodies = GetComponentsInChildren<Rigidbody>();
        airCamera = GameController.instance.mainCam.transform.GetChild(0).gameObject;

        ToggleEffects(false);
    }

    private void Update()
    {
        if (rb.velocity.magnitude > 6 && !areTrailsEnabled) {
            for (int i = 0; i < trails.Length; i++)
            {
                trails[0].SetActive(true);
                trails[1].SetActive(true);
            }
            areTrailsEnabled = true;
        }

        if (rb.velocity.magnitude > 7f && !isSmokeEnabled)
        {
            smoke.SetActive(true);
            isSmokeEnabled = true;
        }

        if (rb.velocity.magnitude > 8f && !isAirCameraEnabled)
        {
            airCamera.SetActive(true);
            isAirCameraEnabled = true;
        }

        if (Input.GetKeyDown(KeyCode.A)) ExitGateCollision();
        if (Input.GetKeyDown(KeyCode.S)) GameController.instance.GameRestartButton();
        if (Input.GetKeyDown(KeyCode.D)) GameController.instance.StarCollected();
    }

    void ToggleEffects(bool isEnable) {
        for (int i = 0; i < trails.Length; i++)
        {
            trails[0].SetActive(isEnable);
            trails[1].SetActive(isEnable);
        }
        areTrailsEnabled = isEnable;

        smoke.SetActive(isEnable);
        isSmokeEnabled = isEnable;

        airCamera.SetActive(isEnable);
        isAirCameraEnabled = isEnable;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable")) {
            other.gameObject.SetActive(false);
            GameController.instance.StarCollected();
            GameController.instance.CollectableCollected();
            GameController.instance.CreateParticleEffect(0, 0  ,  other.transform.position);
            GameController.instance.CreateParticleEffect(4, 0.5f, other.transform.position);
        }

        if (other.CompareTag("ExitGate")) {
            ExitGateCollision();
            GameController.instance.CreateParticleEffect(4, 0, other.transform.position);
            GameController.instance.CreateParticleEffect(5, 0, other.transform.position);
            GameController.instance.CreateParticleEffect(6, 0, other.transform.position);
            GameController.instance.CreateParticleEffect(7, 0, other.transform.position);

            //Stop fast moving particles
            GameController.instance.HitWall();
            ToggleEffects(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle")) {
            if (collision.relativeVelocity.magnitude > 6f) {
                GameController.instance.CreateParticleEffect(3, 0,collision.contacts[0].point);
            }

            GameController.instance.HitWall();
            ToggleEffects(false);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy_1") && !isTouchedEnemy) {
            GameController.instance.joystick.gameObject.SetActive(false);
            KinematicsToggle(true);
            GameController.instance.isGameOver = true;
            GameController.instance.ToggleEnemyKinematic(true);
            GameController.instance.GameOverAnimations();
            GameController.instance.CreateParticleEffect(1,0,transform.position + new Vector3(0, 4f , -3f));
            GameController.instance.CreateParticleEffect(2,1,collision.contacts[0].point);

            isTouchedEnemy = true;
        }
    }

    public void KinematicsToggle(bool isKinematic)
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = isKinematic;
        }
    }

    public void ExitGateCollision() {
        GameController.instance.joystick.gameObject.SetActive(false);
        GameController.instance.ToggleEnemyKinematic(true);
        GameController.instance.StarCollected();
        KinematicsToggle(true);
        GameController.instance.isGameWin = true;
        GameController.instance.GameWinFunction(2f);
    }
}
