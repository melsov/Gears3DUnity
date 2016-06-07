using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class ProducerPartner : Partner {

    public abstract List<ClientPartner> getClients();
    public abstract void give(ExchangeData exchangeData);
    public abstract void add(ClientPartner _client);

    public override List<Partner> getPartners() {
        List<ClientPartner> cps = getClients();
        List<Partner> result = new List<Partner>(cps.Count);
        foreach(ClientPartner cp in cps) {
            result.Add(cp);
        }
        return result;
    }
}

public struct ExchangeData
{

}
