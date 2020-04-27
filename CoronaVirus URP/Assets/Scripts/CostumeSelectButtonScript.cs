using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeSelectButtonScript : MonoBehaviour
{
    public void ThisButtonSelected() {
        GameController.instance.CostumeSetButtonFromCostumeScreen(int.Parse(transform.name));
    }
}
