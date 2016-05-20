using UnityEngine;
using System.Collections;
 
public class LinearSpringConstraint : LinearConstraint {

    public bool endIsHome = true;

    protected Vector3 home {
        get { return endIsHome ? lineSegment.end.position : lineSegment.start.position; }
    }

    protected override void constrain() {
        base.constrain();
        rb.MovePosition(Vector3.Lerp(rb.position, home, .5f));
    }
}
