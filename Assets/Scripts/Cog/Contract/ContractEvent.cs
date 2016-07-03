using UnityEngine;
using System.Collections;
using System;

public class ContractEvent {
    private WeakReference _cm;
    private Cog.ContractManager cm {
        get { return (Cog.ContractManager)_cm.Target; }
    }
    private Cog cog {
        get { return cm.cog; }
    }

    public ContractEvent(Cog.ContractManager cm_) {
        _cm = new WeakReference(cm_);
    }


}

public class ContractEventActions
{
    public delegate void ExistingContractAction(CogContract cc, ContractEventType cet);
    public ExistingContractAction mouseDown;
    public ExistingContractAction mouseUp;
}

public enum ContractEventType
{
    MOUSE_DOWN, 
    MOUSE_UP
}