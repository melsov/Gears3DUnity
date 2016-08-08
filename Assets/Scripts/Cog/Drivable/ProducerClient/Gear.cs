using UnityEngine;
//using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

public interface GearDrivable {}

//TODO: generally convert tranform operations to (kinematic) rigidbody operations
public class Gear : Drivable , GearDrivable
{
    protected const int MAX_CLIENT_GEARS = 8;
    public int toothCount = 6;
    public const float toothOffset = .5f;
    public const float ToothDepth = .25f;

    protected float toothOffsetAngleRadians {
        get {
            return Mathf.PI * 2 / toothCount;
        }
    }
    protected float toothOffsetAngleDegrees { get { return toothOffsetAngleRadians * Mathf.Rad2Deg; } }

    public virtual float innerRadius {
        get { return toothOffset / (2f * Mathf.Sin(Mathf.PI / toothCount));  }
    }

    protected virtual float colliderRadius {
        get { return innerRadius; }
    }
    public float outerRadius {
        get { return innerRadius + ToothDepth; }
    }
    public float middleRadius {
        get { return innerRadius + ToothDepth * .5f; }
    }

    protected Rigidbody rbGear;
    private void setupRBGear() {
        rbGear = gearTransform.GetComponent<Rigidbody>();
    }
    public delegate void RotGear(Quaternion global);
    public RotGear rotGear;
    private void setupRotGear() {
        if (gearTransform.GetComponent<Rigidbody>()) {
            rotGear = delegate (Quaternion global) {
                rbGear.MoveRotation(global);
            };
        } else {
            rotGear = delegate (Quaternion global) {
                gearTransform.rotation = global;
            };
        }
    }

    protected override void awake() {
        base.awake();
        setupToothRotationMapFor(this);
        setupRBGear();
        setupRotGear();
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        cc.radius = colliderRadius;
        lengthenCollider(cc);
        if (gearTransform != null) {
            gearTransform.gameObject.layer = LayerLookup.GearMesh;
        }
        
        //if (tag.Equals("TestThisGear"))
        //    StartCoroutine(testClockAngle());
    }
    protected override void updateAngleStep() {
        if (isOnAxel()) {
            base.updateAngleStep();
        } else {
            _angleStep.update(gearTransform.rotation.eulerAngles.y);
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
                return other.GetComponent<GearDrivable>() != null;
            });
            asProducerLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return true;
            });
        }
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            actions.receive = delegate (Cog cog) {
                
            };
            actions.beAbsolvedOf = delegate (Cog cog) {
                disconnectFromDriver();
            };
        } else if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.receive = delegate (Cog _producer) {
            };
            actions.beAbsolvedOf = delegate (Cog _producer) {
                disconnectBackendSockets();
            };
        }
        return actions;
    }

    // TODO: at least two sub categories of Drivable: driven-and-driving (e.g. gear), driving only (e.g. motor)
    // Driven-and-driving subclass implements and seals getCSBoss(): provides a unique client site,
    // driving only provides no site set.
    // ALSO: driven only?
    // LASTLY: Drivable itself (actually) seals getCSB() to guarantee that it gets a DrivableCSB.
    // Gets the actual DrivableCSB through an abstract method for sub classes
    // ALSO LASTLY: could add key value pairs for any sockets in socket set

    // TODO: convert _driver into a property whose get relies on drivableCSB.driver;

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        UniqueClientContractSiteBoss uccsb = new UniqueClientContractSiteBoss(
            /* 1.) client site */
            new KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet>(
                ClientOnlyCTARSet.clientDrivenAndParentChildSet(), 
                new ExclusionarySiteSet(new ContractSite(this, SiteOrientation.selfMatchingOrientation()))
                ),
            /* 2.) producer sites */
            new Dictionary<CTARSet, SiteSet>() {
                { new CTARSet(new ContractTypeAndRole(CogContractType.DRIVER_DRIVEN, RoleType.PRODUCER)),
                    new SiteSet(ContractSite.factory(this, SiteOrientation.selfMatchingOrientation(), MAX_CLIENT_GEARS))},
            });
        addConnectionSiteEntriesForFrontSocketSet(this, uccsb);
        return uccsb;
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            return delegate (ConnectionSiteAgreement csa) {
                gearPositionRelativeTo((Drivable)csa.destination.cog, csa.connektReconstruction);
                adjustForCrowding();
                
            };
        } else if (specification.contractType == CogContractType.PARENT_CHILD) {
            return delegate (ConnectionSiteAgreement csa) {
                Debug.LogError(name + " set socket to axel");
                setSocketClosestToAxel(getAxel(csa.destination.cog));
            };
        }
        return ConnectionSiteAgreement.doNothing;
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

//CONSIDER: if needed this could be improved by replacing conditional with a delegate, set at contract receive time
    protected float rotationDeltaY(Drive drive) {
        if (drive.sourceIsType(typeof(RackGear))) { //NOTE: no need for this condition atm.
            return rackGearDriver.toothPosition(gearTransform.position) * toothOffsetAngleDegrees; // rackGearDeltaYDegrees(drive.amount); //-1f * Mathf.Rad2Deg * drive.amount / innerRadius;
        } else {
            return drive.amount * -1f * toothOffsetAngleRadians;
        }
    }

    protected RackGear rackGearDriver {
        get {
            return (RackGear)uniqueContractSiteAgreement.producerSite.cog;
        }
    }

    #region tooth-rotation-map
    protected class ToothRotationMap
    {
        public struct GearInfo
        {
            public readonly int toothCount;
            public readonly float toothOffsetAngleDegrees;

            public GearInfo(int toothCount, float toothOffsetAngleDegrees) {
                this.toothCount = toothCount;
                this.toothOffsetAngleDegrees = toothOffsetAngleDegrees;
            }

            public static GearInfo fromGear(Gear gear) {
                GearInfo gi = new GearInfo(
                    gear.toothCount, gear.toothOffsetAngleDegrees);
                return gi;
            }
        }

        private Dictionary<int, Quaternion> llookup = new Dictionary<int, Quaternion>();
        protected GearInfo gearInfo;

        private const int divisions = 40;
        private float divisionsF { get { return divisions; } }

        public ToothRotationMap(GearInfo gearInfo) {
            this.gearInfo = gearInfo;
            float theta = 90f;
            for(int i = 0; i <= gearInfo.toothCount; ++i) {
                Quaternion current = Quaternion.Euler(0f, theta, 0f);
                Quaternion next = Quaternion.Euler(0f, theta + gearInfo.toothOffsetAngleDegrees, 0f);
                for (int j = 0; j < divisions; ++j) {
                    llookup.Add(i * divisions + j, Quaternion.Slerp(current, next, j / divisionsF));
                }
                theta += gearInfo.toothOffsetAngleDegrees;
            }
        }

        public Quaternion rotationFor(float normalizedToothScale) {
            if(normalizedToothScale < 0f) {
                return llookup[0];
            }
            normalizedToothScale = Angles.FloatModSigned(normalizedToothScale, gearInfo.toothCount) * divisionsF;
            int floor = Mathf.FloorToInt(normalizedToothScale);
            int ceil = Mathf.CeilToInt(normalizedToothScale);
            return Quaternion.Slerp(llookup[floor], llookup[ceil], normalizedToothScale - floor);
        }

    }

    private static Dictionary<int, ToothRotationMap> _toothRotationMaps = new Dictionary<int, ToothRotationMap>();
    private static void setupToothRotationMapFor(Gear gear) {
        if (!_toothRotationMaps.ContainsKey(gear.toothCount)) {
            _toothRotationMaps[gear.toothCount] = new ToothRotationMap(ToothRotationMap.GearInfo.fromGear(gear));
            gear.getTRM = delegate () {
                return _toothRotationMaps[gear.toothCount];
            };
        }
    }
    private delegate ToothRotationMap GetTRM();
    private GetTRM getTRM;
    private ToothRotationMap toothRotationMap {
        get {
            return getTRM();
        }
    }
    #endregion

    public override Drive receiveDrive(Drive drive) {
        // ****** TEST
        //return drive; // isolating connect to rack gear
        //TEST
        if (drive.sourceIsType(typeof(RackGear))) {
            rotGear(toothRotationMap.rotationFor(rackGearDriver.toothPosition(gearTransform.position)));
            //rotGear(Quaternion.Euler(new Vector3(0f, baseYEuler + rotationDeltaY(drive), 0f))); // <<<<--Wanttt

            //rotGear(Quaternion.Euler(gearTransform.eulerAngles + new Vector3(0f, rotationDeltaY(drive), 0f)));
        } else {
            rotGear(Quaternion.Euler(gearTransform.eulerAngles + new Vector3(0f, rotationDeltaY(drive), 0f)));
        }

        //rotGear(Quaternion.Euler(gearTransform.eulerAngles + new Vector3(0f, rotationDeltaY(drive), 0f))); //OLD WAY
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
    
    protected IEnumerator testClockAngle() {
        Quaternion q = Angles.zPosPointsTowards(Vector3.up * -1f);
        transform.rotation = q;
        
        int wedges = 12;
        float theta = Mathf.PI / 2f;
        float incr = Mathf.PI * 2 / ((float)wedges);
        foreach(VectorXZ dir in Angles.UnitVectors(wedges)) { 
            yield return new WaitForSeconds(1f);

            //float clock = Angles.VectorXZToDegrees(dir); // positiveClockAngle(dir);
            //print(clock + " : " + dir.ToString());
            //Vector3 euler = new Vector3(0f, clock, 0f);
            transform.rotation = Angles.zPosPointsTowards(dir);
            //transform.eulerAngles = euler;
            BugLine.Instance.drawFrom(transform.position + Vector3.up * 2f, dir);
            theta += incr;
        }
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
    public virtual void gearPositionRelativeTo(Drivable _someDriver, ConnektReconstruction cr) {
        if (_someDriver != null) {
            if (!(_someDriver is Gear)) { base.positionRelativeTo(_someDriver); return; }
            print("$$$ pos rel to gear");
            Gear gear = (Gear)_someDriver;
            moveToYPosOf(gear.gearTransform);
            setDistanceFrom(gear, cr);
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
    /* TODO: devine the mysteries...*/
    protected Vector3 eulersRelativeToGear(Gear gear) {
        Vector3 euler = startEulerAnglesForAligningTeeth;
        VectorXZ relXZ = new VectorXZ(transform.position - gear.transform.position);
        if (gear is RackGear) {
            return toothRotationMap.rotationFor(((RackGear)gear).toothPosition(gearTransform.position)).eulerAngles;
        }
        float normalizedOther = gear.proportionalCWToothOffsetFromAbsPosition(xzPosition);
        float closestOffset = cwToothOffsetFrom(relXZ * -1f);
        euler.y += closestOffset + toothOffsetAngleDegrees * (normalizedOther + .5f);
        return euler; 
    }

    protected virtual void setDistanceFrom(Gear gear, ConnektReconstruction cr) {
        Vector3 refPoint = gear.gearTransform.position;
        if (gear is RackGear) {
            RackGear rackGear = (RackGear)gear;
            refPoint = rackGear.closestPointOnLine(new VectorXZ(gearTransform.position)).vector3(rackGear.transform.position.y);
        }
        Vector3 relPos = cr ? cr.relativePosition : gearTransform.position - refPoint;
        if (gear is RackGear) {
            relPos = ((RackGear) gear).sympatheticToNormal(relPos);
        }
        /* First time ? store the rel pos we found */
        if (!cr) {
            cr = new ConnektReconstruction();
            cr.relativePosition = relPos;
        }
        relPos = relPos.normalized * (innerRadius + gear.innerRadius + ToothDepth);
        Vector3 target = refPoint + relPos;
        Vector3 dif = target - gearTransform.position;
        transform.position += dif;
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
        Bug.bugAndPause("this doesn't happen right?");
        gear.addDrivable(this);
        _driver = gear;
        positionRelativeTo(gear);
        adjustForCrowding();
    }

    protected void adjustForCrowding() {
        foreach(Collider c in NearbyColliders.nearbyColliders(GetComponent<CapsuleCollider>(), .5f, LayerMask.GetMask("GearMesh"), 1.3f)) {
            Gear neighbor = c.GetComponent<Gear>();
            if (neighbor == this) { continue; }
            if (!neighbor || contractPortfolio.hasContractWith(neighbor)) { continue; }
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


