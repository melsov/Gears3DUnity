using UnityEngine;
using System.Collections;

public class ElbowTube : Tube {

    protected override void awake() {
        base.awake();
    }

    protected override Vector3 down {
        get {
            return transform.rotation * (Vector3.forward * -1f);
        }
    }

    protected Vector3 intoEntrance {
        get { return entrance.transform.rotation * (Vector3.up * -1f); }
    }

    protected Vector3 outOfExit {
        get { return exit.transform.rotation * (Vector3.up * 1f); }
    }
    
    protected Vector3 centerToEntrance {
        get { return entrance.transform.position - transform.position; }
    }

    protected override void setVelocity(Rigidbody rb) {
        Vector3 rel = rb.transform.position - transform.position;
        float m = Vector3.Dot(rel.normalized, centerToEntrance.normalized);
        rb.velocity = Vector3.Lerp(outOfExit, intoEntrance, m * m) * strength;
    }
}
