using System;
using UnityEngine;

public class Catapult : Cog {
    public void Awake() {
        HingeJoint hj = GetComponentInChildren<FreeRotationPeg>().getHinge().getHingeJoint();
        hj.useSpring = true;
        JointSpring js = hj.spring;
        js.targetPosition = 15f;
        js.spring = 80f;
        hj.spring = js;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override ContractSiteBoss getContractSiteBoss() {
        throw new NotImplementedException();
    }
}
