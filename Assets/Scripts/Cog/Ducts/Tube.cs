using UnityEngine;
using System.Collections;

public class Tube : Duct {

    public float strength = 300f;
    protected Transform entrance;
    protected Transform exit;

    void Awake() {
        awake();
    }

    protected virtual void awake() {
        entrance = GetComponentInChildren<TubeEntrance>().transform;
        exit = GetComponentInChildren<TubeExit>().transform;
    }

    protected virtual Vector3 down {
        get { return transform.rotation * (Vector3.up * -1f); }
    }

    private Vector3 normal() {
        return new VectorXZ(down).normal.vector3();
    }

    private Vector3 awayFromEntrance {
        get { return entrance.position - exit.transform.position; }
    }

    void OnTriggerEnter(Collider other) {
        pullThrough(other);
    }

    void OnTriggerStay(Collider other) {
        pullThrough(other);
    }
    
    void OnTriggerExit(Collider other) {
        pullThrough(other);
    }

//TODO: make the math better
    protected virtual Vector3 isEntering(Transform t) {
        Vector3 towards = entrance.position - t.position;
        if(towards.sqrMagnitude > .1f && Vector3.Dot(awayFromEntrance, towards) < 0f) {
            Vector3 n = normal();
            if (Vector3.Dot(towards, n) < 0f) {
                n *= -1f;
            }
            n = Vector3.Lerp(down, n, towards.magnitude * .5f);
            return n;
        }
        return Vector3.zero;
    }

    protected virtual void pullThrough(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 enter = isEntering(other.transform);
        if (!enter.Equals(Vector3.zero)) { 
            rb.velocity = Vector3.Lerp(enter, rb.velocity.normalized, .2f) * strength;
            return;
        }
        setVelocity(rb);
    }

    protected virtual void setVelocity(Rigidbody rb) {
        rb.velocity = Vector3.Lerp(down, rb.velocity.normalized, .2f) * strength;
    }


}
