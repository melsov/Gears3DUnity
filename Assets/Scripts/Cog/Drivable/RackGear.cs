using UnityEngine;
using System.Collections;

public class RackGear : Gear {
    protected LineSegment lineSegment;
    protected const float ToothBaseHeight = ToothDepth * 2.7f;
    protected VectorXZ basePosition;
    protected float toothWidth { get { return toothOffset * 2f; } } //TODO: difference btwn toothWidth and toothOffest? why is there one?
    protected VectorXZ xzPosition {
        get { return new VectorXZ(transform.position); }
    }
    protected float offset {
        get { return (xzPosition - basePosition).magnitude; }
    }
    protected override void updateAngleStep() {
        if (!isDriven()) { return; }
        _angleStep.update(offset);
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
    public override float driveScalar() {
        return _angleStep.deltaAngle / toothWidth;
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
