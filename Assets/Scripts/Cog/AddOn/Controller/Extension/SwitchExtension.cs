using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public interface IObservableSwitchStateProvider
{
    ObservableSwitchState getObservableSwitchState();
}
//TODO: make SwitchExtension OnOffIndicator aware
public abstract class SwitchExtension : ExtensionControllerAddOn , ISwitchStateProvider, IObservableSwitchStateProvider {

    private ObservableSwitchState state = new ObservableSwitchState(SwitchState.OFF);
    
    protected IEnumerable<ISwitchStateProvider> producerSSPs() {
        foreach(ControllerAddOn cao in producerCAOs()) { yield return (ISwitchStateProvider)cao; }
    }

    protected override bool canBeClientOf(Cog cog) {
        return cog is ISwitchStateProvider;
    }

    protected override void updateState(ControllerAddOn ignore) {
        state.state = calculateState((ISwitchStateProvider)ignore);
        updateClients(state.state);
    }

    protected virtual float scalarForState(SwitchState state) {
        return SwitchStateHelper.floatFor(state);
    }
    private void updateClients(SwitchState state) {
        if(setScalar == null) { print("scalar null for " + name); return; }
        setScalar(scalarForState(state));
    }

    protected abstract SwitchState calculateState(ISwitchStateProvider ignore);

    public SwitchState currentState() {
        return calculateState(null);
    }

    public ObservableSwitchState getObservableSwitchState() {
        return state;
    }
}
