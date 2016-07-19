using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Pegboard : MonoBehaviour, ISocketSetContainer
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
        return getSocketSet<BackendSocket>(backendSocketSet);
        //if (backendSocketSet == null) {
        //    backendSocketSet = new SocketSet(GetComponentsInChildren<BackendSocket>());
        //    if (backendSocketSet == null) {
        //        backendSocketSet = new SocketSet(null);
        //    }
        //}
        //return backendSocketSet;
    }

    public SocketSet getFrontendSocketSet() {
        return getSocketSet<FrontendSocket>(frontendSocketSet);
        //if (frontendSocketSet == null) {
        //    frontendSocketSet = new SocketSet(GetComponentsInChildren<FrontendSocket>());
        //    if (frontendSocketSet == null) {
        //        frontendSocketSet = new SocketSet(null);
        //    }
        //}
        //return frontendSocketSet;
    }

    private SocketSet getSocketSet<T>(SocketSet _socketSet) where T : Socket {
        if (_socketSet == null) {
            _socketSet = new SocketSet(TransformUtil.FindInCogExcludingChildCogs<T>(Cog.FindCog(transform)).ToArray());
        }
        return _socketSet;
    }

    public Socket closestOppositeEndSocket(Socket socket) {
        if (getFrontendSocketSet().contains(socket)) {
            return getBackendSocketSet().getSocketClosestTo(socket.transform.position);
        } else {
            return getFrontendSocketSet().getSocketClosestTo(socket.transform.position);
        }
    }
}
