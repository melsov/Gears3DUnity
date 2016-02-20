using UnityEngine;
using System.Collections;
using System;

public class Pegboard : MonoBehaviour , ISocketSetContainer
{
    protected SocketSet backendSocketSet;
    protected SocketSet frontendSocketSet;

    void Awake() {
        awake();
    }
    protected virtual void awake() {
    }

    public Transform getTransform() {
        return transform;
    }

    public Rigidbody getRigidbodyWithGravity() {
        Rigidbody r = GetComponent<Rigidbody>();
        if (r == null) {
            r = gameObject.AddComponent<Rigidbody>();
        }
        r.useGravity = true;
        return r;
    }

    public void unsetRigidbodyWithGravity() {
        Rigidbody r = GetComponent<Rigidbody>();
        if (r != null) {
            print("unset rigidbody for: " + gameObject.name);
            r.useGravity = false;
            r.isKinematic = true;
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        }
    }

    public SocketSet getBackendSocketSet() {
        if (backendSocketSet == null) {
            backendSocketSet = new SocketSet(GetComponentsInChildren<BackendSocket>());
        }
        return backendSocketSet;
    }

    public SocketSet getFrontendSocketSet() {
        if (frontendSocketSet == null) {
            frontendSocketSet = new SocketSet(GetComponentsInChildren<FrontendSocket>());
        }
        return frontendSocketSet;
    }
}
