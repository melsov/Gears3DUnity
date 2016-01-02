using UnityEngine;
using System.Collections;
using System;

public class Switch : ControllerAddOn  {

    protected OnOffIndicator onOffIndicator;
    private RaycastHit rch;

    protected bool on;

    protected override void awake() {
        base.awake();
        onOffIndicator = GetComponentInChildren<OnOffIndicator>();
        onOffIndicator.gameObject.layer = LayerLookup.DragOverride;
    }

    protected override bool vConnectTo(Collider other) {
        if (base.vConnectTo(other)) {
            updateClient();
            return true;
        }
        return false;
    }

// to do: make this independant of indicator?
    protected override void vEndDragOverride(VectorXZ cursorGlobal) {
        // cursor still over indicator?
        OnOffIndicator ooi = RayCastUtil.getColliderUnderCursor(out rch).GetComponent<OnOffIndicator>();
        if (ooi == onOffIndicator) {
            toggleOn();
        }
    }

    protected virtual void toggleOn() {
        on = !on;
        if (onOffIndicator != null) {
            onOffIndicator.state = on;
        }
        updateClient();
    }

    protected virtual void updateClient() {
        if (client != null) {
            setScalar(on ? 1f : 0f);
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
