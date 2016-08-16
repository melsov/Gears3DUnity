using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public abstract class Activatable : Drivable
{
    protected HandleSet handleSet;
    protected LeverLimits leverLimits;
    protected abstract float leverMultiplier {
        get; set;
    }
    protected Handle lever { get { return handleSet.handles[0]; } }
    protected OnOffReverseIndicator onOffIndicator;
    protected Counter counter;

    protected override void awake() {
        base.awake();
        handleSet = GetComponentInChildren<HandleSet>();
        leverLimits = GetComponentInChildren<LeverLimits>();
        leverLimits.increments = 9;
        onOffIndicator = GetComponentInChildren<OnOffReverseIndicator>();
        counter = GetComponentInChildren<Counter>();
	}

    public override void Start() {
        base.Start();
        int lev = (int)leverMultiplier;
        setLeverPositon(lev);
        counter.setTo((float)lev);
        setMultiplier(lev);
        updateIndicator();
    }
    
    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new ActivatableContractNegotiator(this);
    }

    public class ActivatableContractNegotiator : ContractNegotiator
    {
        public ActivatableContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            AddOn addOn = findAddOn(cogForTypeWorkaround);
            if (addOn) {
                if (addOn is ControllerAddOn) {
                    Bug.contractLog("activatable found addOn: " + addOn.name + " of cog: " + FindCog(addOn.transform).name);
                    List<ContractSpecification> specs = new List<ContractSpecification>();
                    specs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.CLIENT));
                    return specs;
                }
            }
            return base.orderedContractPreferencesAsOfferer(cogForTypeWorkaround);
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableActivatableContractLookup(this);
    }

    public class ViableActivatableContractLookup : ViableContractLookup
    {
        protected Activatable activatable { get { return (Activatable)cog; } }

        public ViableActivatableContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, delegate (Cog other) {
                return activatable.controllerAddOn == null; 
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
                Bug.contractLog("receiving addOn contract");
                controllerAddOn = (ControllerAddOn) findAddOn(_producer);
                controllerAddOn.addSetScalar(handleAddOnScalar);
            };
            cas.beAbsolvedOf = delegate (Cog _producer) {
                controllerAddOn.removeSetScalar(handleAddOnScalar);
                controllerAddOn = null;
                resetAddOnScalar();
            };
            return cas;
        }
        return ClientActions.getDoNothingActions();
    }
    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        /* client */
        KeyValuePair<ClientOnlyCTARSet, ExclusionarySiteSet> clientSitePair = LocatableSiteSetAndCTARSetSetup.uniqueSiteSetAndClientOnlyCTARFor(this);

        /* producer */
        Dictionary<CTARSet, SiteSet> lookup = new Dictionary<CTARSet, SiteSet>() { };

        return new UniqueClientContractSiteBoss(clientSitePair, lookup);
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            return ConnectionSiteAgreement.alignCog(this); //transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    #endregion

    protected override void handleAddOnScalar(float scalar) {
        activate(scalar);
    }

    protected abstract void activate(float scalar);
    protected abstract SwitchState getSwitchState();
    protected abstract float getPower();
    protected virtual string getSoundName() { return AudioLibrary.GearSoundName; }

    protected void updateIndicator() {
        if (!onOffIndicator) { return; }
        onOffIndicator.state = getSwitchState();
    }

    protected void setLeverPositon(int lev) {
        leverLimits.setTarget(lever.transform, lev);
        //lever.positionOnAxis(leverLimits.globalLinearPositionForLevel(lev));
    }

    protected override void vDragOverride(CursorInfo ci) { // VectorXZ cursorGlobal) {
        int lev = leverLimits.levelFor(ci.current);
        setMultiplier(lev);
        counter.turnTo(lev);
        setLeverPositon(lev);
    }

    protected void setMultiplier(int lev) {
        leverMultiplier = lev;
        updateAudio();
    }
    
    protected void updateAudio() {
        if (Angles.VerySmall(getPower())) { AudioManager.Instance.stop(this, getSoundName()); }
        else { AudioManager.Instance.play(this, getSoundName()); }
    }
}
