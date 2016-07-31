using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: debug rack gear restore from save: driven gears are not moving
public class RackGear : Gear {
    protected LineSegment lineSegment;
    protected const float ToothBaseHeight = ToothDepth * 2.7f;
    private VectorXZ baseToToothBaseHeight = new VectorXZ(0f, .684f);
    protected static readonly Vector3 vToothCountPositive = Vector3.right;

    protected VectorXZ basePosition;
    protected Transform anchor;
    protected float length;

    //protected float toothWidth { get { return toothOffset * 2f; } } //TODO: difference btwn toothWidth and toothOffest? why is there one?

    protected VectorXZ offset {
        get { return (xzPosition - basePosition); }
    }
    
    protected float offsetScalar {
        get {
            return offset.dot(new VectorXZ(transform.rotation * vToothCountPositive));
        }
    }
    public VectorXZ normal {
        get { return lineSegment.normal; }
    }
    public VectorXZ normalized {
        get { return lineSegment.normalized; }
    }
    private VectorXZ toothStartPos { get { return lineSegment.startXZ; } }

    protected override void updateAngleStep() {
        _angleStep.update(linearOffset);
    }
    public float linearOffset {
        get { return offsetScalar / toothOffset; }
    }

    public float toothPosition(Vector3 global) {
        return Mathf.Clamp((global - toothStartPos).dot(gearTransform.rotation * vToothCountPositive), 0f, length + toothOffset * 1.75f) / toothOffset;
    }

    public override bool isDriven() {
        if (base.isDriven()) { return true; }
        return anchor != null;
    }
    protected override float colliderRadius {
        get {
            return ToothBaseHeight;
        }
    }
    public override float innerRadius {
        get {
            return 0f; // ToothBaseHeight;
        }
    }
    protected override void awake() {
        base.awake();
        lineSegment = GetComponent<LineSegment>();
        basePosition = xzPosition;
        length = lineSegment.distance.magnitude;
    }


    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new RackGearContractNegotiator(this);
    }

    public class RackGearContractNegotiator : GearContractNegotiator
    {
        public RackGearContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            if (cogForTypeWorkaround is Piston) {
                result.Add(new ContractSpecification(CogContractType.PARENT_CHILD, RoleType.CLIENT));
            } else if (cogForTypeWorkaround is Gear) {
                result.Add(new ContractSpecification(CogContractType.DRIVER_DRIVEN, RoleType.CLIENT));
            }
            return result;
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableRackGearContractLookup(this);
    }

    public class ViableRackGearContractLookup : ViableGearContractLookup
    {
        public ViableRackGearContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });
            asClientLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return other.GetComponent<LinearDrive>() != null; //(i.e. Piston)
            });

            asProducerLookup.Add(CogContractType.DRIVER_DRIVEN, delegate (Cog other) {
                return other.GetComponent<Gear>();
            });
            asProducerLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return true;
            });
        }
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        List<ContractSite> sites = ContractSite.contractSiteListFromSocketSet(this, _pegboard.getBackendSocketSet());
        sites.Add(new ContractSite(this, SiteOrientation.selfMatchingOrientation()));
        UniqueClientContractSiteBoss uccsb = new UniqueClientContractSiteBoss(
            /* 1.) client site */
            new ExclusionarySiteSetClientPair(
                ClientOnlyCTARSet.clientDrivenAndParentChildSet(),
                new ExclusionarySiteSet(sites.ToArray())
                ),
            /* 2.) producer sites */
            new Dictionary<CTARSet, SiteSet>() {
                { new CTARSet(new ContractTypeAndRole(CogContractType.DRIVER_DRIVEN, RoleType.PRODUCER)),
                    new SiteSet(ContractSite.factory(this, SiteOrientation.selfMatchingOrientation(), MAX_CLIENT_GEARS))},
            });
        addConnectionSiteEntriesForFrontSocketSet(this, uccsb);
        return uccsb;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            actions = base.clientActionsFor(producer, specification);
        } else if (specification.contractType == CogContractType.PARENT_CHILD) {
            if (producer is LinearDrive) {
                actions.receive = delegate (Cog _producer) {
                    transform.rotation = Quaternion.Euler(((LinearDrive)_producer).linearDriveEuler());
                    basePosition = xzPosition;
                };
            }
            actions.beAbsolvedOf = delegate (Cog _producer) {
                disconnectBackendSockets();
            };
        }
        return actions;
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.DRIVER_DRIVEN) {
            return delegate (ConnectionSiteAgreement csa) {
                positionRelativeTo((Drivable)csa.destination.cog);
                adjustForCrowding();
            };
        } else if (specification.contractType == CogContractType.PARENT_CHILD) {
            return ConnectionSiteAgreement.alignAndPushYLayer(transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    #endregion

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
        transform.RotateAround(pivot.position, Vector3.up, Quaternion.FromToRotation(direction.vector3(), dir.vector3()).eulerAngles.y); // .SetLookRotation(dir.vector3());
    }

    protected override DrivableConnection getDrivableConnection(Collider other) {
        DrivableConnection dc = base.getDrivableConnection(other);
        if (dc.viable) return dc;
        RackGearConnection rgc = new RackGearConnection(this);
        rgc.linearMotionDrivable = findConnectedLinearActuator(other);
        if (rgc.linearMotionDrivable == null) {
            rgc.linearMotionDrivable = FindInCog<GearDrivenMechanism>(other.transform);
            if (rgc.linearMotionDrivable == null) {
                return dc;
            }
        }
        rgc.peg = _pegboard.getBackendSocketSet().closestOpenPegOnFrontendOf(other, out rgc.socket);
        if (rgc.peg != null || (autoGeneratePegOnConnect && hasFrontEndSockets(other))) {
            //if (RotationModeHelper.CompatibleModes(rgc.peg.pegIsParentRotationMode, rgc.socket.socketIsChildRotationMode)) {
            rgc.other = other;
            rgc.makeConnection = makeConnectionWithLinearMotionDrivable;
            //}
            print("made conn delegate");
        }
        return rgc;
    }

    protected class RackGearConnection : DrivableConnection
    {
        public Drivable linearMotionDrivable;
        public RackGearConnection(Drivable _drivable) : base(_drivable) { }
    }

    protected bool makeConnectionWithLinearMotionDrivable(DrivableConnection dc) {
        RackGearConnection rgc = (RackGearConnection)dc;
        if (rgc.peg == null) {
            if (!instantiatePegAndConnect(rgc)) { return false; }
        }
        VectorXZ dir;
        if (rgc.linearMotionDrivable is LinearActuator) {
            dir = ((LinearActuator)rgc.linearMotionDrivable).direction;
        } else if (rgc.linearMotionDrivable is Piston) {
            dir = ((Piston)rgc.linearMotionDrivable).direction;
        } else {
            return false;
        }
        rotateInDirection(dir, rgc.peg.transform);
        setSocketToPeg(rgc);
        return true;
    }

    protected override void onSocketToParentPeg(Socket socket) {
        base.onSocketToParentPeg(socket);
        anchor = socket.drivingPeg.transform;
        Debug.LogError("on sock to parent peg rack gear");
        basePosition = xzPosition;
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
        return _angleStep.getAngle();
        //return _angleStep.deltaAngle;
    }

    public override Drive receiveDrive(Drive drive) {
        if (drive.sourceIsType(typeof(Gear))) { 
        //if (drivingGear != null) {
            Vector3 dir = transform.rotation * Vector3.right;
            float scalar = -((Gear)drive.source).tangentVelocity(); // -drivingGear.tangentVelocity(); 
            transform.position += dir * scalar;
        }
        return drive;
    }

    public VectorXZ closestPointOnLine(VectorXZ p) {
        return lineSegment.closestPointOnLine(p);
    }
    public VectorXZ closestPointOnSegment(VectorXZ p) { return lineSegment.closestPointOnSegment(p); }

    public Vector3 sympatheticToNormal(Vector3 v) { return lineSegment.sympatheticToNormal(v); }

    protected override VectorXZ getConnectionPoint(Collider other) {
        return lineSegment.closestPointOnSegment(new VectorXZ(other.transform.position));
    }

    protected int closestToothOrdinal(VectorXZ global) {
        VectorXZ online = lineSegment.closestPointOnLine(global);
        VectorXZ dif = online - lineSegment.startXZ;
        int tooth;
        if (lineSegment.sympatheticDirection(dif)) {
            return Mathf.FloorToInt(dif.magnitude / toothOffset);
        } else {
            return Mathf.CeilToInt(dif.magnitude / toothOffset); 
        }
    }

    protected int closestToothOrdinal(VectorXZ global, bool wantVirtual) {
        int tooth = closestToothOrdinal(global); 
        if (!wantVirtual) { return tooth; }
        VectorXZ online = lineSegment.closestPointOnLine(global);
        if (!lineSegment.isOnSegment(online)) {
            VectorXZ dif = online - lineSegment.startXZ;
            if (lineSegment.sympatheticDirection(dif)) { //beyond line end
                tooth = toothCount;
            } else {
                tooth = 0;
            }
        }
        return tooth;
    }

    protected VectorXZ closestTooth(VectorXZ other) {
        return closestTooth(other, false);
    }

    protected VectorXZ closestTooth(VectorXZ global, bool wantVirtual) {
        //VectorXZ online = lineSegment.closestPointOnLine(global);
        //VectorXZ dif = online - lineSegment.startXZ;
        //int tooth = closestToothOrdinal(global); 
        //if (wantVirtual && !lineSegment.isOnSegment(online)) {
        //    if (lineSegment.sympatheticDirection(dif)) { //beyond line end
        //        tooth = toothCount;
        //    } else {
        //        tooth = 0;
        //    }
        //}
        int tooth = closestToothOrdinal(global, wantVirtual);
        return lineSegment.startXZ + lineSegment.normalized * (tooth * toothOffset);
    }
    protected VectorXZ closestInterstice(VectorXZ global) {
        int tooth = closestToothOrdinal(global, true);
        return lineSegment.startXZ + lineSegment.normalized * ((tooth + .5f) * toothOffset);
    }

    

    protected override void setDistanceFrom(Gear gear, ConnektReconstruction cr) {
        transform.position += TransformUtil.distanceToTangentPointAbsoluteNormalDirection(
            gear, 
            lineSegment, 
            innerRadius + ToothDepth, 
            true).vector3(gear.transform.position.y);
    }

    public VectorXZ closestIntersticeFrom(VectorXZ global) {
        return closestTooth(global); // + transform.rotation * baseToToothBaseHeight.vector3();
    }
    public override float proportionalCWToothOffsetFromAbsPosition(VectorXZ global) {
        VectorXZ virtualTooth = closestTooth(global, true);
        VectorXZ online = lineSegment.closestPointOnLine(global);

        BugLine.Instance.drawFromToBumpUp(virtualTooth.vector3(), online.vector3());

        VectorXZ dif = online - virtualTooth;
        return dif.magnitude / toothOffset;
    }

    public override void gearPositionRelativeTo(Drivable _someDriver, ConnektReconstruction cr) {
        if (_someDriver != null) {
            if (!(_someDriver is Gear)) { base.positionRelativeTo(_someDriver); return; }
            Gear gear = (Gear)_someDriver;

            //set distance
            setDistanceFrom(gear, cr);

            //nudge to engage other's teeth
            VectorXZ gearXZ = new VectorXZ(gear.transform.position);
            VectorXZ closest = lineSegment.closestPointOnLine(gearXZ);

            float normalizedOther = gear.normalizedCWToothOffsetFrom(closest - gearXZ);
            float nudge =  toothOffset * (normalizedOther + .5f);
            VectorXZ toothPos = closestTooth(gearXZ);
            VectorXZ n = closest - toothPos + lineSegment.normalized * nudge; 
            transform.position += n.vector3() ;
        }
    }
}
