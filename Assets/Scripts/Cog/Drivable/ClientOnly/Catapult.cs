using System;
using System.Collections;
using UnityEngine;

//TODO: catapult extends Dispenser. dispenses catapult action
public class Catapult : Dispenser {

    [SerializeField]
    private float useMotorSeconds = 1.5f;
    [SerializeField]
    protected Transform lever;

    protected HingeJoint hj;
    protected Rigidbody hingeRB;
    protected override void awake() {
        base.awake();
        hj = GetComponentInChildren<HingeJoint>(); // GetComponentInChildren<FreeRotationPeg>().getHinge().getHingeJoint();
        hj.useSpring = true;
        JointSpring js = hj.spring;
        js.targetPosition = -10f;
        js.spring = 80f;
        hj.spring = js;
        hj.useLimits = true;
        JointLimits jl = hj.limits;
        jl.min = -10f;
        jl.max = 120f;
        hingeRB = hj.GetComponent<Rigidbody>();
        lever.position = TransformUtil.SetY(lever.position, YLayer.dispensable);
        StartCoroutine(testDi());
    }

    private IEnumerator testDi() {
        while(true) {
            yield return new WaitForSeconds(5f);
            dispense();
        }
    }

    protected override void dispense() {
        StartCoroutine(motorOnOff());
    }

    private IEnumerator motorOnOff() {
        hj.useMotor = true;
        yield return new WaitForSeconds(useMotorSeconds);
        hj.useMotor = false;
    }
}
