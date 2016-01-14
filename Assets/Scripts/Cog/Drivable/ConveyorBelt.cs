using UnityEngine;
using System.Collections.Generic;
using System;

public class ConveyorBelt : Drivable , ICollisionProxyClient
{
    protected LineSegment lineSegment;
    protected float _radius;
    protected List<Collision> collisions;
    protected float wheelRotation;

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
            return wheelRotation * radius;
        }
    }

    protected override void awake() {
        base.awake();
        lineSegment = GetComponentInChildren<LineSegment>();
        collisions = new List<Collision>();
    }

    protected override void update() {
        base.update();
    }

    void FixedUpdate() {
        foreach (Collision coll in collisions) {
            coll.rigidbody.MovePosition(coll.transform.position + lineSegment.normalized.vector3() * beltSpeed);
        }

    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        // TODO: update appearance of the belt?
        // TODO: set up collider dropper and cursor agent. Test drivability of belt
        wheelRotation = drive.amount * -1f / radius;
        return Drive.Zero;
    }

    public void proxyCollisionEnter(Collision collision) {
        collision.rigidbody.velocity = Vector3.zero;
        collisions.Add(collision);
    }

    public void proxyCollisionStay(Collision collision) {
        //collision.rigidbody.AddForce(lineSegment.normalized.vector3() * .5f);
        //TODO: want simple move by instead
        //collision.rigidbody.MovePosition(collision.transform.position + lineSegment.normalized.vector3() * .2f);
    }

    public void proxyCollisionExit(Collision collision) {
        collisions.Remove(collision);
    }
}
