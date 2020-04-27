using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_PlayerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExitGate")) {
            TutorialManager.instance.TutorialWin();
            TutorialManager.instance.HitWall();
            TutorialManager.instance.CreateParticleEffect(4, 0, other.transform.position);
            TutorialManager.instance.CreateParticleEffect(5, 0, other.transform.position);
            TutorialManager.instance.CreateParticleEffect(6, 0, other.transform.position);
            TutorialManager.instance.CreateParticleEffect(7, 0, other.transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !TutorialManager.instance.isTutorialWin) {
            TutorialManager.instance.CreateParticleEffect(3, 0, collision.contacts[0].point);
            TutorialManager.instance.HitWall();
        }
    }

}
