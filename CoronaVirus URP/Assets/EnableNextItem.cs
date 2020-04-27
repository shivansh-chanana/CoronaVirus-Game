using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableNextItem : MonoBehaviour
{
    public GameObject[] nextItem;

    public GameObject[] disableTheseToo;

    public bool enableNextTask;

    private void Start()
    {
        ToggleNextItem(false); ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {

            //Save player currentPos
            TutorialManager.instance.posCheckPoint = other.transform.position;

            if (enableNextTask) {
                //Disable title
                for (int i = 0; i < disableTheseToo.Length; i++)
                {
                    disableTheseToo[i].SetActive(false);
                }
            }

            ToggleNextItem(true);

            TutorialManager.instance.CreateParticleEffect(0, 0, other.transform.position);
            TutorialManager.instance.CreateParticleEffect(4, 0.5f, other.transform.position);

            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ToggleNextItem(false);
    }

    void ToggleNextItem(bool enable) {
        for (int i = 0; i < nextItem.Length; i++)
        {
            nextItem[i].gameObject.SetActive(enable);
        }
    }

}
