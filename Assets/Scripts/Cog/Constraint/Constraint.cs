﻿using UnityEngine;
using System.Collections;

public abstract class Constraint : MonoBehaviour {

    public ConstraintTarget constraintTarget;
    protected Rigidbody rb;
    public bool isParentConstraint;
    public Transform systemParent;

    void Awake() {
        awake();
    }
    protected virtual void awake() {
        rb = GetComponent<Rigidbody>();
        rb.drag = 10f;
    }

    void FixedUpdate() {
        fixedUpdate();
    }
    protected void fixedUpdate() {
        if (isParentConstraint) {
            return;
        }
        if (constraintTarget.parentConstraint != null) {
            constraintTarget.parentConstraint.constrain();
        }
        constrain();
    }

    public virtual void removeTarget() {
        constraintTarget.target = null;
    }

    protected abstract void constrain();

}

[System.Serializable]
public struct ConstraintTarget
{
    public Transform target;
    public Transform reference;
    public Transform altReference;
    public Constraint parentConstraint;

    public ConstraintTarget(Transform _target, Transform _reference) {
        target = _target; reference = _reference; parentConstraint = null; altReference = null;
    }
    public ConstraintTarget (Transform _target) {
        target = _target; reference = null; parentConstraint = null; altReference = null;
    }

    public bool isPsuedoNull() {
        return target == null;
    }
}