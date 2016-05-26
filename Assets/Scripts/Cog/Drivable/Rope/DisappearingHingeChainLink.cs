using UnityEngine;
using System.Collections;
using System;

public class DisappearingHingeChainLink : HingeChainLink , ITriggerProxyClient {

    public Collider disappearEntrance;

	[SerializeField]
    public Vector3 enteringDirection = EnvironmentSettings.up;
    protected Renderer renderr;

    public override void Awake() {
        base.Awake();
        renderr = GetComponentInChildren<Renderer>();
    }
    public void FixedUpdate() {
        // Missing Reference Exception here:
        try {
            hide(transform.position.z < disappearEntrance.transform.position.z);
        } catch (Exception e) {
            Destroy(gameObject);
        }
    }

    protected void disappear(Collider other, bool entering) {
        if (other != disappearEntrance) { return; }
        if ((Vector3.Dot(rigidbod.velocity, enteringDirection) > 0f) == entering) {
            hide(!entering);
        }
    }

    public void hide(bool _hide) {
        renderr.enabled = _hide;
    }

    public void proxyTriggerEnter(Collider other) {
        disappear(other, true);
    }

    public void proxyTriggerStay(Collider other) {
    }

    public void proxyTriggerExit(Collider other) {
        disappear(other, false);
    }

    public bool hiding {
        get { return !renderr.enabled; }
    }
}
