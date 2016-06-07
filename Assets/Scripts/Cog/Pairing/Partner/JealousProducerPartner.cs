using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class JealousProducerPartner : ProducerPartner
{
    private List<ClientPartner> clients = new List<ClientPartner>();
    public override void add(ClientPartner client) {
        if (!clients.Contains(client)) {
            clients.Add(client);
        }
    }

    public override List<ClientPartner> getClients() {
        return clients;
    }

    public override void give(ExchangeData exchangeData) {
        for(int i = 0; i < clients.Count; ++i) {
            clients[i].receive(exchangeData);
        }
    }
}

