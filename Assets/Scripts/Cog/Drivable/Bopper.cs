using UnityEngine;
using System.Collections;
using System;
//TODO: see if we can work around hinge joint finickiness vis-a-vis attaching colliders to its hinge joint
public class Bopper : Drivable {

    public override float driveScalar() {
        return 0f;
    }


    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override ConnectionSiteBoss getConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }
}
