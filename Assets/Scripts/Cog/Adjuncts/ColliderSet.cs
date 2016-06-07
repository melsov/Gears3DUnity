using UnityEngine;
using System.Collections.Generic;

public class ColliderSet : MonoBehaviour {

    public void Awake() {
        getEnabledColliders();
    }

    public bool contains(Collider c) {
        return enabledColliders.ContainsKey(c);
    }

    protected Dictionary<Collider, bool> enabledColliders;
    protected bool needReenableColliders;
    protected Dictionary<Collider, bool> getEnabledColliders() {
        if (enabledColliders == null) {
            enabledColliders = new Dictionary<Collider, bool>();
            foreach(Collider c in GetComponentsInChildren<Collider>()) {
                enabledColliders.Add(c, c.enabled);
            }
        }
        return enabledColliders;
    }

    public void disableForNextFixedFrame() {
        disableColliders(true);
    }

    protected virtual void disableColliders(bool disable) {
        foreach(Collider c in getEnabledColliders().Keys) {
            c.enabled = disable ? false : enabledColliders[c];
        }
        GetComponentInChildren<Rigidbody>().Sleep();
        needReenableColliders = disable;
        if (disable) {
            StartCoroutine(reenableCollidersAfterFixedFrame()); 
        }
    }

    protected System.Collections.IEnumerator reenableCollidersAfterFixedFrame() {
        yield return new WaitForFixedUpdate();
        disableColliders(false);
    }
}
