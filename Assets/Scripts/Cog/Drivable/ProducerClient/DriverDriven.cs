using UnityEngine;
using System.Collections;
using System;

public abstract class DriverDriven : Drivable {

    protected override UniqueClientConnectionSiteBoss getUniqueClientSiteConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
