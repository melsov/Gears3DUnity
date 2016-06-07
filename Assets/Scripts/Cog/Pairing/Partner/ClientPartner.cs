using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ClientPartner : Partner {

    private WeakReference _producer;
    public ProducerPartner producer {
        get {
            if (_producer != null) {
                return (ProducerPartner)_producer.Target;
            }
            return null;
        }
        set {
            _producer = new WeakReference(value);
        }
    }

    public override List<Partner> getPartners() {
        return new List<Partner>() { producer };
    }

    public void receive(ExchangeData ed) {
        
    }
}
