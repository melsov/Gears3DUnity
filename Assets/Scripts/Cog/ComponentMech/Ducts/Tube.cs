using UnityEngine;
using System.Collections.Generic;

public class Tube : Duct {

    public float strength = 300f;
    public Transform entrance;
    public Transform exit;
    protected HashSet<Rigidbody> occupants;
    protected float width;

    protected override void awake() {
        base.awake();
        occupants = new HashSet<Rigidbody>();
        //entrance = GetComponentInChildren<TubeEntrance>().transform;
        //exit = GetComponentInChildren<TubeExit>().transform;
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        if (cc != null) {
            width = cc.radius * 2f;
        }
    }

    protected virtual Vector3 down {
        get { return transform.rotation * EnvironmentSettings.gravityDirection; }
    }

    protected Vector3 normal() {
        return new VectorXZ(down).normal.vector3();
    }

    protected virtual Vector3 awayFromEntrance {
        get { return entrance.position - exit.transform.position; }
    }
    protected virtual Vector3 awayFromExit {
        get { return exit.transform.position - entrance.position; }
    }

    void OnTriggerEnter(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.useGravity = false;
        }
        AudioManager.Instance.play(this, AudioLibrary.TubeEnterSoundName);
        pullThrough(other);
    }

    void OnTriggerStay(Collider other) {
        pullThrough(other);
    }
    
    void OnTriggerExit(Collider other) {
        pullThrough(other);
        occupants.Remove(other.GetComponent<Rigidbody>());
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.useGravity = true;
        }
    }

    protected bool closerToEntrance(Transform t) {
        if ((t.position - entrance.position).sqrMagnitude < (t.position - exit.position).sqrMagnitude) {
            return true;
        }
        return false;
    }
    
    protected virtual bool movingTowardsExit(Rigidbody rb) {
        return Vector3.Dot(rb.velocity, down) > 0f;
    }

    protected virtual Vector3 isEntering(Transform t) {
        Vector3 towards, entrPos, exitward, exitPos;
        if (closerToEntrance(t)) {
            entrPos = entrance.position;
            exitPos = exit.position;
            towards = entrance.position - t.position;
            exitward = down; 
        } else {
            entrPos = exit.position;
            exitPos = entrance.position;
            towards = exit.position - t.position;
            exitward = down * -1;
        }
        if(towards.sqrMagnitude > .1f && Vector3.Dot(exitward, towards) > 0f) {
            Vector3 targetPos = entrPos + (exitPos - entrPos) * .1f;
            return targetPos - t.position;
            //if (Vector3.Dot(towards, n) < 0f) {
            //    n *= -1f;
            //}
            //n = Vector3.Lerp(exitward, n, towards.magnitude / (width * .5f));
            //return n;
        }
        return Vector3.zero;
    }

    protected virtual void pullThrough(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (!occupants.Contains(rb)) {
            TESTROCK(rb, Color.red);
            Vector3 enter = isEntering(other.transform);
            if (!enter.Equals(Vector3.zero)) {
                rb.velocity = Vector3.Lerp(enter, rb.velocity.normalized, .2f) * strength;
                return;
            }
        }
        occupants.Add(rb);
        setVelocity(rb);
    }

    protected virtual void setVelocity(Rigidbody rb) {
        if (movingTowardsExit(rb)) {
            rb.velocity = Vector3.Lerp(down, rb.velocity.normalized, .2f) * strength;
        } else {
            rb.velocity = Vector3.Lerp(down * -1f, rb.velocity.normalized, .2f) * strength;
        }
    }

    protected void TESTROCK(Rigidbody rb, Color color) {
        rb.GetComponentInChildren<Renderer>().material.color = color;
    }


}
