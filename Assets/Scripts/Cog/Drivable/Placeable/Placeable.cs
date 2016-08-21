using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Placeable : Drivable {

    #region contract

    protected override ContractNegotiator getContractNegotiator() { return new PlaceableContractNegotiator(this); }

    public class PlaceableContractNegotiator : ContractNegotiator
    {
        public PlaceableContractNegotiator(Cog cog_) : base(cog_) { }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            result.Add(new ContractSpecification(CogContractType.PARENT_CHILD, RoleType.CLIENT));
            return result;
        }
    }

    protected override ViableContractLookup getViableContractLookup() { return new ViablePlaceableContractLookup(this); }

    public class ViablePlaceableContractLookup : ViableContractLookup
    {
        public ViablePlaceableContractLookup(Cog cog_) : base(cog_) { }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.PARENT_CHILD, yup);
            asProducerLookup.Add(CogContractType.PARENT_CHILD, yup);
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        return base.producerActionsFor(client, specification);
    }

    protected virtual void onReceiveProducer(Cog producer, ContractSpecification specification) { }
    protected virtual void onBeAbsolvedOfProducer(Cog producer, ContractSpecification specification) { }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.receive = delegate (Cog cog) {
                specification.connectionSiteAgreement.clientSite.setDecoration(SharedPrefabs.Instance.createSocket());
            };
            actions.beAbsolvedOf = delegate (Cog cog) {
                transform.position = TransformUtil.SetY(transform.position, YLayer.Layer(GetType())); //TODO: deal with tubes being elevated to higher Ys
                specification.connectionSiteAgreement.clientSite.destroyDecoration();
            };
        }
        return actions;
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            return ConnectionSiteAgreement.alignAndPushYLayer(this); //transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        Dictionary<CTARSet, SiteSet> otherSites = new Dictionary<CTARSet, SiteSet>();
        /* Add front sockets as sites, if any */
        if (_pegboard.getFrontendSocketSet().sockets.Length > 0) {
            KeyValuePair<CTARSet, SiteSet> sitesPair = PairCTARSiteSet.fromSocketSet(this, _pegboard.getFrontendSocketSet(), RigidRelationshipConstraint.CAN_ONLY_BE_PARENT);
            otherSites.Add(sitesPair.Key, sitesPair.Value);
        }
        return new UniqueClientContractSiteBoss(
            ExclusionarySiteSetClientPair.fromSocketSet(this, _pegboard.getBackendSocketSet()),
            otherSites
        );
    }

    #endregion

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
	
}
