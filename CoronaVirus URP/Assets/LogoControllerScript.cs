using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class LogoControllerScript : MonoBehaviour
{
    bool isTutorialComplete;
    public List<string> LevelNames;
    public float LogoDelay;
    public Animator anim;

    private void Awake()
    {
        //Initialize Game Analytics
        GameAnalytics.Initialize(); 
    }

    void Start()
    {
        isTutorialComplete = PlayerPrefsX.GetBool("isTutorialComplete" , false);
        Invoke("EndLogAnim" , LogoDelay);
    }

    IEnumerator LoadNextScene(float timer) {
        yield return new WaitForSeconds(timer);

        if (isTutorialComplete)
        {
            int currentLevel = 0;
            SceneManager.LoadScene(LevelNames[currentLevel]);
        }
        else {
            SceneManager.LoadScene("TutorialScene");
        }
    }

    void EndLogAnim() {
        anim.Play("LogoEnd");
    }

    public void EndLogoTransition() {
        StartCoroutine(LoadNextScene(0f));
    }

}
