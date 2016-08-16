using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ControllerAddOn : AddOn {

    public delegate void SetScalar(float scalar);
    protected SetScalar setScalar;
    public virtual void addSetScalar(SetScalar setScalar) {
        this.setScalar += setScalar;
    }
    public virtual void removeSetScalar(SetScalar setScalar) {
        this.setScalar -= setScalar;
    }
    //DBUG
    public bool debugSetScalarIsNull() {
        return setScalar == null;
    }

    protected override void awake() {
        base.awake();
    }

    #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new CAOContractNegotiator(this);
    }

    public class CAOContractNegotiator : ContractNegotiator
    {
        protected ControllerAddOn controllerAddOn {
            get { return (ControllerAddOn)cog; }
        }
        public CAOContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> prefs = new List<ContractSpecification>();
            prefs.Add(new ContractSpecification(CogContractType.CONTROLLER_ADDON_DRIVABLE, RoleType.PRODUCER));
            return prefs;
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableCAOContractLookup(this);
    }

    public class ViableCAOContractLookup : ViableContractLookup
    {
        protected ControllerAddOn controllerAddOn {
            get { return (ControllerAddOn)cog; }
        }
        public ViableCAOContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asProducerLookup.Add(CogContractType.CONTROLLER_ADDON_DRIVABLE, yup); 
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        ProducerActions pas = new ProducerActions();
        pas.initiate = delegate (Cog _client) {
            if (false) { // shouldPositionOnConnect) {
                Debug.LogError("trying to purge 'shouldPositionOnConnect'; do we really need it in the case of: " + name);
                UnityEditor.EditorApplication.isPaused = true;
                positionOnConnect(_client);
            } else {
                //_client.positionRelativeToAddOn(this);
            }
            if (shouldFollowClient) {
                follower.offset = transform.position - _client.transform.position;
                follower.target = _client.transform;
            }
        };
        pas.dissolve = delegate (Cog _client) {
        };
        pas.fulfill = delegate (Cog _client) {
        };
        return pas;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        return ClientActions.getDoNothingActions();
    }

    protected override ContractSiteBoss getContractSiteBoss() {
        ContractSiteBoss csb = new ContractSiteBoss(LocatableSiteSetAndCTARSetSetup.connectionSiteLookupFor(this));
        addConnectionSiteEntriesForBackSocketSet(this, csb);
        return csb;
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.CONTROLLER_ADDON_DRIVABLE) {
            return ConnectionSiteAgreement.alignCog(this); // transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    #endregion
}


public interface IControllerAddOnProvider
{
    ControllerAddOn getControllerAddOn();
}



