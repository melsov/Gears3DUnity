using UnityEngine;
using System.Collections;
using System;

public class GearSwitch : GearDrivenMechanism, IControllerAddOnProvider  {

    protected ProxySwitch proxySwitch;
    protected RotationObserver rotationObserver;

    protected override void awake() {
        base.awake();
        proxySwitch = GetComponentInChildren<ProxySwitch>();
        proxySwitch.shouldFollowClient = false;
        rotationObserver = gearMesh.GetComponent<RotationObserver>();
        if (rotationObserver == null) {
            rotationObserver = gearMesh.gameObject.AddComponent<RotationObserver>();
        }
        rotationObserver.intervals = 2;
        rotationObserver.notifyRotation = onRotationEvent;
        forcePermanentEarmarkedParentChildContract(proxySwitch, Earmark.A);
    }

    public void onRotationEvent(RotationEvent re) {
        proxySwitch.doToggle();
    }

    /*
     * IControllerAddOnProvider method
     * */
    public ControllerAddOn getControllerAddOn() {
        return proxySwitch;
    }

    protected override void updateMechanism(Drive drive) {
        
    }

    //protected override void vTriggerExit(Collider other) {
    //    base.vTriggerExit(other);
    //    IAddOnClient c = FindInCog<IAddOnClient>(other.transform);
    //    if (c == proxySwitch.client) {
    //        proxySwitch.disconnect();
    //    }
    //}


}
