using UnityEngine;
using System.Collections;
using System;

public class Piston : GearDrivenMechanism {

    //protected Transform gearMesh;
    protected Transform shaft;
    protected Transform pistonHead;
    protected LineSegment lineSegment;

    [SerializeField]
    protected float headSpeed = .1f;
    private float progressor = 0f;

    protected override void awake() {
        base.awake();
        foreach(Transform t in GetComponentsInChildren<Transform>()) {
            if (t.name.Equals("Shaft")) {
                shaft = t;
            } 
        }
        pistonHead = GetComponentInChildren<Pegboard>().transform;
        lineSegment = GetComponentInChildren<LineSegment>();
    }

    public VectorXZ direction {
        get { return lineSegment.distance.normalized; }
    }

    protected override void updateMechanism(Drive drive) {
        updatePistonHead(drive);
    }
    protected void updatePistonHead(Drive drive) {
        progressor += drive.amount * headSpeed;
        if (float.MaxValue - progressor < 100f) { progressor = 0f; }
        float linearPos = lineSegment.distance.magnitude * (1f + Mathf.Sin(progressor)) / 2f;
        pistonHead.position = lineSegment.start.position + (lineSegment.normalized * linearPos).vector3();
    }
    protected float startToHeadSquared {
        get { return new VectorXZ(lineSegment.start.position - pistonHead.position).magnitudeSquared; }
    }

    protected float normalizedPistonHeadPosition() {
        return startToHeadSquared / lineSegment.distance.magnitudeSquared;
    }

    protected override Transform dragOverrideTarget {
        get {
            return shaft.transform;
        }
    }

}
