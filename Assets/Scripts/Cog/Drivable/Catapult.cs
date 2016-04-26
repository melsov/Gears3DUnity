using UnityEngine;
using System.Collections;

public class Catapult : Cog {
    public void Awake() {
        HingeJoint hj = GetComponentInChildren<FreeRotationPeg>().getHinge().getHingeJoint();
        hj.useSpring = true;
        JointSpring js = hj.spring;
        js.targetPosition = 15f;
        js.spring = 80f;
        hj.spring = js;
    }
}
