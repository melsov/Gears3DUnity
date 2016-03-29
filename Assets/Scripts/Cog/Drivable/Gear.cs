using UnityEngine;
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
            float n = normalizedToothRotationOffsetFrom(dir);
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
        //return _angleStep.deltaAngle * radius;
        return _angleStep.deltaAngle / toothOffsetAngleRadians;
    }

    public override Drive receiveDrive(Drive drive) {
        transform.eulerAngles += new Vector3(0f, drive.amount * -1f * toothOffsetAngleRadians, 0f);
        //transform.eulerAngles += new Vector3(0f, drive.amount * -1f / radius, 0f);
        return drive;
    }

    public float rot { get { return transform.rotation.eulerAngles.y; } }

    protected float toothRotationOffsetDegrees { get { return Angles.FloatModSigned(rot, toothOffsetAngleDegrees); } }

    protected float normalizedToothRotationOffsetFrom(VectorXZ rel) {
        //TODO: draw some lines
        //TEST: on same tooth gears
        //TEST: on only positive rel vecs
        float clockAngle = Angles.VectorXZToDegrees(rel);
        clockAngle = Angles.PositiveAngleDegrees(clockAngle);
        clockAngle += Mathf.PI / 2f;
        clockAngle -= Angles.PositiveAngleDegrees(rot); // toothRotationOffsetDegrees;
        float mod = Angles.FloatModSigned(clockAngle, toothOffsetAngleDegrees);
        if (mod < 0f) {
            mod += toothOffsetAngleDegrees;
        }
        return mod / toothOffsetAngleDegrees;
    }

    public override void positionRelativeTo(Drivable _driver) {

        if (_driver != null) {
            if (!(_driver is Gear)) { base.positionRelativeTo(_driver); return; }

            Vector3 relPos = transform.position - _driver.transform.position;
            relPos = relPos.normalized * (innerRadius + ((Gear)_driver).innerRadius + ToothDepth);
            transform.position = _driver.transform.position + relPos;

            Gear gear = (Gear)_driver;

            Vector3 euler = transform.rotation.eulerAngles;
            float normalizedOther = gear.normalizedToothRotationOffsetFrom(new VectorXZ(-relPos));
            euler.y = toothOffsetAngleDegrees * (normalizedOther + .5f);
            transform.eulerAngles = euler;
            //transform.rotation = Quaternion.Euler(euler);

        }
    }

    protected override bool vConnectTo(Collider other) {
        if (isDriven()) { //TODO: aren't we guaranteed not to be connected at this point?
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
        // If this is a gear, get driven by it
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

