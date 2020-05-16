using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    [SerializeField] Material shieldMat;

    [Space]
    [SerializeField] float shieldTimer;
    [SerializeField] bool isWarningTimerStarted;
    [SerializeField] bool turnToBlackNow = true;
    [SerializeField] Color blackTemp;
    [SerializeField] Color colorTemp;


    // Start is called before the first frame update
    void Start()
    {
        ResetMe();
    }

    // Update is called once per frame
    void Update()
    {
        if(isWarningTimerStarted)WarningEffect();
    }

    private void WarningEffect()
    {
        if (turnToBlackNow)
        {
            shieldMat.color = Color.Lerp(shieldMat.color, blackTemp, 0.6f);
            if (shieldMat.color == blackTemp)
            {
                turnToBlackNow = false;
            }
        }
        else
        {
            shieldMat.color = Color.Lerp(shieldMat.color, colorTemp, 0.6f);
            if (shieldMat.color == colorTemp)
            {
                turnToBlackNow = true;
            }
        }
    }

    public void StartTimer() {
        StartCoroutine(StartDisableTimer());
    }

    IEnumerator StartDisableTimer() {
        shieldTimer = GameController.instance.totalFillbarTime/3f;
        yield return new WaitForSeconds(shieldTimer - shieldTimer/4);
        isWarningTimerStarted = true;
        yield return new WaitForSeconds(shieldTimer/4);
        GameController.instance.isShieldOn = false;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ResetMe();
    }

    void ResetMe() {
        shieldMat.color = colorTemp;
        isWarningTimerStarted = false;
        turnToBlackNow = true;
    }
}
