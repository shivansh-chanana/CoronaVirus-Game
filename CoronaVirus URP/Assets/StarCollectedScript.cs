using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCollectedScript : MonoBehaviour
{
    public RectTransform myRect;
    public RectTransform myRectChild;

    [Header("Serialize field")]
    public RectTransform targetPos;
    public bool isGoToTarget;

    private void Start()
    {
        isGoToTarget = false;
    }

    void Update()
    {
        if (isGoToTarget) {
            myRect.position = Vector3.Lerp(myRect.position, targetPos.position , 0.1f);
            myRectChild.sizeDelta = Vector2.Lerp(myRectChild.sizeDelta , new Vector2(100,100), 0.1f);
        }
    }
}
