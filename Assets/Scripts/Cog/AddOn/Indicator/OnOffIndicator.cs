using UnityEngine;
using System.Collections;

public class OnOffIndicator : Indicator {

    protected SwitchState _state;
    protected IOnOffIndicatorProxy proxy;

    public void Awake() {
        proxy = GetComponentInChildren<IOnOffIndicatorProxy>();
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
        if (_renderer == null || _renderer.material == null) { return; }
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
