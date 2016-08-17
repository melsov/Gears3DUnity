using UnityEngine;
using System.Collections;
using System;

public abstract class DriverDriven : Drivable {

    protected override UniqueClientContractSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    // Use this for initialization
    public override void Start() {
        base.Start();
    }
    
}
