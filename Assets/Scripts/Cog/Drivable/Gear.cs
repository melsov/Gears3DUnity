using UnityEngine;
//using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

//TODO: generally convert tranform operations to (kinematic) rigidbody operations
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
        lengthenCollider(cc);
        if (gearTransform != null) {
            gearTransform.gameObject.layer = LayerLookup.GearMesh;
        }
    }

    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new GearContractNegotiator(this);
    }

    public class GearContractNegotiator : ContractNegotiator
    {
        public GearContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            if (cogForTypeWorkaround is Gear) {
                result.Add(new ContractSpecification(CogContractType.DRIVER_DRIVEN, RoleType.CLIENT));
            } else if (cogForTypeWorkaround is Motor) {
                print("pref for motor");
                result.Add(new ContractSpecification(CogContractType.PARENT_CHILD, RoleType.CLIENT));
            }
            return result;
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableGearContractLookup(this);
    }

    public class ViableGearContractLookup : ViableDrivableContractLookup
    {
        protected Gear gear {
            get { return (Gear)cog; }
        }

        public ViableGearContractLookup(Cog cog_) : base(cog_) {}

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });
            asClientLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                Axel axel = gear.getAxel(other);
                return axel && !axel.hasChild;
            });

            asProducerLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        ProducerActions actions = ProducerActions.getDoNothingActions();
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            actions.initiate = delegate (Cog cog) {
                addDrivable((Drivable)cog);
            };
            actions.dissolve = delegate (Cog cog) {
                while(drivables.Contains((Drivable)cog)) {
                    drivables.Remove((Drivable)cog);
                }
            };
            actions.fulfill = conveyDrive;
        }
        return actions;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            actions.receive = delegate (Cog cog) {
                _driver = (Drivable)cog;
                positionRelativeTo(_driver);
                adjustForCrowding();
            };
            actions.beAbsolvedOf = delegate (Cog cog) {
                print(name + " get absovled from contract");
                disconnectFromDriver();
            };
        } else if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.receive = delegate (Cog _producer) {
                print("gear ParentChild receive action with: " + _producer.name);
                setSocketClosestToAxel(getAxel(_producer));
            };
            actions.beAbsolvedOf = delegate (Cog _producer) {
                disconnectBackendSockets();
            };
        }
        return actions;
    }

    #endregion

    protected virtual void lengthenCollider(CapsuleCollider cc) {
        cc.height = 10f;
        cc.center = TransformUtil.SetY(cc.center, -3.3f);
    }

    public virtual float driveRadius {
        get {
            return innerRadius + ToothDepth / 2f;
        }
    }
    protected override float radius {
        get {
            return innerRadius * Mathf.Sin(toothOffsetAngleRadians);
        }
    }

    public float tangentVelocity() {
        return _angleStep.deltaAngle * Mathf.Deg2Rad * innerRadius;
    }

    public override float driveScalar() {
        return _angleStep.deltaAngle / toothOffsetAngleRadians;
    }

    protected float rotationDeltaY(Drive drive) {
        if (_driver is RackGear) {
            return -Mathf.Rad2Deg * drive.amount / innerRadius;
        } else {
            return drive.amount * -1f * toothOffsetAngleRadians;
        }
    }

    public override Drive receiveDrive(Drive drive) {
        gearTransform.eulerAngles += new Vector3(0f, rotationDeltaY(drive), 0f);
        return drive;
    }

    public float rot { get { return gearTransform.rotation.eulerAngles.y; } }

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
            moveToYPosOf(gear.gearTransform);
            setDistanceFrom(gear);
            gearTransform.eulerAngles = eulersRelativeToGear(gear);
        }
    }

    protected override void vDisconnect() {
        base.vDisconnect();
        resetYPosition();
    }

    protected void resetYPosition() {
        gearTransform.position = TransformUtil.SetY(gearTransform.position, YLayer.Layer(typeof(Gear)));
    }

    protected void moveToYPosOf(Transform other) {
        gearTransform.position = TransformUtil.SetY(gearTransform.position, other.transform.position.y);
    }

    protected Vector3 startEulerAnglesForAligningTeeth {
        get { return gearTransform.eulerAngles; }
    }
    protected virtual Transform gearTransform {
        get { return transform; }
    }

    protected Vector3 eulersRelativeToGear(Gear gear) {
        Vector3 euler = startEulerAnglesForAligningTeeth;
        VectorXZ relXZ = new VectorXZ(transform.position - gear.transform.position);
        if (gear is RackGear) {
            relXZ = xzPosition - ((RackGear)gear).closestPointOnLine(xzPosition);
        }
        float normalizedOther = gear.proportionalCWToothOffsetFromAbsPosition(xzPosition);
        float closestOffset = cwToothOffsetFrom(relXZ * -1f);
        euler.y += closestOffset + toothOffsetAngleDegrees * (normalizedOther + .5f);
        return euler; 
    }

    protected virtual void setDistanceFrom(Gear gear) {
        print("gear set dist from other " + gear.name);
        Vector3 refPoint = gear.transform.position;
        if (gear is RackGear) {
            RackGear rackGear = (RackGear)gear;
            refPoint = rackGear.closestPointOnLine(new VectorXZ(transform.position)).vector3(rackGear.transform.position.y);
        }
        Vector3 relPos = transform.position - refPoint;
        print("setting position distance: " + relPos.magnitude);
        relPos = relPos.normalized * (innerRadius + gear.innerRadius + ToothDepth);
        transform.position = refPoint + relPos;
    }

    protected override DrivableConnection getDrivableConnection(Collider other) {
        GearConnection gc = new GearConnection(this);
        if (isDriven() || isConnectedTo(other.transform) || !isInConnectionRange(other)) { return gc; }
        gc.axel = getAxel(other);
        if (gc.axel != null && gc.axel.hasChild) { Debug.LogError("already has child"); } // DEBUG
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

    protected virtual bool setSocketClosestToAxel(DrivableConnection dc) {
        setSocketClosestToAxel(((GearConnection) dc).axel);
        adjustForCrowding();
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
        adjustForCrowding();
    }

    protected void adjustForCrowding() {
        foreach(Collider c in NearbyColliders.nearbyColliders(GetComponent<CapsuleCollider>(), .5f, LayerMask.GetMask("GearMesh"), 1.3f)) {
            Gear neighbor = c.GetComponent<Gear>();
            if (neighbor == this) { continue; }
            if (!neighbor || neighbor == _driver || drivables.Contains(neighbor)) { continue; }
            if (Mathf.Abs(gearTransform.position.y - neighbor.gearTransform.position.y) < YLayer.LayerHeight) {
                yHeightOneLayerUpFrom(neighbor.gearTransform);
            }
        }
    }
    protected void yHeightOneLayerUpFrom(Transform other) {
        gearTransform.position = TransformUtil.SetY(gearTransform.position, other.position.y + YLayer.LayerHeight);
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


