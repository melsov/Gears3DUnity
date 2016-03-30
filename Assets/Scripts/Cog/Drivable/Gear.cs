using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

public class Gear : Drivable  {

//TODO: plan out caps colider radius and assign programmatically
//TODO: related: make var: tooth depth
    public int toothCount = 6;
    public float toothOffset = .25f;
    public const float ToothDepth = .25f;
    protected float toothOffsetAngleRadians {
        get {
            return Mathf.PI * 2 / toothCount;
        }
    }
    protected float toothOffsetAngleDegrees { get { return toothOffsetAngleRadians * Mathf.Rad2Deg; } }

    protected float innerRadius {
        get { return toothOffset / Mathf.Sin(Mathf.PI / toothCount);  }
    }
    protected float outerRadius {
        get { return innerRadius + ToothDepth; }
    }

    protected override void awake() {
        base.awake();
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        cc.radius =  innerRadius; // + ToothDepth;
    }
    
    private void testOffset() {
        int end = 12;
        int howMany = toothCount * end;
        int i = 0;
        foreach(VectorXZ dir in Angles.UnitVectors(0f, Mathf.PI * 2f , howMany)) {
            float n = normalizedCWToothOffsetFrom(dir);
            VectorXZ v = Angles.UnitVectorAt(n * toothOffsetAngleRadians +  Mathf.Floor(i/((float)end)) * toothOffsetAngleRadians );
            BugLine.Instance.drawFromTo(transform.position + Vector3.up, transform.position + Vector3.up + (v * (innerRadius + ToothDepth * n)).vector3());
            i++;
        }
    }

    public virtual float driveRadius {
        get {
            return innerRadius * Mathf.Sin(toothOffsetAngleRadians);
            //return innerRadius + ToothDepth / 2f;
        }
    }
    protected override float radius {
        get {
            //return base.radius; // + ToothDepth /2f; 
            return driveRadius;
        }
    }

    public override float driveScalar() {
        return _angleStep.deltaAngle / toothOffsetAngleRadians;
    }

    public override Drive receiveDrive(Drive drive) {
        transform.eulerAngles += new Vector3(0f, drive.amount * -1f * toothOffsetAngleRadians, 0f);
        return drive;
    }

    public float rot { get { return transform.rotation.eulerAngles.y; } }

    protected float toothRotationOffsetDegrees { get { return Angles.FloatModSigned(rot, toothOffsetAngleDegrees); } }

    protected float normalizedCWToothOffsetFrom(VectorXZ rel) {
        float offset = cwToothOffsetFrom(rel);
        return offset / toothOffsetAngleDegrees;
    }

    protected float cwToothOffsetFrom(VectorXZ rel) {
        float clockAngle = positiveClockAngle(rel); 
        float ctacw = closestToothAngleCW(rel);
        return ctacw - clockAngle;
    }
    protected float positiveClockAngle(VectorXZ rel) {
        return Angles.PositiveAngleDegrees(Angles.VectorXZToDegrees(rel));
    }

    protected float closestToothAngleCW(VectorXZ rel) {
        float clockAngle = Angles.VectorXZToDegrees(rel);
        clockAngle += rot;
        clockAngle = Angles.PositiveAngleDegrees(clockAngle);
        float mod = Angles.FloatModSigned(clockAngle, toothOffsetAngleDegrees);
        float result = clockAngle - mod - rot;
        return result;
    }

    private void debugClockAngle(float clockAngle) {
        debugClockAngle(clockAngle, "clock angle: ");
    }
    private void debugClockAngle(float clockAngle, string msg) {
        print(msg + clockAngle);
        BugLine.Instance.drawFromTo(transform.position + Vector3.up, 
            transform.position + Vector3.up + (Angles.UnitVectorAt(clockAngle * Mathf.Deg2Rad) * (outerRadius * .95f)).vector3());
    }
    public override void positionRelativeTo(Drivable _driver) {
        if (_driver != null) {
            if (!(_driver is Gear)) { base.positionRelativeTo(_driver); return; }
            Gear gear = (Gear)_driver;

            //set distance
            Vector3 relPos = transform.position - _driver.transform.position;
            relPos = relPos.normalized * (innerRadius + gear.innerRadius + ToothDepth);
            transform.position = _driver.transform.position + relPos;

            //set rotation
            Vector3 euler = transform.rotation.eulerAngles;
            VectorXZ relXZ = new VectorXZ(relPos);
            float normalizedOther = gear.normalizedCWToothOffsetFrom(relXZ);
            float closestOffset = cwToothOffsetFrom(relXZ * -1f);
            euler.y = euler.y + closestOffset + toothOffsetAngleDegrees * (normalizedOther + .5f);
            transform.eulerAngles = euler; 
        }
    }

    protected override bool vConnectTo(Collider other) {
        if (isDriven()) {
            return false;
        }
        if (isConnectedTo(other.transform)) { return false; }

        // If this is an axel, get driven by it
        Axel axel = getAxel(other);
        if (axel != null) {
            setSocketClosestToAxel(axel);
            return true;
        }
        if (!isInConnectionRange(other)) {
            return false;
        }

        // Otherwise, if this is a gear, get driven by it
        Gear gear = other.GetComponent<Gear>(); 
        if (gear != null && gear is Drivable) {
            _driver = gear;
            gear.addDrivable(this);
            positionRelativeTo(gear);
            return true;
        }
        return false;
    }



    protected override bool connectToControllerAddOn(ControllerAddOn cao) {
        return false;
    }

    protected override bool connectToReceiverAddOn(ReceiverAddOn rao) {
        return false;
    }
}

