using UnityEngine;
using System.Collections;
using System;
//TODO: see if we can work around hinge joint finickiness vis-a-vis attaching colliders to its hinge joint
public class Bopper : Drivable {
    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override float driveScalar() {
        return 0f;
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
