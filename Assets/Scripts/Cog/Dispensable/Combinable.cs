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

    public void OnTriggerEnter(Collider other) {
        CombinerSlot slot = other.GetComponent<CombinerSlot>();
        if (slot) {
            slot.addCombinable(this);
        }
    }

    public static bool SameType(Combinable a, Combinable b) {
        return a.GetType() == b.GetType();
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
