using UnityEngine;
using System.Collections;

public class LinearConstraint : Constraint {

    public LineSegment lineSegment;
	
	// Called from fixed update
	protected override void constrain () {
        Vector3 position;
        if (constraintTarget.target == null) {
            position = transform.position;
        } else {
            position = constraintTarget.target.position;
        }
        VectorXZ closestPoint = lineSegment.closestPointOnSegment(new VectorXZ(position));
        rb.MovePosition(closestPoint.vector3(transform.position.y));
    }
}
