using UnityEngine;
using System.Collections;

public class LinearConstraint : Constraint {

    public LineSegment lineSegment;
	
    protected Vector3 targetPosition {
        get {
            if (constraintTarget.target == null) {
                return transform.position;
            } else {
                return constraintTarget.target.position;
            }
        }
    }
	// Called from fixed update
	protected override void constrain () {
        rb.MovePosition(lineSegment.closestPointOnSegment(new VectorXZ(targetPosition)).vector3(transform.position.y));
    }
}
