using UnityEngine;
using System.Collections;

public class OnOffIndicator : Indicator {

    protected bool _state;
    public bool state {
        set {
            _state = value;
            updateIndicator();
        }
    }

    protected override void updateIndicator() {
        if (_state) {
            _renderer.material.SetColor("_Color", onColor);
        } else {
            _renderer.material.SetColor("_Color", offColor);
        }
    }
}
