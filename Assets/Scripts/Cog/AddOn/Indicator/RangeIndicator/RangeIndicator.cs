using UnityEngine;
using System.Collections;
using System;

public abstract class RangeIndicator : MonoBehaviour {

    private ObservableFloat observableFloat;

    public virtual void Awake() { }

    public void OnEnable() {
        if (observableFloat == null) {
            observableFloat = GetComponentInParent<IObservableFloatProvider>().getObservableFloat();
        }
        observableFloat.register(setNumber);
    }

    private void setNumber(float f) {
        updateDisplay(f);
    }

    public void OnDisable() {
        if (observableFloat == null) return;
        observableFloat.unregister(setNumber);
    }

    protected abstract void updateDisplay(float f);
}
