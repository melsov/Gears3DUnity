﻿using UnityEngine;
using System.Collections;

public class OneWayElbowTube : Tube {

    protected Transform center;

    protected override void awake() {
        base.awake();
        center = TransformUtil.FindChildWithName(transform, "Center");
    }

    protected override Vector3 down {
        get {
            return transform.rotation * EnvironmentSettings.gravityDirection;
        }
    }

    protected Vector3 intoEntrance {
        get { return entrance.transform.rotation * (Vector3.up * -1f); }
    }

    protected Vector3 outOfExit {
        get { return exit.transform.rotation * (Vector3.up * 1f); }
    }

    //protected float exitLeft {
    //    get {
    //        return isExitLeft ? 1f : -1f;
    //        //float sign = Mathf.Sign(transform.position.x - exit.transform.position.x);
    //        //if (Mathf.Abs(sign) < Mathf.Epsilon) { return 1f; }
    //        //return sign * -1f;
    //    }
    //}
    
    protected Vector3 centerToEntrance {
        get { return entrance.transform.position - center.position; } // transform.position; }
    }

    protected override void setVelocity(Rigidbody rb) {
        Vector3 rel = rb.transform.position - center.position; // transform.position;
        float m = Vector3.Dot(rel.normalized, centerToEntrance.normalized);
        rb.velocity = Vector3.Lerp(outOfExit, intoEntrance, m * m) * strength;
    }
}
