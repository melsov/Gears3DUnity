﻿using UnityEngine;
using System.Collections;

public class RackGear : Gear {
    protected LineSegment lineSegment;
    protected const float ToothBaseHeight = ToothDepth * 2.7f;

    protected VectorXZ basePosition;
    protected VectorXZ baseDirection;
    protected Transform anchor;

    protected float toothWidth { get { return toothOffset * 2f; } } //TODO: difference btwn toothWidth and toothOffest? why is there one?

    protected VectorXZ offset {
        get { return (xzPosition - basePosition); }
    }
    protected float offsetDirection {
        get {
            float r = offset.dot(new VectorXZ(transform.rotation * Vector3.right));
            if (r < 0f) return -1f;
            return 1f;
        }
    }

    protected void updateBasePosition() {
        basePosition = xzPosition;
    }
    protected override void updateAngleStep() {
        if (!isDriven()) { return; }
        
        _angleStep.update(offset.magnitude * offsetDirection);
    }
    public override bool isDriven() {
        if (base.isDriven()) { return true; }
        return anchor != null;
    }
    protected Gear drivingGear {
        get {
            if (_driver == null) return null;
            return (Gear)_driver;
        }
    }
    public override float innerRadius {
        get {
            return ToothBaseHeight;
        }
    }
    protected override void awake() {
        base.awake();
        lineSegment = GetComponent<LineSegment>();
    }

    protected LinearActuator findConnectedLinearActuator(Collider other) {
        Drivable d = other.GetComponentInParent<Drivable>();
        if (d == null) { return null; }
        return d.getDrivableParent<LinearActuator>();
    }

    protected VectorXZ direction {
        get {
            return new VectorXZ(transform.rotation * Vector3.right);
        }
    }

    protected void rotateInDirection(VectorXZ dir, Transform pivot) {
        if (!dir.sympatheticDirection(direction)) {
            dir *= -1f;
        }
        transform.rotation.SetLookRotation(dir.vector3());
    }

    protected override bool vConnectTo(Collider other) {
        if(base.vConnectTo(other)) {
            return true;
        }

        Socket aSocket;
        Peg peg = _pegboard.getBackendSocketSet().closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            if (RotationModeHelper.CompatibleModes(peg.pegIsParentRotationMode, aSocket.socketIsChildRotationMode)) {
                LinearActuator la = findConnectedLinearActuator(other);
                if (la == null) { print("la null"); return false; }
                print("Got LA");
                rotateInDirection(la.direction, peg.transform);
                setSocketToPeg(aSocket, peg);
                anchor = peg.transform;
                basePosition = xzPosition;
                return true;
            }
        }

        return false;
    }
    protected override void vDisconnect() {
        base.vDisconnect();
        anchor = null;
    }

    protected override bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        if (isConnectedTo(other.transform)) {
            return false;
        }
        if (!_pegboard.getBackendSocketSet().isConnected()) {
            return vConnectTo(other);
        }
        return false;
    }
    public override float driveScalar() {
        return _angleStep.deltaAngle; // / toothWidth;
    }

    public override Drive receiveDrive(Drive drive) {
        if (drivingGear != null) {
            Vector3 dir = transform.rotation * Vector3.right;
            float scalar = -drivingGear.tangentVelocity(); 
            transform.position += dir * scalar;
        }
        return drive;
    }

    public VectorXZ closestPointOnLine(VectorXZ p) {
        return lineSegment.closestPointOnLine(p);
    }
    public VectorXZ closestPointOnSetment(VectorXZ p) { return lineSegment.closestPointOnSegment(p); }

    protected override VectorXZ getConnectionPoint(Collider other) {
        return lineSegment.closestPointOnSegment(new VectorXZ(other.transform.position));
    }

    protected int closestToothOrdinal(VectorXZ other) {
        VectorXZ online = lineSegment.closestPointOnLine(other);
        VectorXZ dif = online - lineSegment.startXZ;
        int tooth;
        if (lineSegment.sympatheticDirection(dif)) {
            tooth = Mathf.FloorToInt(dif.magnitude / toothWidth);
        } else {
            tooth = Mathf.CeilToInt(dif.magnitude / toothWidth);
        }
        return tooth;
    }
    protected VectorXZ closestTooth(VectorXZ other) {
        return closestTooth(other, false);
    }
    protected VectorXZ closestTooth(VectorXZ other, bool wantVirtual) {
        VectorXZ online = lineSegment.closestPointOnLine(other);
        VectorXZ dif = online - lineSegment.startXZ;
        int tooth = closestToothOrdinal(other); 
        if (wantVirtual && !lineSegment.isOnSegment(online)) {
            if (lineSegment.sympatheticDirection(dif)) { //beyond line end
                tooth = toothCount;
            } else {
                tooth = 0;
            }
        }
        float toothDist = tooth * toothWidth;
        return lineSegment.startXZ + lineSegment.normalized * toothDist;
    }

    protected override void setDistanceFrom(Gear gear) {
        VectorXZ closest = lineSegment.closestPointOnLine(new VectorXZ(gear.transform.position));
        Vector3 pos = _driver.transform.position + (lineSegment.normal * -1f).vector3() * (innerRadius + gear.innerRadius + ToothDepth);
        transform.position += pos - closest.vector3(transform.position.y);
    }

    public override float proportionalCWToothOffsetFromAbsPosition(VectorXZ global) {
        VectorXZ virtualTooth = closestTooth(global, true);
        VectorXZ online = lineSegment.closestPointOnLine(global);
        VectorXZ dif = online - virtualTooth;
        return dif.magnitude / toothWidth;
    }

    public override void positionRelativeTo(Drivable _driver) {
        if (_driver != null) {
            if (!(_driver is Gear)) { base.positionRelativeTo(_driver); return; }
            Gear gear = (Gear)_driver;

            //set distance
            setDistanceFrom(gear);

            //nudge to engage other's teeth
            VectorXZ gearXZ = new VectorXZ(gear.transform.position);
            VectorXZ closest = lineSegment.closestPointOnLine(gearXZ);

            float normalizedOther = gear.normalizedCWToothOffsetFrom(closest - gearXZ);
            float nudge =  toothWidth * (normalizedOther + .5f);
            VectorXZ toothPos = closestTooth(gearXZ);
            VectorXZ n = closest - toothPos + lineSegment.normalized * nudge; 
            transform.position += n.vector3() ;
        }
    }
}