using UnityEngine;
using System.Collections;

public class OnOffIndicator : Indicator {

    protected SwitchState _state;
    public SwitchState state {
        set {
            _state = value;
            updateIndicator();
        }
    }

    protected override void updateIndicator() {
        if (_state == SwitchState.ON) {
            _renderer.material.SetColor("_Color", onColor);
        } else if (_state == SwitchState.REVERSE) {
            _renderer.material.SetColor("_Color", reverseColor);
        } else {
            _renderer.material.SetColor("_Color", offColor);
        }
    }
}
