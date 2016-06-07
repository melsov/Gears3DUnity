using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class Partner : MonoBehaviour {

    protected WeakReference _partnerBox = new WeakReference(null);
    public PartnerBox partnerBox {
        get {
            return (PartnerBox) _partnerBox.Target;
        }
        set {
            _partnerBox = new WeakReference(value);
        }
    }
    public abstract List<Partner> getPartners();

    public virtual void sendEvent(PartnerEventInfo pei) {
        foreach(Partner p in getPartners()) {
            p.receiveEvent(pei);
        }
    }

    public virtual void receiveEvent(PartnerEventInfo pei) {

    }
    
}

public struct PartnerEventInfo
{
    
    
}
