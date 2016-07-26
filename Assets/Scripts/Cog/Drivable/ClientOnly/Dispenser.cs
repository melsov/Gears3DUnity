using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Dispenser : Drivable { 

    public Dispensable item;
    public Transform spawnPlatform;
    public float fireRate = .5f;
    public float ejectForce = 4f;
    protected bool shouldDispense;
    protected float timer;
    protected bool hasBuiltInButton;

    protected override void awake() {
        base.awake();
        ControllerAddOn cao = GetComponentInChildren<ControllerAddOn>();
        if (cao != null) {
            cao.connectTo(GetComponent<Collider>());
            hasBuiltInButton = true;
            //enterPermanentContractWith(cao);
        }
    }

    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new DispenserContractNegotiator(this);
    }

    public class DispenserContractNegotiator : ContractNegotiator
    {
        public DispenserContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            AddOn addOn = findAddOn(cogForTypeWorkaround);
            if (addOn) {
                if (addOn is ControllerAddOn) {
                    print("dispenser found addOn: " + addOn.name + " of cog: " + FindCog(addOn.transform).name);
                    List<ContractSpecification> specs = new List<ContractSpecification>();
                    specs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.CLIENT));
                    return specs;
                }
            }
            return base.orderedContractPreferencesAsOfferer(cogForTypeWorkaround);
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableDispenserContractLookup(this);
    }

    public class ViableDispenserContractLookup : ViableContractLookup
    {
        public ViableDispenserContractLookup(Cog cog_) : base(cog_) {
        }

        protected Dispenser dispenser { get { return (Dispenser)cog; } }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, delegate (Cog other) {
                print("hi");
                print("dispenser client look up. controller add on null " + (dispenser.controllerAddOn == null));
                return dispenser.controllerAddOn == null; 
            });
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        return ProducerActions.getDoNothingActions();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        if(specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            ClientActions cas = new ClientActions();
            cas.receive = delegate (Cog _producer) {
                print("dispenser receiving addOn contract");
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

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        //TODO: restore this but make Dispenser a cog????? (awkward because its only driver is an add on...)
        //Dictionary<CTARSet, SiteSet> lookup = LocatableSiteSetAndCTARSetSetup.connectionSiteLookupFor(this);
        //UnityEngine.Assertions.Assert.IsTrue(lookup.Keys.Count == 1, "Dispenser should have exactly one connection site");
        //UniqueSiteSiteSet usss;

        return new UniqueClientContractSiteBoss(LocatableSiteSetAndCTARSetSetup.uniqueSiteSetAndClientOnlyCTARFor(this));
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            return ConnectionSiteAgreement.alignTarget(transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    //protected override bool contractShouldBeUnbreakable(CogContract cc) {
    //    if (cc.type == CogContractType.CONTROLLER_ADDON_DRIVABLE &&
    //        TransformUtil.IsChildOf(transform, cc.producer.cog.transform) &&
    //        cc.client.cog == this) {
    //        return true;
    //    }
    //    return base.contractShouldBeUnbreakable(cc);
    //}

    #endregion

    protected override bool connectToControllerAddOn(ControllerAddOn cao) {
        if (hasBuiltInButton) { return false; }
        return base.connectToControllerAddOn(cao);
    }

    protected float _power = 1f;
    protected virtual float power {
        get { return _power; }
        set {
            if (Time.fixedTime - timer > fireRate && value > 0f) {
                shouldDispense = true;
                _power = 1f;
                timer = Time.fixedTime;
            }
        }
    }


    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    protected virtual Vector3 dispenseDirection {
        get { return new VectorXZ(spawnPlatform.transform.position - transform.position).vector3(0f).normalized;  }
    }


    protected override void update() {
        base.update();
        if (shouldDispense) {
            shouldDispense = false;
            dispense();
        }
    }
    
    protected virtual void dispense() {
        Dispensable d = Instantiate<Dispensable>(item);
        d.enabled = true;
        d.transform.position = spawnPlatform.position;
        d.GetComponent<Rigidbody>().AddForce(dispenseDirection * ejectForce, ForceMode.Impulse);
    }

    protected override void handleAddOnScalar(float scalar) {
        power = scalar;
    }

    protected override void resetAddOnScalar() {
        base.resetAddOnScalar();
        power = 0f;
    }
}
