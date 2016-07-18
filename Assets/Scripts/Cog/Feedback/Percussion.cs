using UnityEngine;
using System.Collections;
using System;

public class Percussion : Instrument {

    [SerializeField]
    protected string audioEntityName;
    [SerializeField]
    protected Color highlight = Color.green;

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override Color getColor() {
        return highlight;
    }

    protected override ContractSiteBoss getConnectionSiteBoss() {
        throw new NotImplementedException();
    }

    protected override string getNoteName() {
        return audioEntityName;
    }

}
