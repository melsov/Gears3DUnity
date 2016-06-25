using UnityEngine;
using System.Collections;
using System;

public class ControllerAddOn : AddOn {

    public delegate void SetScalar(float scalar);
    public SetScalar setScalar;

    protected override void awake() {
        base.awake();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }
}

public interface IControllerAddOnProvider
{
    ControllerAddOn getControllerAddOn();
}



