using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
//TODO: see if we can work around hinge joint finickiness vis-a-vis attaching colliders to its hinge joint
public class Bopper : Drivable {

    public override float driveScalar() {
        return 0f;
    }


    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

 #region contract

    protected override ContractNegotiator getContractNegotiator() {
        return new BopperContractNegotiator(this);
    }

    public class BopperContractNegotiator : ContractNegotiator
    {
        public BopperContractNegotiator(Cog cog_) : base(cog_) {
        }

        protected override List<ContractSpecification> orderedContractPreferencesAsOfferer(Cog cogForTypeWorkaround) {
            return new List<ContractSpecification>() {
                new ContractSpecification(CogContractType.PARENT_CHILD, RoleType.CLIENT)
            };
        }
    }

    protected override ViableContractLookup getViableContractLookup() {
        return new ViableBopperContractLookup(this);
    }

    public class ViableBopperContractLookup : ViableContractLookup
    {
        protected Bopper bopper { get { return (Bopper)cog; } }

        public ViableBopperContractLookup(Cog cog_) : base(cog_) {
        }

        protected override void setupLookups() {
            asClientLookup.Add(CogContractType.PARENT_CHILD, delegate (Cog other) {
                return ((Drivable)other).hasOpenFrontendSockets();
            });
        }
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        ClientActions actions = ClientActions.getDoNothingActions();
        if (specification.contractType == CogContractType.PARENT_CHILD) {
            actions.receive = delegate (Cog _producer) {
            };
            actions.beAbsolvedOf = delegate (Cog _producer) {
                disconnectBackendSockets();
            };
        }
        return actions;
    }

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        return new UniqueClientContractSiteBoss(
            ExclusionarySiteSetClientPair.fromSocketSet(this, _pegboard.getBackendSocketSet()));
    }




    #endregion
}
