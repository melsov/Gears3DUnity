﻿using UnityEngine;
using System.Collections.Generic;

public class Tube : Duct {

    public float strength = 300f;
    protected Transform entrance;
    protected Transform exit;
    protected HashSet<Rigidbody> occupants;
    protected float width;

    void Awake() {
        awake();
    }

    protected virtual void awake() {
        occupants = new HashSet<Rigidbody>();
        entrance = GetComponentInChildren<TubeEntrance>().transform;
        exit = GetComponentInChildren<TubeExit>().transform;
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        if (cc != null) {
            width = cc.radius * 2f;
        }
    }

    protected virtual Vector3 down {
        get { return transform.rotation * EnvironmentSettings.gravityDirection; }
    }

    private Vector3 normal() {
        return new VectorXZ(down).normal.vector3();
    }

    protected Vector3 awayFromEntrance {
        get { return entrance.position - exit.transform.position; }
    }
    private Vector3 awayFromExit {
        get { return exit.transform.position - entrance.position; }
    }

    void OnTriggerEnter(Collider other) {
        AudioManager.Instance.play(this, AudioLibrary.TubeEnterSoundName);
        pullThrough(other);
    }

    void OnTriggerStay(Collider other) {
        pullThrough(other);
    }
    
    void OnTriggerExit(Collider other) {
        pullThrough(other);
        occupants.Remove(other.GetComponent<Rigidbody>());
    }

    private bool closerToEntrance(Transform t) {
        if ((t.position - entrance.position).sqrMagnitude < (t.position - exit.position).sqrMagnitude) {
            return true;
        }
        return false;
    }
    
    protected virtual bool movingTowardsExit(Rigidbody rb) {
        return Vector3.Dot(rb.velocity, down) > 0f;
    }

    protected virtual Vector3 isEntering(Transform t) {
        Vector3 towards, reverse, n, exitward;
        n = normal();
        if (closerToEntrance(t)) {
            towards = entrance.position - t.position;
            reverse = awayFromEntrance;
            exitward = down;
        } else {
            towards = exit.position - t.position;
            reverse = awayFromExit;
            exitward = down * -1;
        }
        if(towards.sqrMagnitude > .1f && Vector3.Dot(reverse, towards) < 0f) {
            if (Vector3.Dot(towards, n) < 0f) {
                n *= -1f;
            }
            n = Vector3.Lerp(exitward, n, towards.magnitude / (width * .5f));
            return n;
        }
        return Vector3.zero;
    }

    protected virtual void pullThrough(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (!occupants.Contains(rb)) {
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


}
