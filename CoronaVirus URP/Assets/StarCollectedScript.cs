using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCollectedScript : MonoBehaviour
{

    [Header("Serialize field")]
    public RectTransform targetPos;
    public bool isGoToTarget;

    RectTransform myRect;

    private void Start()
    {
        isGoToTarget = false;
    }

    void Update()
    {
        if (isGoToTarget) {
            myRect.position = Vector3.MoveTowards(myRect.position, targetPos.position , 0.2f);
        }
    }
}
