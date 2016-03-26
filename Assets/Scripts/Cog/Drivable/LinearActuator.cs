using UnityEngine;
using System.Collections;
using System;

public class LinearActuator : Drivable , IPegProxy {

    // TODO: resolve LA's origin weirdness
    // TOOD: make LA's slider length settable

    protected LineSegment lineSegment;
    protected HandleSet handleSet;

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

    public virtual Pole drivingPole {
        get {
            if (drivingPeg.parent == null) return null;
            return drivingPeg.parent.parentContainer.getTransform().GetComponentInParent<Pole>();
        }
    }

    public bool hasDrivingPole {
        get { return drivingPole != null; }
    }

    protected LinearActuatorConstraint linearActuatorConstraint {
        get {
            if (drivingPeg == null) return null;
            if (drivingPeg.isChildConstraint == null) return null;
            if (drivingPeg.isChildConstraint.constraintTarget.parentConstraint == null) return null;
            return drivingPeg.isChildConstraint.constraintTarget.parentConstraint.GetComponent<LinearActuatorConstraint>();
        }
    }
    

    protected override bool vConnectTo(Collider other) {
        print("LA connect");
        Pole pole = other.GetComponent<Pole>();
        if (pole != null && drivingPole != null && pole != drivingPole) {
            print("have a pole already");
            return false;
        }

        print("got something: " + other.name);
        // if other is a pole. 
        // that has a free socket 

        if (pole != null) {
            return pole.acceptBackendPegOnDrivable(this);
        }
        return false;
    }

    protected override void vOnDragEnd() {
        checkConstraint();
    }

    protected override void vDisconnect() {
        LinearActuatorConstraint lac = linearActuatorConstraint;
        if(lac != null) {
            if (!lac.isTargetInRange()) {
                base.vDisconnect(); //CONSIDER: has no meaning for linear actuators?
            }
        }
    }

    protected override void vEndDragOverride(VectorXZ cursorGlobal) {
        checkConstraint();
    }

    protected void checkConstraint() {
        LinearActuatorConstraint lac = linearActuatorConstraint;
        if (lac != null) {
            if (lac.isTargetInRange(true)) {
                lac.configure();
            } else {
                removeParentConstraintAndConstraintTargets();
                base.vDisconnect();
            }
        }
    }

    private void removeParentConstraintAndConstraintTargets() {
        drivingPole.removeLinearActuatorConstraint();
        drivingPeg.isChildConstraint.constraintTarget.parentConstraint = null;
        drivingPeg.isChildConstraint.removeTarget();
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

    public void extendToAccommodate(VectorXZ p) {
        lineSegment.extendToAccommodate(p);
    }

}

public interface IPegProxy
{
    Guid getGuid();
    Peg getPeg();
}
