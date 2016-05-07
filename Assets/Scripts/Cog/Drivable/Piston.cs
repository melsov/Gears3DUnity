using UnityEngine;
using System.Collections;

public class Piston : Gear {

    protected Transform gearMesh;
    protected Transform shaft;
    protected Transform pistonHead;
    protected LineSegment lineSegment;

    [SerializeField]
    protected float headSpeed = .1f;
    //drive translates into shaft movement
    //rotation on connect applies only to gear

    protected override void awake() {
        base.awake();
        foreach(Transform t in GetComponentsInChildren<Transform>()) {
            if (t.name.Equals("Shaft")) {
                shaft = t;
            } else if (t.name.Equals("GearMesh")) {
                gearMesh = t;
            }
        }
        pistonHead = GetComponentInChildren<Pegboard>().transform;
        lineSegment = GetComponentInChildren<LineSegment>();
    }

    public override Drive receiveDrive(Drive drive) {
        Drive baseDrive = base.receiveDrive(drive);
        updatePistonHead(baseDrive);
        return baseDrive;
    }
    private float progressor = 0f;
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

    protected override Vector3 startEulerAnglesForAligningTeeth {
        get {
            return gearMesh.rotation.eulerAngles;
        }
    }
    protected override Transform gearTransform {
        get {
            return gearMesh.transform;
        }
    }

    protected override Transform dragOverrideTarget {
        get {
            return shaft.transform;
        }
    }

}
