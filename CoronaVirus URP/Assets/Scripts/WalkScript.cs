using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkScript : MonoBehaviour
{
    public HingeJoint bone;
    public Transform obj;
    public bool inverter;

    // Update is called once per frame
    void Update()
    {
        JointSpring Js = bone.spring;

        Js.targetPosition = obj.transform.localEulerAngles.x;

        if (Js.targetPosition > 180)
            Js.targetPosition = Js.targetPosition - 360;

        Js.targetPosition = Mathf.Clamp(Js.targetPosition , bone.limits.min + 5 , bone.limits.max - 5);

        if (inverter)
            Js.targetPosition = Js.targetPosition * -1f;

        bone.spring = Js;
    }
}
