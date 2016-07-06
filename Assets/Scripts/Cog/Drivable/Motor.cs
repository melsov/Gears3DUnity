using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Motor : Drivable
{
    protected HandleSet handleSet;
    protected LeverLimits leverLimits;
    protected float leverMultiplier;
    protected float leverMax = 10f;
    protected Handle lever { get { return handleSet.handles[0]; } }
    protected OnOffIndicator onOffIndicator;
    protected Counter counter;

    public float maxAngularVelocity = 10f;
    protected float _power = 1f;
    public virtual float power {
        get { return _power * _isPaused * leverMultiplier; }
        set {
            _power = Mathf.Clamp(value, -1f, 1f);
            if(onOffIndicator != null) {
                onOffIndicator.state = SwitchStateHelper.stateFor(_power);
            }
            updateAudio();
        }
    }

    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new MotorContractNegotiator(this);
    }

    public class MotorContractNegotiator : ContractNegotiator
    {
        public MotorContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            //TODO: motors offer to AddOn and those AddOn/Drivable hybrid things: gear switches
            AddOn addOn = findAddOn(cogForTypeWorkaround);
            if (addOn) {
                if (addOn is ControllerAddOn) {
                    print("motor found addOn: " + addOn.name + " of cog: " + FindCog(addOn.transform).name);
                    List<ContractSpecification> specs = new List<ContractSpecification>();
                    specs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.CLIENT));
                    return specs;
                }
            }
            return base.orderedContractPreferencesAsOfferer(cogForTypeWorkaround);
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableMotorContractLookup(this);
    }

    public class ViableMotorContractLookup : ViableContractLookup
    {
        protected Motor motor { get { return (Motor)cog; } }

        public ViableMotorContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asProducerLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return !motor.axel.hasChild && ((Drivable)other).hasOpenBackendSocket();
            });
            asClientLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, delegate (Cog other) {
                return motor.controllerAddOn == null; 
            });
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            ProducerActions pas = new ProducerActions();
            pas.initiate = delegate (Cog _client) { };
            pas.dissolve = delegate (Cog _client) { };
            pas.fulfill = delegate (Cog _client) { };
            return pas;
        }
        return ProducerActions.getDoNothingActions();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        if(specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            ClientActions cas = new ClientActions();
            cas.receive = delegate (Cog _producer) {
                print("receiving addOn contract");
                controllerAddOn = (ControllerAddOn) findAddOn(_producer);
                controllerAddOn.setScalar += handleAddOnScalar;
            };
            cas.beAbsolvedOf = delegate (Cog _producer) {
                controllerAddOn.setScalar -= handleAddOnScalar;
                controllerAddOn = null;
                resetAddOnScalar();
            };
            return cas;
        }
        return ClientActions.getDoNothingActions();
    }

    protected override ConnectionSiteBoss getConnectionSiteBoss() {
        //Get dictionary with entry for motor's controller add on site
        Dictionary<CTARSet, SiteSet> lookup = LocatableSiteSetAndCTARSetSetup.connectionSiteLookupFor(this);
        //Add an entry for motors axel
        CTARSet parentChildSet = new CTARSet(new ContractTypeAndRole(CogContractType.PARENT_CHILD, RoleType.PRODUCER));
        SiteSet ss = new SiteSet(ConnectionSite.factory(this, SiteOrientation.selfMatchingOrientation(), 1));
        lookup.Add(parentChildSet, ss);
        return new ConnectionSiteBoss(lookup);
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            return ConnectionSiteAgreement.alignTarget(transform);
        }
        //CONSIDER: could we be asked to travel to a gear?
        return ConnectionSiteAgreement.doNothing;
    }

    #endregion

    //TODO: add on connections (on off gear switch) can't be reconnected?
    protected override DrivableConnection getDrivableConnection(Collider other) {
        DrivableConnection dc = new DrivableConnection(this);
        dc = getAddOnDrivableConnection(other, dc);
        print("conn was viable ? " + dc.viable);
        return dc;
    }

    protected void updateAudio() {
        if (Angles.VerySmall(power)) { AudioManager.Instance.stop(this, AudioLibrary.GearSoundName); }
        else { AudioManager.Instance.play(this, AudioLibrary.GearSoundName); }
    }

    protected float _isPaused = 1f;
    protected override void pause(bool isPaused) {
        base.pause(isPaused);
        _isPaused = isPaused ? 0f : 1f;
    }

    protected Axel _axel;
    public Axel axel {
        get { return _axel;  }
    }

    protected float angle;

    #region save
    [System.Serializable]
    class SerializeStorage
    {
        public float maxAngularVelocity;
        public float _power;
        public float _leverMultiplier;
    }
    public override void Serialize(ref List<byte[]> data) {
        base.Serialize(ref data);
        SerializeStorage stor = new SerializeStorage();
        stor.maxAngularVelocity = maxAngularVelocity;
        stor._power = _power;
        stor._leverMultiplier = leverMultiplier;
        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public override void Deserialize(ref List<byte[]> data) {
        base.Deserialize(ref data);
        SerializeStorage stor;
        if((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            maxAngularVelocity = stor.maxAngularVelocity;
            _power = stor._power;
            leverMultiplier = stor._leverMultiplier;
            setLeverPositon((int)leverMultiplier);
        }
    }
    #endregion

    protected override void awake () {
        base.awake();
        _axel = GetComponentInChildren<Axel>();
        UnityEngine.Assertions.Assert.IsTrue(_pegboard.getFrontendSocketSet().sockets.Length == 1);
        axel.beChildOf(_pegboard.getFrontendSocketSet().sockets[0]);
        handleSet = GetComponentInChildren<HandleSet>();
        leverLimits = GetComponentInChildren<LeverLimits>();
        leverLimits.increments = 9;
        onOffIndicator = GetComponentInChildren<OnOffIndicator>();
        counter = GetComponentInChildren<Counter>();
        setLeverPositon((int)leverMultiplier);
	}

	protected override void update () {
        angle += maxAngularVelocity * Time.deltaTime * power;
        axel.turnTo(angle);
	}

    public override bool isDriven() {
        return true;
    }

    public override float driveScalar() {
        return 0; 
    }

    public override Drive receiveDrive(Drive drive) {
        return new Drive(0);
    }

    protected override void handleAddOnScalar(float scalar) {
        print("new add on scalar: " + scalar);
        power = scalar;
    }
    protected override void resetAddOnScalar() {
        power = 1f;
    }

    protected override void vDragOverride(VectorXZ cursorGlobal) {
        Bug.assertNotNullPause(leverLimits.min);
        float z = Mathf.Clamp(cursorGlobal.z, leverLimits.min.z, leverLimits.max.z);
        float gradient = leverLimits.gradientPosition(cursorGlobal.z);
        int lev = leverLimits.closestLevel(gradient);
        setMultiplier(lev);
        counter.turnTo(lev);
        setLeverPositon(lev);
    }

    protected void setLeverPositon(int lev) {
        Vector3 pos = lever.transform.position;
        pos.z = leverLimits.globalZPositionForLevel(lev);
        lever.transform.position = pos;
    }

    protected void setMultiplier(int lev) {
        leverMultiplier = lev;
        updateAudio();
    }
}
