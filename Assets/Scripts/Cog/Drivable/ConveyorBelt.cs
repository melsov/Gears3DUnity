using UnityEngine;
using System.Collections.Generic;
using System;

public class ConveyorBelt : Drivable , ICollisionProxyClient
{
    protected LineSegment lineSegment;
    protected float _radius;
    protected List<Collision> collisions;
    protected AngleStep wheelRotation;
    public float speedMultiplier = 10f;
    private float damper = 1000f;

    protected RotationIndicator[] rotationIndicators;

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
            return wheelRotation.deltaAngle * radius * speedMultiplier / damper;
        }
    }

    protected override void awake() {
        base.awake();
        lineSegment = GetComponentInChildren<LineSegment>();
        collisions = new List<Collision>();
        rotationIndicators = GetComponentsInChildren<RotationIndicator>();
        if (rotationIndicators == null) { rotationIndicators = new RotationIndicator[0]; }
    }

    protected override void update() {
        base.update();
    }

    void FixedUpdate() {
        for (int i = 0; i < collisions.Count; ++i) {
            Collision coll = collisions[i];
            if (coll == null || coll.rigidbody == null) {
                collisions.Remove(coll);
                --i;
                continue;
            }
            coll.rigidbody.MovePosition(coll.transform.position + lineSegment.normalized.vector3() * beltSpeed);
        }
    }


    //protected override bool vConnectTo(Collider other) {
    //    if (isDriven()) { 
    //        return false;
    //    }
    //    if (isConnectedTo(other.transform)) { return false; }

    //    Gear gear = other.GetComponent<Gear>();
    //    if (gear != null && gear is Drivable) {
    //        _driver = gear;
    //        gear.addDrivable(this);
    //        positionRelativeTo(gear);
    //        return true;
    //    }
    //    return false;
    //}

    protected bool makeConveryBeltConnection(DrivableConnection dc) {
        Gear gear = dc.other.GetComponent<Gear>();
        if (gear != null) {
            _driver = gear;
            gear.addDrivable(this);
            positionRelativeTo(gear);
            return true;
        }
        return false;
    }
    protected override DrivableConnection getDrivableConnection(Collider other) {
        DrivableConnection dc = new DrivableConnection(this);
        if (isDriven() || isConnectedTo(other.transform)) { return dc; }
        if (other.GetComponent<Gear>() != null) {
            dc.other = other;
            dc.makeConnection = makeConveryBeltConnection;
        }
        return dc;
    }

    public override void positionRelativeTo(Drivable _driver) {
        if (_driver is Gear) {
            Collider track = null;
            foreach (Collider co in GetComponentsInChildren<Collider>()) {
                if (co.name.Equals("Track")) {
                    track = co;
                    break;
                }
            }
            if (track == null) return;
            //TODO: decide how to align conveyor belts
        }
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        wheelRotation.update(wheelRotation.getAngle() + drive.amount * -1f / radius);
        foreach (RotationIndicator ri in rotationIndicators) {
            ri.rotation = wheelRotation.getAngle();
        }
        return Drive.Zero;
    }

    public void proxyCollisionEnter(Collision collision) {
        collision.rigidbody.velocity = Vector3.zero;
        collision.rigidbody.useGravity = false;
        collisions.Add(collision);
    }

    public void proxyCollisionStay(Collision collision) {
    }

    public void proxyCollisionExit(Collision collision) {
        if (collision != null) {
            collision.rigidbody.useGravity = true;
        }
        collisions.Remove(collision);
    }
}
