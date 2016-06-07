using UnityEngine;
using System.Collections.Generic;

public class PartnerBox : MonoBehaviour {

    protected JealousProducerPartner _producer;
    public JealousProducerPartner producer {
        get { return _producer; }
        set { _producer = value; }
    }

    protected ClientPartner _client;
    public ClientPartner client {
        get { return _client; }
        set { _client = value; }
    }

    protected List<ContributingPartner> contributors = new List<ContributingPartner>();

    public bool isClient { get { return client; } }
    public bool isProducer {
        get {
            return producer || contributors.Count > 0;
        }
    }
}
