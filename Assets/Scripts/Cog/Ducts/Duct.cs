﻿using UnityEngine;
using System.Collections;
using System;

public class Duct : Cog
{
    public override ClientActions clientActionsFor(Cog producer, ContractSpecification specification) {
        throw new NotImplementedException();
    }

    public override ProducerActions producerActionsFor(Cog client, ContractSpecification specification) {
        throw new NotImplementedException();
    }
}
