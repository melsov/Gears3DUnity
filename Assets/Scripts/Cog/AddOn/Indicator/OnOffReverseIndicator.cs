using UnityEngine;
using System.Collections;

public class OnOffReverseIndicator : Indicator {

    protected SwitchState _state;
    protected IOnOffIndicatorProxy proxy;

    protected override void awake() {
        base.awake();
        proxy = GetComponentInChildren<IOnOffIndicatorProxy>();
    }

    public override void OnEnable() {
        base.OnEnable();
        registerWithOSS(true);
    }

    public override void OnDisable() {
        base.OnDisable();
        registerWithOSS(false);
    }

    private void registerWithOSS(bool register) {
        IObservableSwitchStateProvider iossp = GetComponentInParent<IObservableSwitchStateProvider>();
        if(iossp != null) {
            if (register) {
                iossp.getObservableSwitchState().register(handleObservableSwitchStateChange);
                handleObservableSwitchStateChange(iossp.getObservableSwitchState().state);
            } else {
                iossp.getObservableSwitchState().unregister(handleObservableSwitchStateChange);
            }
        }
        
    }

    private void handleObservableSwitchStateChange(SwitchState s) {
        state = s;
    }

    public SwitchState state {
        set {
            _state = value;
            updateIndicator();
        }
    }

    protected override void updateIndicator() {
        if (proxy != null) {
            proxy.acceptState(_state);
        }
        if (!_renderer || !_renderer.material) { return; }
        if(!_renderer.material.HasProperty("_Color")) { Debug.LogError("need an material with color property for this indicator. in " + Cog.FindCog(transform).name); return; }
        if (_state == SwitchState.ON) {
            _renderer.material.SetColor("_Color", onColor);
        } else if (_state == SwitchState.REVERSE) {
            _renderer.material.SetColor("_Color", reverseColor);
        } else {
            _renderer.material.SetColor("_Color", offColor);
        }
    }
}

public interface IOnOffIndicatorProxy
{
    void acceptState(SwitchState state);
}
