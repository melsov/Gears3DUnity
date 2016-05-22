using UnityEngine;
using System.Collections;
 
public class LinearSpringConstraint : LinearConstraint {

    protected int pulseIncrements;
    protected int increments = 5;

    public void pulse() {
        pulseIncrements = increments;
    }

    protected override void constrain() {
        base.constrain();
        if (pulseIncrements-- > 0) {
            rb.MovePosition(Vector3.Lerp(rb.position, extent, .5f));
            return;
        }
        rb.MovePosition(Vector3.Lerp(rb.position, home, .5f));
    }
}
