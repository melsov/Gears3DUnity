using UnityEngine;
using System.Collections;
using System;

public class NotGate : SwitchExtension
{
    protected override int maxClients {
        get {
            return 3;
        }
    }

    protected override int maxProducers {
        get {
            return 1;
        }
    }

    protected ISwitchStateProvider producer {
        get {
            foreach (ISwitchStateProvider s in producerSSPs()) {
                return s;
            }
            return null;
        }
    }

    protected override SwitchState calculateState(ISwitchStateProvider ignore) {
        if (producer == null || producer == ignore) { return SwitchState.OFF; }
        return producer.currentState() == SwitchState.ON ? SwitchState.OFF : SwitchState.ON;
    }
}
