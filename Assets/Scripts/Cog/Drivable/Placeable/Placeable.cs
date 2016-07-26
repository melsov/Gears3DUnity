using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Placeable : Drivable {

    #region contract

    protected override ContractNegotiator getContractNegotiator() { return new DuctContractNegotiator(this); }

    public class DuctContractNegotiator : ContractNegotiator
    {
        public DuctContractNegotiator(Cog cog_) : base(cog_) { }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            List<ContractSpecification> result = new List<ContractSpecification>();
            result.Add(new ContractSpecification(CogContractType.PARENT_CHILD, RoleType.CLIENT));
            return result;
        }
    }

    protected override ViableContractLookup getViableContractLookup() { return new ViableDuctContractLookup(this); }

    public class ViableDuctContractLookup : ViableContractLookup
    {
        public ViableDuctContractLookup(Cog cog_) : base(cog_) { }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.PARENT_CHILD, yup);
        }
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        return ProducerActions.getDoNothingActions();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.receive = delegate (Cog cog) { };
            actions.beAbsolvedOf = delegate (Cog cog) {
                transform.position = TransformUtil.SetY(transform.position, YLayer.Layer(GetType())); //TODO: deal with tubes being elevated to higher Ys
            };
        }
        return actions;
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            return ConnectionSiteAgreement.alignAndPushYLayer(transform);
        }
        return ConnectionSiteAgreement.doNothing;
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        return new UniqueClientContractSiteBoss(
            ExclusionarySiteSetClientPair.fromSocketSet(this, _pegboard.getBackendSocketSet())
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
