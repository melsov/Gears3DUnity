using UnityEngine;
using System.Collections;
using System;

public class Pegboard : MonoBehaviour , ISocketSetContainer
{
    SocketSet backendSocketSet;
    SocketSet frontendSocketSet;

    void Awake() {
        awake();
    }
    protected virtual void awake() {
        backendSocketSet = new SocketSet(GetComponentsInChildren<BackendSocket>());
        frontendSocketSet = new SocketSet(GetComponentsInChildren<FrontendSocket>());        
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
            r.useGravity = false;
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        }
    }

    public SocketSet getBackendSocketSet() {
        return backendSocketSet;
    }

    public SocketSet getFrontendSocketSet() {
        return frontendSocketSet;
    }
}
