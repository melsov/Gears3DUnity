﻿using UnityEngine;
using System.Collections.Generic;

public abstract class Drivable : MonoBehaviour , ICursorAgentClient , ISocketSetContainer
{
    protected AngleStep _angleStep;
    protected AngleStep angleStep {
        get {
            if (isOnAxel()) {
                return connectedSocket.axel.angleStep;
            }
            return _angleStep;
        }
    }
    protected Socket connectedSocket;
    protected SocketSet backendSocketSet;
    protected SocketSet frontendSocketSet;

    protected Drivable _driver;
    protected List<Drivable> drivables = new List<Drivable>();

    protected float radius {
        get { return  GetComponent<CapsuleCollider>().radius * transform.localScale.x; }
    }

    void Awake () {
        awake();
	}
    protected virtual void awake() {
        backendSocketSet = new SocketSet(GetComponentsInChildren<BackendSocket>());
        frontendSocketSet = new SocketSet(GetComponentsInChildren<FrontendSocket>());
    }

    public virtual bool isOnAxel() {
        return connectedSocket != null && connectedSocket.axel != null && transform.parent != null;
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


    protected float axisRotation {
        get { return transform.rotation.eulerAngles.y; }
    }

    protected virtual void updateAngleStep() {
        if (isOnAxel()) {
            _angleStep = connectedSocket.axel.angleStep;
        } else {
            _angleStep.update(axisRotation);
        }
    }

    public abstract float driveScalar();

    public virtual float radiusInDirection(Vector3 direction) {
        return radius;
    }

	void Update () {
        update();
	}

    protected virtual void update() {
        updateAngleStep();
        foreach (Drivable dr in drivables) {
            dr.receiveDrive(new Drive(driveScalar()));
        }
    }

    protected virtual void setSocketClosestToAxel(Axel axel) {
        connectedSocket = backendSocketSet.getOpenChildSocketClosestTo(axel.transform.position, axel.pegIsParentRotationMode); 
        setSocketToPeg(connectedSocket, axel);
    }

    protected virtual void setSocketToPeg(Socket socket, Peg peg) {
        socket.drivingPeg = peg;
        //TransformUtil.ParentToAndAlignXZ(transform, peg.transform, socket.transform);
    }

    public virtual void addDrivable(Drivable _drivable) {
        if (!drivables.Contains(_drivable))
            drivables.Add(_drivable);
    }

    public virtual void removeDrivable(Drivable _drivable) {
        while(drivables.Contains(_drivable)) {
            drivables.Remove(_drivable);
            _drivable.disconnectFromDriver();
        }
    }

    //iCursorAgentClient
    public void disconnect() {
        vDisconnect();
    }

    protected virtual void vDisconnect() {
        detachFromAxel();
        if (_driver != null)
            _driver.removeDrivable(this);
        disconnectFromDriver();
        disconnectSockets();
    }

    protected virtual void disconnectSockets() {
        foreach (Socket soc in getBackendSocketSet().sockets) {
            soc.disconnectDrivingPeg();
        }
    }

    public virtual void disconnectFromDriver() {
        _driver = null;
    }

    protected virtual void detachFromAxel() {
        transform.SetParent(null);
        connectedSocket = null;
    }

    public bool connectTo(Collider other) {
        return vConnectTo(other);
    }

    protected virtual bool vConnectTo(Collider other) {
        // Connect to any peg or axel
        Socket aSocket;
        Peg peg = backendSocketSet.closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            setSocketToPeg(aSocket, peg);
            return true;
        }
        return false;
    }

    public void suspendConnection() {
        vSuspendConnection();
    }

    protected virtual void vSuspendConnection() {
        //suspended = true;
    }

    public Collider shouldPreserveConnection() {
        return vShouldPreserveConnection();
    }

    protected virtual Collider vShouldPreserveConnection() {
        if (_driver != null) {
            Collider result = _driver.GetComponent<Collider>();
            if (result != null && isInConnectionRange(result)) {
                return result;
            }
        }
        if (transform.parent != null && transform.parent.GetComponent<Axel>() != null) {
            Collider result = transform.parent.GetComponent<Collider>();
            if (result != null && isInConnectionRange(result)) {
                return result;
            }
        }
        return null;
    }

    //TODO: make this less 'hacky' when we make a parent Drivable class (replace interface)
    protected virtual bool isInConnectionRange(Collider other) {
        if (other == null) {
            return false;
        }
        VectorXZ globalXZ = new VectorXZ(other.transform.position);
        CapsuleCollider cc = other.GetComponent<CapsuleCollider>();
        if (cc != null) {
            float centerDistance = (globalXZ - new VectorXZ(transform.position)).vector2.magnitude;
            print(centerDistance < cc.radius * other.transform.localScale.x + GetComponent<CapsuleCollider>().radius * transform.localScale.x);
            return centerDistance < cc.radius * other.transform.localScale.x + GetComponent<CapsuleCollider>().radius * transform.localScale.x;

        }
        return false;
    }

    public virtual bool isDriven() {
        if (isOnAxel()) {
            return true;
        }
        if (_driver != null && (MonoBehaviour)_driver != this) {
            return _driver.isDriven(); //TODO: protect (more) against infinite recursion?
        } 
        return false; 
    }

    public abstract Drive receiveDrive(Drive drive);

    public virtual void positionRelativeTo(Drivable _driver) {
        if (_driver != null) {
            Vector3 relPos = transform.position - _driver.transform.position;
            relPos = relPos.normalized * (radius + _driver.radius - .01f); // fudge a little to keep gear inside
            transform.position = _driver.transform.position + relPos;
            return;
        }
    }


    protected Axel getAxel(Collider other) {
        Axel anAxel = other.gameObject.GetComponent<Axel>();
        if (anAxel == null) {
            anAxel = other.gameObject.GetComponentInParent<Axel>();
        }
        if (anAxel != null) {
            if (anAxel.occupiedByChild) return null;
        }
        return anAxel;
    }
}

public interface ISocketSetContainer
{
    Transform getTransform();
    Rigidbody getRigidbodyWithGravity();
    void unsetRigidbodyWithGravity();
    SocketSet getBackendSocketSet();
    SocketSet getFrontendSocketSet();
}

public struct Drive
{
    public Transform atPoint;
    public float amount;

    public Drive(Transform _atPoint, float _amount) {
        atPoint = _atPoint; amount = _amount;
    }

    public Drive(float _amount) {
        atPoint = null; amount = _amount;
    }
}
