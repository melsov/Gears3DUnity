using UnityEngine;
using System.Collections;
using System;

public class LookAtConstraint : Constraint {

    public float testFactor = -3f;

    protected override void constrain() {
        if (constraintTarget.target == null) {
            return;
        }
        Vector3 curDirection = constraintTarget.altReference.position - constraintTarget.reference.position; // transform.rotation * Vector3.forward);
        Vector3 nextDirection = constraintTarget.target.position - constraintTarget.reference.position;
        transform.RotateAround(constraintTarget.reference.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(curDirection, nextDirection).eulerAngles.y);
    }

}
