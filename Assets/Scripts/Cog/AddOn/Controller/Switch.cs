using UnityEngine;
using System.Collections;
using System;

//TODO: Extender switches, logic switches: try making an And Switch first, then abstract
public interface ISwitchStateProvider
{
    SwitchState currentState();
}

public class Switch : ControllerAddOn , ISwitchStateProvider , IObservableSwitchStateProvider {

    public bool isReverseSwitch;

    //protected OnOffReverseIndicator onOffIndicator;
    private RaycastHit rch;

    private ISwitchStateToggle _on;
    protected ISwitchStateToggle on {
        get {
            if (_on == null) {
                if (isReverseSwitch) {
                    _on = new ForwardReverseSwitchState(SwitchState.ON);
                } else {
                    _on = new OnOffSwitchState(SwitchState.ON);
                }
            }
            return _on;
        }
    }
    public ObservableSwitchState getObservableSwitchState() {
        return on.getObservableSwitchState();
    }

    protected override void awake() {
        base.awake();
        //if (isReverseSwitch) {
        //    on = new ForwardReverseSwitchState(SwitchState.ON);
        //} else {
        //    on = new OnOffSwitchState(SwitchState.ON);
        //}
        //onOffIndicator = GetComponentInChildren<OnOffReverseIndicator>();
    }

    public override void Start() {
        base.Start();
        updateIndicator();
    }

    public override bool connectToClient(Cog cog) {
        if (base.connectToClient(cog)) {
            updateClient();
            return true;
        }
        return false;
    }

    // to do: make this independant of indicator?
    protected override void vEndDragOverride(CursorInfo ci) { // VectorXZ cursorGlobal) {
        // cursor still over indicator?
        Collider underCursor = RayCastUtil.getColliderUnderCursor(out rch);
        if (underCursor == null) {
            return;
        }
        //OnOffReverseIndicator ooi = underCursor.GetComponent<OnOffReverseIndicator>();
        //if (ooi == onOffIndicator) {
        //    toggle();
        //}
    }

    protected virtual void toggle() {
        on.nextState();
        //updateIndicator();
        updateClient();
    }

    protected void updateIndicator() {
        //if (onOffIndicator != null) {
        //    onOffIndicator.state = on.getState();
        //}
    }

    protected virtual void updateClient() {
        if (setScalar != null && on != null) {
            setScalar((int)on.getState());
        }
    }

    public SwitchState currentState() {
        return on.getState();
    }

    #region proxy trigger disabled
    /*
        protected Vector3 toggleDirection {
            get {
                if (onOffIndicator is LightSwitchIndicator) {
                    return EnvironmentSettings.gravityDirection * (on ? -1f : 1f);
                } else {
                    return (transform.position - onOffIndicator.transform.position).normalized;
                }
            }
        }

        public void proxyTriggerEnter(Collider other) {
            // for 'always toggling' switches velocity doesn't matter
            // get velocity of collider
            Rigidbody rbOther = other.GetComponent<Rigidbody>();
            print("proxy T enter");
            if (rbOther == null) {
                return;
            }
            float dotP = new VectorXZ(rbOther.velocity.normalized).dot(new VectorXZ(toggleDirection));
            print("dot p is: " + dotP);
            if (dotP > 0f) {
                print("proxy trigger toggled");
                toggleOn();
            }
        }

        public void proxyTriggerStay(Collider other) {
        }

        public void proxyTriggerExit(Collider other) {
        }
    */
    #endregion
}

public class SwitchStateHelper
{
    public static SwitchState stateFor(float f) {
        if (f > 0f) {
            return SwitchState.ON;
        } else if (f < 0f) {
            return SwitchState.REVERSE;
        } else {
            return SwitchState.OFF;
        }
    }

    public static SwitchState stateFor(bool b) {
        return b ? SwitchState.ON : SwitchState.OFF;
    }

    public static float floatFor(SwitchState s) {
        return (float)s;
    }
}

public enum SwitchState
{
    REVERSE = -1, OFF, ON
};

public interface ISwitchStateToggle
{
    void nextState();
    SwitchState getState();
    void setState(SwitchState state);
    ObservableSwitchState getObservableSwitchState();
}

public struct OnOffSwitchState : ISwitchStateToggle
{
    private readonly ObservableSwitchState state; 

    public OnOffSwitchState(SwitchState state_) {
        state = new ObservableSwitchState(state_); // state_;
    }

    public ObservableSwitchState getObservableSwitchState() {
        return state;
    }

    public SwitchState getState() {
        return state.state;
    }

    public void nextState() {
        if (state.state == SwitchState.OFF) {
            state.state = SwitchState.ON;
        } else {
            state.state = SwitchState.OFF;
        }
    }

    public void setState(SwitchState _state) {
        state.state = _state;
    }
}

public struct ForwardReverseSwitchState : ISwitchStateToggle
{
    private readonly ObservableSwitchState state;
    public ForwardReverseSwitchState(SwitchState state_) {
        state = new ObservableSwitchState(state_);
    }

    public ObservableSwitchState getObservableSwitchState() {
        return state;
    }

    public SwitchState getState() {
        return state.state;
    }

    public void nextState() {
        if (state.state == SwitchState.REVERSE) {
            state.state = SwitchState.ON;
        } else {
            state.state = SwitchState.REVERSE;
        }
    }

    public void setState(SwitchState _state) {
        state.state = _state;
    }

}

public class ObservableSwitchState
{
    protected SwitchState _state;
    public virtual SwitchState state {
        get {
            return _state;
        }
        set {
            if (_state != value) {
                _state = value;
                if(onValueChange != null) {
                    onValueChange(_state);
                }
            }
        }
    }

    public delegate void OnValueChange(SwitchState state);
    protected OnValueChange onValueChange;

    public void register(OnValueChange onValueChange) {
        this.onValueChange += onValueChange;
    }

    public void unregister(OnValueChange onValueChange) {
        this.onValueChange -= onValueChange;
    }

    public ObservableSwitchState(SwitchState state) {
        _state = state;
    }
}

public class ObservablePerFloatSwitchState : ObservableSwitchState
{
    private float storage;
    public float Value {
        get { return storage; }
        set {
            storage = value;
            _state = storage < 0f ? SwitchState.REVERSE : (storage > 0f ? SwitchState.ON : SwitchState.OFF);
            if(onValueChange != null) {
                onValueChange(_state);
            }
        }
    }

    public override SwitchState state {
        get {
            return  storage < 0f ? SwitchState.REVERSE : (storage > 0f ? SwitchState.ON : SwitchState.OFF);
        }
        set {
            Debug.LogError("don't want to directly set the state of an ObservablePerFloatSwitchState");
        }
    }

    public ObservablePerFloatSwitchState(float f, SwitchState s) : base(s) {
        this.storage = f;
    }
}
