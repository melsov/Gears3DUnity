using UnityEngine;
using System.Collections;
using System;

public class Switch : ControllerAddOn  {

    public bool isReverseSwitch;

    protected OnOffIndicator onOffIndicator;
    private RaycastHit rch;

    protected ISwitchStateToggle on;


    protected override void awake() {
        base.awake();
        if (isReverseSwitch) {
            on = new ForwardReverseSwitchState(SwitchState.ON);
        } else {
            on = new OnOffSwitchState(SwitchState.ON);
        }
        UnityEngine.Assertions.Assert.IsTrue(on != null, "wait on (switch state toggle) is null?");
        onOffIndicator = GetComponentInChildren<OnOffIndicator>();
    }


    public override bool connectToClient(Cog cog) {
        if (base.connectToClient(cog)) {
            updateClient();
            return true;
        }
        return false;
    }

    // to do: make this independant of indicator?
    protected override void vEndDragOverride(VectorXZ cursorGlobal) {
        // cursor still over indicator?
        Collider underCursor = RayCastUtil.getColliderUnderCursor(out rch);
        if (underCursor == null) {
            return;
        }
        OnOffIndicator ooi = underCursor.GetComponent<OnOffIndicator>();
        if (ooi == onOffIndicator) {
            toggleOn();
        }
    }

    protected virtual void toggleOn() {
        on.nextState();
        if (onOffIndicator != null) {
            onOffIndicator.state = on.getState();
        }
        updateClient();
    }

    protected virtual void updateClient() {
        print("up client set Scalar null ? " + (setScalar == null));
        if (/*client != null &&*/  setScalar != null && on != null) {
            setScalar((int)on.getState());
        }
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
}

public struct OnOffSwitchState : ISwitchStateToggle
{
    private SwitchState state; // = SwitchState.OFF;
    
    public OnOffSwitchState(SwitchState state_) {
        state = state_;
    }

    public SwitchState getState() {
        return state;
    }

    public void nextState() {
        if (state == SwitchState.OFF) {
            state = SwitchState.ON;
        } else {
            state = SwitchState.OFF;
        }
    }

    public void setState(SwitchState _state) {
        state = _state;
    }
}

public struct ForwardReverseSwitchState : ISwitchStateToggle
{
    private SwitchState state;
    public ForwardReverseSwitchState(SwitchState state_) {
        state = state_;
    }

    public SwitchState getState() {
        return state;
    }

    public void nextState() {
        if (state == SwitchState.REVERSE) {
            state = SwitchState.ON;
        } else {
            state = SwitchState.REVERSE;
        }
    }

    public void setState(SwitchState _state) {
        state = _state;
    }

}
