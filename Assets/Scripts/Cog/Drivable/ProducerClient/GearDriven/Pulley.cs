using UnityEngine;
using System.Collections;
using System;

public class Pulley : GearDrivenMechanism , IPegProxy {

    protected DisappearingRope leftRope;
    protected DisappearingRope rightRope;
    protected float length;

    protected override void awake() {
        base.awake();
        foreach(DisappearingRope r in GetComponentsInChildren<DisappearingRope>()) {
            if (rightRope == null) {
                rightRope = r;
            } else {
                if (r.transform.localPosition.x > rightRope.transform.localPosition.x) {
                    leftRope = rightRope;
                    rightRope = r;
                } else {
                    leftRope = r;
                }
            }
        }
    }

    public void Start() {
        positionRopes();
    }

    private void positionRopes() {
        Vector3 first = leftRope.firstLink.transform.position;
        Vector3 last = leftRope.lastLink.transform.position;
        float zDif = first.z - last.z;
        leftRope.baseLink.MovePosition(new Vector3(leftRope.baseLink.position.x, leftRope.baseLink.position.y, transform.position.z + zDif / 2f));
        rightRope.baseLink.MovePosition(new Vector3(rightRope.baseLink.position.x, rightRope.baseLink.position.y, transform.position.z + zDif / 2f));
        length = zDif;
        PegboardGroup[] pegboards = _pegboard.GetComponentsInChildren<PegboardGroup>();
        Bug.assertPause(pegboards.Length == 2, "need exactly two pegboard groups");
        // get socket sets before they go to another parent
        _pegboard.getBackendSocketSet();
        _pegboard.getFrontendSocketSet();
        pegboards[0].GetComponent<Follower>().target = leftRope.lastLink.transform;
        pegboards[1].GetComponent<Follower>().target = rightRope.lastLink.transform;

    }

    protected override void updateMechanism(Drive drive) {
        float spin = angleStep.deltaAngle * radius * Mathf.Deg2Rad; // -rotationDeltaY(drive) * radius / toothCount;

        if ((((DisappearingHingeChainLink)leftRope.nextLastLink).hiding && -spin > 0f) || 
            (((DisappearingHingeChainLink)rightRope.nextLastLink).hiding && spin > 0f)) {
            return;
        }
        pull(rightRope, spin);
        pull(leftRope, -spin);
    }

    protected void pull(Rope rope, float amount) {
        Vector3 pos = rope.baseLink.position;
        pos.z += amount;
        rope.baseLink.MovePosition(pos);
    }

    public Guid getGuid() {
        throw new NotImplementedException();
    }

    public Peg getPeg() {
        throw new NotImplementedException();
    }

    public Pegboard getPegboard() {
        return _pegboard;
    }
}
