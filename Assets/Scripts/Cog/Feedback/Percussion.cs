using UnityEngine;
using System.Collections;
using System;

public class Percussion : Instrument {

    [SerializeField]
    protected string audioEntityName;
    [SerializeField]
    protected Color _highlight = Color.green;

    public override ConnectionSiteAgreement.ConnektAction connektActionAsTravellerFor(ContractSpecification specification) {
        throw new NotImplementedException();
    }

    protected override Color getColor() {
        return _highlight;
    }

    protected override ContractSiteBoss getContractSiteBoss() {
        throw new NotImplementedException();
    }

    protected override string getNoteName() {
        return audioEntityName;
    }

}
