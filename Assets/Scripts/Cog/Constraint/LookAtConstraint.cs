using UnityEngine;
using System.Collections;
using System;

public class LookAtConstraint : Constraint {

    public float testFactor = 1.2f;
    protected Vector3 prevTargetPosition = Vector3.zero;

    protected override void awake() {
        base.awake();
        prevTargetPosition = constraintTarget.reference.position;
    }

//CONSIDER: THIS SHOULD REALLY BE CALLED 'LINESEGMENT CONSTRAINT'
    protected override void constrain() {
        if (constraintTarget.target == null) {
            return;
        }
        Vector3 curDirection = constraintTarget.altReference.position - constraintTarget.reference.position; // transform.rotation * Vector3.forward);
        Vector3 target = constraintTarget.target.position;
        Vector3 nudge = constraintTarget.reference.position - prevTargetPosition; // constraintTarget.target.rotation.eulerAngles * testFactor; // Dot(constraintTarget.target.rotation.eulerAngles, curDirection.normalized) * .3f * constraintTarget.target.rotation.eulerAngles;
        if(nudge.sqrMagnitude != 0f) {
            VectorXZ n = new VectorXZ(nudge);
            float dot = constraintTarget.lineSegmentReference.normalized.dot(n) * testFactor;
            n = constraintTarget.lineSegmentReference.normalized * dot;
            target = target + n.vector3();
        }
        Vector3 nextDirection = target - constraintTarget.reference.position;
        transform.RotateAround(constraintTarget.reference.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(curDirection, nextDirection).eulerAngles.y);

        prevTargetPosition = constraintTarget.reference.position;
    }

    //protected override void constrain() {
    //    if (constraintTarget.target == null) {
    //        return;
    //    }
    //    Vector3 curDirection = constraintTarget.altReference.position - constraintTarget.reference.position; // transform.rotation * Vector3.forward);
    //    Vector3 nextDirection = constraintTarget.target.position - constraintTarget.reference.position;
    //    transform.RotateAround(constraintTarget.reference.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(curDirection, nextDirection).eulerAngles.y);
    //}

}
