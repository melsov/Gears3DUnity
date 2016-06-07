using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ContributingPartner : ProducerPartner
{
    public ClientPartner client {
        get;
        set;
    }

    public override void add(ClientPartner _client) {
        this.client = _client;
    }

    public override List<ClientPartner> getClients() {
        return new List<ClientPartner>() { client };
    }

    public override void give(ExchangeData exchangeData) {
        client.receive(exchangeData);
    }
}
