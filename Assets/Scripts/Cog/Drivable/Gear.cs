using UnityEngine;
//using UnityEditor;
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

    public virtual float innerRadius {
        get { return toothOffset / Mathf.Sin(Mathf.PI / toothCount);  }
    }
    public float outerRadius {
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
            float n = proportionalCWToothOffsetFrom(dir);
            VectorXZ v = Angles.UnitVectorAt(n * toothOffsetAngleRadians +  Mathf.Floor(i/((float)end)) * toothOffsetAngleRadians );
            BugLine.Instance.drawFromTo(transform.position + Vector3.up, transform.position + Vector3.up + (v * (innerRadius + ToothDepth * n)).vector3());
            i++;
        }
    }

    public virtual float driveRadius {
        get {
            return innerRadius + ToothDepth / 2f;
        }
    }
    protected override float radius {
        get {
            //return base.radius; // + ToothDepth /2f;
            return innerRadius * Mathf.Sin(toothOffsetAngleRadians);
        }
    }

    public float tangentVelocity() {
        return _angleStep.deltaAngle * Mathf.Deg2Rad * innerRadius;
    }

    public override float driveScalar() {
        return _angleStep.deltaAngle / toothOffsetAngleRadians;
    }

    public override Drive receiveDrive(Drive drive) {
        if (_driver is RackGear) {
            transform.eulerAngles += new Vector3(0f, -Mathf.Rad2Deg * drive.amount / innerRadius, 0f);
        } else {
            transform.eulerAngles += new Vector3(0f, drive.amount * -1f * toothOffsetAngleRadians, 0f);
        }
        return drive;
    }

    public float rot { get { return transform.rotation.eulerAngles.y; } }

    protected float toothRotationOffsetDegrees { get { return Angles.FloatModSigned(rot, toothOffsetAngleDegrees); } }

    public virtual float proportionalCWToothOffsetFromAbsPosition(VectorXZ global) {
        return proportionalCWToothOffsetFrom(global - xzPosition);
    }
    public float proportionalCWToothOffsetFrom(VectorXZ rel) {
        float offset = cwToothOffsetFrom(rel);
        return offset / toothOffsetAngleDegrees;
    }
    public float normalizedCWToothOffsetFrom(VectorXZ rel) {
        return Angles.FloatModSigned(cwToothOffsetFrom(rel), toothOffsetAngleDegrees) / toothOffsetAngleDegrees;
    }

    protected virtual float cwToothOffsetFrom(VectorXZ rel) {
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

            setDistanceFrom(gear);

            //set rotation
            Vector3 euler = transform.rotation.eulerAngles;
            VectorXZ relXZ = new VectorXZ(transform.position - _driver.transform.position);
            if (gear is RackGear) {
                relXZ = xzPosition - ((RackGear)gear).closestPointOnLine(xzPosition);
            }
            //float normalizedOther = gear.proportionalCWToothOffsetFrom(relXZ);
            float normalizedOther = gear.proportionalCWToothOffsetFromAbsPosition(xzPosition);
            float closestOffset = cwToothOffsetFrom(relXZ * -1f);
            euler.y = euler.y + closestOffset + toothOffsetAngleDegrees * (normalizedOther + .5f);
            transform.eulerAngles = euler; 
        }
    }

    protected virtual void setDistanceFrom(Gear gear) {
        Vector3 refPoint = _driver.transform.position;
        if (gear is RackGear) {
            RackGear rackGear = (RackGear)gear;
            refPoint = rackGear.closestPointOnLine(new VectorXZ(transform.position)).vector3(rackGear.transform.position.y);
        }
        Vector3 relPos = transform.position - refPoint;
        relPos = relPos.normalized * (innerRadius + gear.innerRadius + ToothDepth);
        transform.position = refPoint + relPos;
    }

    protected override DrivableConnection getDrivableConnection(Collider other) {
        GearConnection gc = new GearConnection(this);
        if (isDriven() || isConnectedTo(other.transform) || !isInConnectionRange(other)) { return gc; }
        gc.axel = getAxel(other);
        if (gc.axel != null && !gc.axel.hasChild) {
            gc.makeConnection = setSocketClosestToAxel;
        } else if (other.GetComponent<Gear>() != null) {
            gc.other = other;
            gc.makeConnection = beDrivenBy;
        }
        return gc;
    }

    protected class GearConnection : DrivableConnection
    {
        public Axel axel;
        public GearConnection(Gear _gear) : base(_gear) { }
    }

    //protected override bool vConnectTo(Collider other) {
    //    print("gear connect");
    //    if (isDriven()) {
    //        print("is driven already");
    //        return false;
    //    }
    //    if (isConnectedTo(other.transform)) {
    //        print("is connected to " + other.name + " already");
    //        return false;
    //    }
    //    // If this is an axel, get driven by it
    //    Axel axel = getAxel(other);
    //    if (axel != null && !axel.hasChild) {
    //        print("axel not null");
    //        setSocketClosestToAxel(axel);
    //        return true;
    //    }
    //    if (!isInConnectionRange(other)) {
    //        print("connection not in range");
    //        return false;
    //    }

    //    // Otherwise, if this is a gear, get driven by it
    //    Gear gear = other.GetComponent<Gear>(); 
    //    if (gear != null) {
    //        Bug.bugError(name + " conn to gear: " + gear.name);
    //        beDrivenBy(gear);
    //        return true;
    //    }
    //    return false;
    //}

    protected virtual bool setSocketClosestToAxel(DrivableConnection dc) {
        setSocketClosestToAxel(((GearConnection) dc).axel);
        return true;
    }

    protected virtual bool beDrivenBy(DrivableConnection dc) {
        Gear gear = dc.other.GetComponent<Gear>();
        if (gear != null) {
            beDrivenBy(gear);
            return true;
        }
        return false;
    }
    public void beDrivenBy(Gear gear) {
        gear.addDrivable(this);
        _driver = gear;
        positionRelativeTo(gear);
        AudioManager.Instance.play(this, AudioLibrary.GearSoundName);
    }

    protected override bool connectToControllerAddOn(ControllerAddOn cao) {
        return false;
    }

    protected override bool connectToReceiverAddOn(ReceiverAddOn rao) {
        return false;
    }

    #region connection data
    [System.Serializable]
    class ConnectionData
    {
        public List<string> drivableGuids = new List<string>();
    }
    public override void storeConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd = new ConnectionData();
        foreach(Drivable d in drivables) {
            cd.drivableGuids.Add(d.GetComponent<Guid>().guid.ToString());
        }
        SaveManager.Instance.SerializeIntoArray(cd, ref connectionData);
    }

    public override void restoreConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd;
        if ((cd = SaveManager.Instance.DeserializeFromArray<ConnectionData>(ref connectionData)) != null) {
            foreach(String drivableGuid in cd.drivableGuids) {
                GameObject drivenGO = SaveManager.Instance.FindGameObjectByGuid(drivableGuid);
                Drivable d = drivenGO.GetComponent<Drivable>();
                if (d is Gear) {
                    ((Gear)d).beDrivenBy(this);
                } else {
                    addDrivable(d);
                }
            }
        }
    }
    #endregion

    //TODO: restore conn data for gears / rack gears.
    //remember position , rotation.
    //figure out: do rack gears tend to move along with LAs? when LAs reconnect / restore with LACs?
}


