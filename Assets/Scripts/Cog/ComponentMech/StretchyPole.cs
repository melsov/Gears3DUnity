using UnityEngine;
using System.Collections;
using System;

public class StretchyPole : MonoBehaviour {
    [SerializeField]
    protected Transform _extendToTarget;
    public Transform target {
        get { return _extendToTarget; }
        set {
            _extendToTarget = value;
            if (_extendToTarget) {
                doExtend = followTarget;
            } else {
                doExtend = doNothing;
            }
        }
    }
    [SerializeField]
    protected LineSegment lineSegment;

    protected delegate void DoExtend();
    protected DoExtend doExtend;

    public void Awake() {
        UnityEngine.Assertions.Assert.IsFalse(GetComponentInChildren<Rigidbody>(), "Probably a bad idea for StretchyPole to have a rigidbody");
        UnityEngine.Assertions.Assert.IsFalse(GetComponentInChildren<Collider>(), "Probably a bad idea for StretchyPole to have a collider");
        if (_extendToTarget) {
            doExtend = followTarget;
        } else {
            doExtend = doNothing;
        }
    }

    protected void followTarget() { extendTo(_extendToTarget.position); }
    protected void doNothing() { }

    public void extendTo(Vector3 global) {
        lineSegment.setEndPosition(global);
        align();
    } 

    private void align() {
        VectorXZ midPoint = lineSegment.midPoint();
        Quaternion q = Quaternion.LookRotation(lineSegment.distance.vector3());
        float scale = lineSegment.distance.magnitude;
        transform.position = TransformUtil.SetY(midPoint.vector3(), transform.position.y);
        transform.rotation = q;
        transform.localScale =  new Vector3(transform.localScale.x, transform.localScale.y, scale);
    }

    public void FixedUpdate() {
        doExtend();
    }
}
