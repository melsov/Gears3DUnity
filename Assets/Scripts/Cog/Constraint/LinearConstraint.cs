using UnityEngine;
using System.Collections;

public class LinearConstraint : Constraint {

    public LineSegment lineSegment;

	public bool endIsHome = true;
    protected Vector3 home {
        get { return endIsHome ? lineSegment.end.position : lineSegment.start.position; }
    }
    protected Vector3 extent {
        get { return endIsHome ? lineSegment.start.position : lineSegment.end.position; }
    }

    public float normalizedPosition {
        get { return (home - rb.position).sqrMagnitude / (home - extent).sqrMagnitude; }
    }

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
