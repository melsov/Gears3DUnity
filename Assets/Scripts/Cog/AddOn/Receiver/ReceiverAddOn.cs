using UnityEngine;
using System.Collections;
using System;

public class ReceiverAddOn : AddOn {

    protected float _input;
    public virtual float input {
        get;
        set;
    }

    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }
}
