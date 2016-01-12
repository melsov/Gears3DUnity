using UnityEngine;
using System.Collections;
using System;

public class ConveyorBelt : Drivable , ICollisionProxyClient
{
    protected LineSegment lineSegment;
    protected float _radius;
    protected override float radius {
        get {
            if (_radius == 0f) {
                _radius = GetComponentInChildren<CollisionProxy>().transform.localScale.z / 2f;
            }
            return _radius;
        }
    }

    protected float beltSpeed {
        get {
            return angleStep.deltaAngle * radius;
        }
    }

    protected override void awake() {
        base.awake();
        lineSegment = GetComponentInChildren<LineSegment>();
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
// TODO: update appearance of the belt?
        return Drive.Zero;
    }

    public void proxyCollisionEnter(Collision collision) {
        collision.rigidbody.velocity = Vector3.zero;
        
    }

    public void proxyCollisionStay(Collision collision) {
        //collision.rigidbody.AddForce(lineSegment.normalized.vector3() * .5f);
        //TODO: want simple move by instead
    }

    public void proxyCollisionExit(Collision collision) {
        print("c exit");
    }
}
