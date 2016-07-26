using UnityEngine;
using System.Collections;
using System;

public class Piston : GearDrivenMechanism , LinearDrive
{

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
        UnityEngine.Assertions.Assert.IsFalse(shaft == null, "A piston doesn't have a shaft; what gives? (Shaft must be named 'Shaft')");
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

    public Quaternion linearDriveRotation() {
        return shaft.transform.rotation;
    }

    public Vector3 linearDriveEuler() {
        return shaft.transform.rotation.eulerAngles;
    }

    protected override Transform dragOverrideTarget {
        get {
            return shaft.transform;
        }
    }

}

public interface LinearDrive
{
    Quaternion linearDriveRotation();
    Vector3 linearDriveEuler();
}