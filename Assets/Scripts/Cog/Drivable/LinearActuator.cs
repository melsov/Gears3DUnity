using UnityEngine;
using System.Collections;
using System;

public class LinearActuator : Drivable , IPegProxy {

    // TODO: resolve LA's origin weirdness
    // TOOD: make LA's slider length settable

    protected LineSegment lineSegment;

    protected override void awake() {
        base.awake();
        lineSegment = GetComponentInChildren<LineSegment>();
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    // Linear Actuator never does the connecting itself?
    protected override bool vConnectTo(Collider other) {
        if (drivingPeg.hasParentSocket) { return false; }
        Pole pole = other.GetComponent<Pole>();
        if (pole != null) {
            return pole.acceptBackendPegOnDrivable(this);
        }
        // if other is a pole. 
        // that has a free 
        return false;
    }

    public Guid getGuid() {
        return GetComponent<Guid>();
    }

    public Peg getPeg() {
        return GetComponentInChildren<Peg>();
    }

    protected Peg drivingPeg {
        get {
            return _pegboard.getBackendSocketSet().sockets[0].drivingPeg;
        }
    }

}

public interface IPegProxy
{
    Guid getGuid();
    Peg getPeg();
}
