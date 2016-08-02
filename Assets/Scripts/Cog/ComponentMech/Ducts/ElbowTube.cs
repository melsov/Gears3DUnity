using UnityEngine;
using System.Collections;

public class ElbowTube : Tube {

    protected Transform center;
    protected Transform elbowBase;

    protected override void awake() {
        base.awake();
        center = TransformUtil.FindChildWithName(transform, "Center");
        elbowBase = TransformUtil.FindChildWithName(transform, "ElbowLocation");
        width = entrance.GetComponent<CapsuleCollider>().radius * 2f;
    }

    protected Vector3 intoEntrance {
        get { return entrance.transform.rotation * (Vector3.up * -1f); }
    }

    protected Vector3 outOfExit {
        get { return exit.transform.rotation * (Vector3.up * 1f); }
    }
    
    protected Vector3 centerToEntrance {
        get { return entrance.transform.position - center.position; }
    }
    protected Vector3 centerToExit {
        get { return exit.transform.position - center.position; }
    }

    protected override Vector3 isEntering(Transform t) {
        Vector3 towards, exitward, entrPos;
        if (closerToEntrance(t)) {
            entrPos = entrance.position;
            towards = entrance.position - t.position;
            exitward = intoEntrance;
        } else {
            entrPos = exit.position;
            towards = exit.position - t.position;
            exitward = outOfExit * -1f;
        }
        if(towards.sqrMagnitude > .1f && Vector3.Dot(exitward, towards) > 0f) {
            Vector3 targetPos = entrPos + (elbowBase.position - entrPos) * .2f;
            return targetPos - t.position;
        }
        return Vector3.zero;
    }

    protected override bool movingTowardsExit(Rigidbody rb) {
        Vector3 entranceToExit = exit.transform.position - entrance.transform.position;
        return Vector3.Dot(rb.velocity, entranceToExit) > 0f;
    }

    protected override void setVelocity(Rigidbody rb) {
        if (movingTowardsExit(rb)) {
            setVelocity(rb, centerToEntrance, intoEntrance, outOfExit);
        } else {
            setVelocity(rb, centerToExit, outOfExit * -1f, intoEntrance * -1f);
        }
    }

    protected void setVelocity(Rigidbody rb, Vector3 centerTo, Vector3 entering, Vector3 exiting) {
        TESTROCK(rb, Color.cyan);
        Vector3 rel = rb.transform.position - center.position;
        float m = Vector3.Dot(rel.normalized, centerTo.normalized);
        rb.velocity = Vector3.Lerp(exiting, entering, m) * strength;
    }

    
}
