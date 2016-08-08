using UnityEngine;
using System.Collections.Generic;

/*
 * TODO: Scale. Combinables have a weight (mass of rb)
* */

public abstract class Combinable : MonoBehaviour {

    public Sprite sprite;
    protected Rigidbody rb {
        get { return GetComponent<Rigidbody>(); }
    }
    protected Renderer renderr {
        get {
            return GetComponentInChildren<Renderer>();
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
        beInvisible(true);
    }
    
    public void enable() {
        rb.useGravity = true;
        GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        beInvisible(false);
    }
    
    private void beInvisible(bool yes) {
        foreach (Transform t in GetComponentsInChildren<Transform>()) {
            if(t.GetComponent<Renderer>()) {
                t.GetComponent<Renderer>().enabled = !yes;
            }
        }
    }
}
