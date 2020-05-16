using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tutorial_EnemyScript : MonoBehaviour
{
    public Animator objAnim;

    Rigidbody rb;
    NavMeshAgent navMesh;

    Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        navMesh = GetComponent<NavMeshAgent>();
        objAnim.SetBool("PlayerFound", true);
    }

    private void Update()
    {
        navMesh.SetDestination(TutorialManager.instance.playerTransform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player")) {
            TutorialManager.instance.playerTransform.position = TutorialManager.instance.posCheckPoint;

            TutorialManager.instance.AvoidPeopleHighlight(5f);
            TutorialManager.instance.HitWall();
            ResetEnemyPosition();
        }
    }

    public void ResetEnemyPosition() {
        transform.position = startPos;
    }

    public void StopEnemies() {
        navMesh.speed = 0;
    }
}
