using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableScript : MonoBehaviour
{
    public int collectableNum;

    private void Start()
    {
        GetComponent<TargetIndigator>().m_targetIconOffScreen = GameController.instance.collectableSpr[collectableNum];
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Obstacle")) {
            Vector3 newPos = GameController.instance.RandomPosition();
            newPos.y = 1f;
            transform.position = newPos;
            //Debug.Log("<color=red> i was in obstacle : </color>" + other.name , other.gameObject);
        }
    }
}
