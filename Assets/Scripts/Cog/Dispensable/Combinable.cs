using UnityEngine;
using System.Collections.Generic;

public abstract class Combinable : MonoBehaviour {

    public Sprite sprite;
    protected Rigidbody rb {
        get { return GetComponent<Rigidbody>(); }
    }
    protected Renderer renderr {
        get {
            Renderer r = GetComponent<Renderer>();
            if (r == null) {
                r = GetComponentInChildren<Renderer>();
            }
            return r;
        }
    }

    void OnTriggerEnter(Collider other) {
        onTriggerEnter(other);
    }

    protected virtual void onTriggerEnter(Collider other) {
        CombinerSlot slot = other.GetComponent<CombinerSlot>();

        if (slot != null) {
            slot.addCombinable(this);
        }
    }

    public void disable() {
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;
        renderr.enabled = false;
    }
    
    public void enable() {
        rb.useGravity = true;
        GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        renderr.enabled = true;
    }
}
