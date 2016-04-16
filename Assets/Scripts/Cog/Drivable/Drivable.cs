﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

//TODO: new drivable: 'rack' (pole with gear teeth)

[System.Serializable]
public abstract class Drivable : Cog , ICursorAgentClient , IAddOnClient , IGameSerializable, IRestoreConnection
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
    protected Pegboard _pegboard;
    protected Drivable _driver;
    protected List<Drivable> drivables = new List<Drivable>();
    protected ControllerAddOn controllerAddOn;
    protected List<ReceiverAddOn> receiverAddOns = new List<ReceiverAddOn>();
    protected Transform _cursorRotationPivot = null;
    protected Transform _cursorRotationHandle = null;

    protected virtual float radius {
        get { return  GetComponent<CapsuleCollider>().radius * transform.localScale.x; }
    }

    protected VectorXZ xzPosition { get { return new VectorXZ(transform.position); } }

    void Awake () {
        awake();
	}
    protected virtual void awake() {
        _pegboard = GetComponent<Pegboard>();
        if (_pegboard== null) {
            _pegboard = GetComponentInChildren<Pegboard>();
            if (_pegboard == null) {
                _pegboard = gameObject.AddComponent<Pegboard>();
            }
        }
        TransformUtil.PositionOnYLayer(transform);
        if (GetComponent<Highlighter>() == null) {
            gameObject.AddComponent<Highlighter>();
        }
        Pause.Instance.onPause += pause;
        setupSocketDelegates();
    }
    private void setupSocketDelegates() {
        foreach(Socket s in _pegboard.getBackendSocketSet().sockets) {
            s.socketToParentPeg = onSocketToParentPeg;
        }
    }

    protected virtual void pause(bool isPaused) {

    }

    public virtual bool isOnAxel() {
        return connectedSocket != null && connectedSocket.axel != null && transform.parent != null;
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

    protected void debugY() {
        print("this " + GetType() + " has Y: " + transform.position.y);
    }

    protected virtual void update() {
        updateAngleStep();
        //foreach (Drivable dr in drivables) {
        //    dr.receiveDrive(new Drive(driveScalar()));
        //}
        for (int i=0; i < drivables.Count; ++i) {
            Drivable dr = drivables[i];
            if (dr == null) {
                drivables.RemoveAt(i--);
                continue;
            }
            dr.receiveDrive(new Drive(driveScalar()));
        }
    }

    protected virtual void setSocketClosestToAxel(Axel axel) {
        connectedSocket = _pegboard.getBackendSocketSet().getOpenChildSocketClosestTo(axel.transform.position, axel.pegIsParentRotationMode); 
        setSocketToPeg(connectedSocket, axel);
    }

    protected virtual void setSocketToPeg(Socket socket, Peg peg) {
        socket.drivingPeg = peg;
    }

    protected virtual void onSocketToParentPeg(Socket socket) {

    }

    public virtual void addDrivable(Drivable _drivable) {
        if (drivables.Contains(_drivable)) { print("already contains drivable: " + _drivable.name); }
        if (!drivables.Contains(_drivable) && _drivable != this)
            drivables.Add(_drivable);
    }

    public virtual void removeDrivable(Drivable _drivable) {
        while(drivables.Contains(_drivable)) {
            drivables.Remove(_drivable);
            _drivable.disconnectFromDriver();
        }
    }

    #region iCursorAgentClient methods
    public void disconnect() {
        vDisconnect();
    }

    protected virtual void vDisconnect() {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.zero;
        }
        Bug.debugIfIs<LinearActuator>(this, "linear actuator vDisconnect");
        Bug.debugIfIs<Pole>(this, "pole vDisconnect");
        detachFromAxel();
        if (_driver != null)
            _driver.removeDrivable(this);
        disconnectFromDriver();
        disconnectSockets();
        //disconnectDrivens(); //WANT
        transform.SetParent(null);
    }

    protected virtual void disconnectDrivens() {
        foreach (Drivable d in drivables) {
            d.disconnectFromDriver();
        }
        drivables.RemoveAll(delegate (Drivable d) { return true; });
    }

    protected virtual void disconnectSockets() {
        foreach(Socket soc in _pegboard.getFrontendSocketSet().sockets) {
            soc.breakChildConnections();
        }
        foreach (Socket soc in _pegboard.getBackendSocketSet().sockets) {
            if (soc.hasDrivingPeg()) {
                if (soc.drivingPeg.owner != this) { 
                    soc.disconnectDrivingPeg();
                } else {
                    soc.drivingPeg.disconnectFromParent();
                }
            }
        }
    }

    public virtual void disconnectFromDriver() {
        _driver = null;
    }

    protected virtual void detachFromAxel() {
        connectedSocket = null;
    }

    public bool connectTo(Collider other) {
        return vConnectTo(other);
    }

    protected virtual bool vConnectTo(Collider other) {
        if (isConnectedTo(other.transform)) return false;
        // Connect to any peg or axel
        Socket aSocket;
        Peg peg = _pegboard.getBackendSocketSet().closestOpenPegOnFrontendOf(other, out aSocket);
        if (peg != null) {
            setSocketToPeg(aSocket, peg);
            return true;
        }
        return false;
    }

    public void onDragEnd() {
        vOnDragEnd();
    }
    protected virtual void vOnDragEnd() {

    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return vMakeConnectionWithAfterCursorOverride(other);
    }
    
    protected virtual bool vMakeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    protected virtual bool isConnectedTo(Transform t) {
        return isConnectedTo(t, 5);
    }
    private bool isConnectedTo(Transform t, int depth) {
        if (depth <= 0) return false;
        if (t == null) return false;
        Drivable other = t.GetComponentInParent<Drivable>(); 
        if (other == null) return false;
        foreach (Socket s in _pegboard.getBackendSocketSet().sockets) {
            if (other == s.connectedDrivable()) {
                return true;
            } else if (s.connectedDrivable() != null) {
                if (s.connectedDrivable().isConnectedTo(t, --depth)) {
                    return true;
                }
            }
        }
        foreach(Socket s in _pegboard.getFrontendSocketSet().sockets) {
            if (other == s.connectedDrivable()) {
                return true;
            } else if (s.connectedDrivable() != null) {
                if (s.connectedDrivable().isConnectedTo(t, --depth)) {
                    return true;
                }
            }
        }
        return false;
    }
    public List<Drivable> drivableParents() {
        return drivableParents(20);
    }
    private List<Drivable> drivableParents(int depth) {
        if (depth <= 0) return null;
        List<Drivable> result = new List<Drivable>();
        foreach(Socket s in _pegboard.getBackendSocketSet().sockets) {
            if(s.connectedDrivable() != null) {
                result.Add(s.connectedDrivable());
                List<Drivable> recur = s.connectedDrivable().drivableParents(--depth);
                if (recur != null) {
                    result.AddRange(recur);
                }
                return result;
            }
        }
        return new List<Drivable>();
    }
    public T getDrivableParent<T>() {
        foreach (Drivable d in drivableParents()) {
            if (d is T) { return d.GetComponent<T>(); }
        }
        return default(T);
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

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        vStartDragOverride(cursorGlobal, dragOverrideCollider);
    }
    public void dragOverride(VectorXZ cursorGlobal) { 
        vDragOverride(cursorGlobal);
    }
    public void endDragOverride(VectorXZ cursorGlobal) {
        vEndDragOverride(cursorGlobal);
    }


    protected virtual void updateCursorRotationPivot(Collider dragOverrideCollider) {
        if (_cursorRotationPivot == null) {
            _cursorRotationPivot = this.transform;

            //doomed? since maybe we disconnect before this?
            if (_pegboard.getBackendSocketSet().hasUniqueParentPeg()) {
                Peg peg = _pegboard.getBackendSocketSet().childSocketsWithParents()[0].drivingPeg;
                if (peg != null) {
                    _cursorRotationPivot = peg.transform;
                }
            }

            HandleSet handleSet = dragOverrideCollider.GetComponentInParent<HandleSet>();
            if (handleSet == null) {
                return;
            }
            if (handleSet != null) {
                Handle other = handleSet.getAnotherThatIsntThisOne(dragOverrideCollider.GetComponent<Handle>());
                if (other != null) {
                    _cursorRotationPivot = other.transform;
                }
            }
        }
    }

    protected virtual void removeConstraintsFromWidget(Handle handle) {
        if (handle.widget == null) return;
        Socket socket = handle.widget.GetComponent<Socket>();
        if (socket != null) {
            socket.removeConstraint();
        }
    }

    protected virtual void vStartDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
        removeConstraintsFromWidget(dragOverrideCollider.GetComponent<Handle>());
        _cursorRotationHandle = dragOverrideCollider.transform;
        _cursorRotationPivot = null;
        updateCursorRotationPivot (dragOverrideCollider);
    }
    protected virtual void vDragOverride(VectorXZ cursorGlobal) {
        // rotate around the pivot
        Vector3 current = _cursorRotationHandle.position - _cursorRotationPivot.position;
        Vector3 target = cursorGlobal.vector3(_cursorRotationPivot.position.y) - _cursorRotationPivot.position;
        transform.RotateAround(_cursorRotationPivot.position, EnvironmentSettings.towardsCameraDirection, Quaternion.FromToRotation(current, target).eulerAngles.y);
    }

    protected virtual void vEndDragOverride(VectorXZ cursorGlobal) {
    }

    public void triggerExitDuringDrag(Collider other) {
        vTriggerExit(other);
    }
//THIS DOESN'T GET TRIGGERED ?
    protected virtual void vTriggerExit(Collider other) {
        return; // TODO: refine cases where we should disconnect when moving handles
        ISocketSetContainer ssc = other.GetComponent<ISocketSetContainer>();
        if (ssc == null) {
            print("vTriggerExit: ssc null");
            return;
        }
        Peg parentPeg = _pegboard.getBackendSocketSet().pegDrivingThisSetOnOther(ssc.getFrontendSocketSet());
        bool shouldDisconnect = false;
        if (parentPeg != null) {
            print("vTriggerExit: parent peg null");
            shouldDisconnect = true;
        }
        // did we disconnect from out driver?
        if (!shouldDisconnect) {
            Drivable d = other.GetComponent<Drivable>();
            if (d == _driver) {
                shouldDisconnect = true;
            }
        }
        print("should disconnect: " + shouldDisconnect);
        if (shouldDisconnect) {
            print("vTrigger Exit: got should disconnect");
            vDisconnect();
        }
    }

    public Collider mainCollider() {
        return vMainCollider();
    }
    protected virtual Collider vMainCollider() {
        Collider c = GetComponent<Collider>();
        if (c == null) {
            List<Transform> mainColliders = TagLookup.ChildrenWithTag(gameObject, TagLookup.MainCollider);
            Assert.IsTrue(mainColliders.Count < 2);
            if (mainColliders.Count == 1) {
                c = mainColliders[0].GetComponent<Collider>();
            } else {
                c = GetComponentInChildren<Collider>();
            }
        }
        return c;
    }
    #endregion

    protected virtual bool isInConnectionRange(Collider other) {
        if (other == null) {
            return false;
        }
        Drivable d = other.GetComponent<Drivable>();
        
        VectorXZ globalXZ = new VectorXZ(other.transform.position);
        if (d != null) {
            globalXZ = d.getConnectionPoint(GetComponent<Collider>());
        }
        CapsuleCollider cc = other.GetComponent<CapsuleCollider>();
        if (cc != null) {
            float centerDistance = (globalXZ - getConnectionPoint(other)).toVector2.magnitude;
            print(centerDistance < cc.radius * other.transform.localScale.x + GetComponent<CapsuleCollider>().radius * transform.localScale.x);
            return centerDistance < cc.radius * other.transform.localScale.x + GetComponent<CapsuleCollider>().radius * transform.localScale.x;
        }
        return false;
    }

    protected virtual VectorXZ getConnectionPoint(Collider other) {
        return new VectorXZ(transform.position);
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

    public virtual Constraint parentConstraintFor(Constraint childConstraint, Transform childTransform) {
        return null;
    }

    public bool connectToAddOn(AddOn addOn_) {
        if (addOn_ is ControllerAddOn) {
            return connectToControllerAddOn((ControllerAddOn)addOn_);
        } else if (addOn_ is ReceiverAddOn) {
            return connectToReceiverAddOn((ReceiverAddOn)addOn_);
        }
        return false;
    }
    protected virtual bool connectToControllerAddOn(ControllerAddOn cao) {
        if (controllerAddOn == null) {
            controllerAddOn = cao;
            controllerAddOn.setScalar = handleAddOnScalar;
            return true;
        }
        return false;
    }
    protected virtual bool connectToReceiverAddOn(ReceiverAddOn rao) {
        receiverAddOns.Add(rao);
        return true;
    }

    protected virtual void handleAddOnScalar(float scalar) { }

    protected virtual void updateRecieverAddOns(float scalar) {
        foreach(ReceiverAddOn rao in receiverAddOns) {
            rao.input = scalar;
        }
    }

    public void disconnectAddOn(AddOn addOn_) {
        if (addOn_ == controllerAddOn) {
            controllerAddOn = null;
        } else if (addOn_ is ReceiverAddOn) {
            receiverAddOns.Remove((ReceiverAddOn)addOn_);
        }
    }

    public void disconnectFromParentHinge() {
        _pegboard.unsetRigidbodyWithGravity();
    }

    [System.Serializable]
    class SerializeStorage
    {

    }
    public virtual void Serialize(ref List<byte[]> data) {
        SerializeStorage stor = new SerializeStorage();

        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public virtual void Deserialize(ref List<byte[]> data) {
        SerializeStorage stor;
        if ((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {

        }
    }

    [System.Serializable]
    class ConnectionData
    {
        public List<string> drivableGuids = new List<string>();
    }
    public virtual void storeConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd = new ConnectionData();
        foreach(Drivable d in drivables) {
            cd.drivableGuids.Add(d.GetComponent<Guid>().guid.ToString());
        }
        SaveManager.Instance.SerializeIntoArray(cd, ref connectionData);
    }

    public virtual void restoreConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd;
        if ((cd = SaveManager.Instance.DeserializeFromArray<ConnectionData>(ref connectionData)) != null) {
            foreach(String drivableGuid in cd.drivableGuids) {
                GameObject drivenGO = SaveManager.Instance.FindGameObjectByGuid(drivableGuid);
                Drivable d = drivenGO.GetComponent<Drivable>();
                addDrivable(d);
                if (this is RackGear)
                    Assert.IsTrue(drivables.Count > 0);
            }
        }
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
    public static Drive Zero = new Drive(null, 0f);

    public Drive(Transform _atPoint, float _amount) {
        atPoint = _atPoint; amount = _amount;
    }

    public Drive(float _amount) {
        atPoint = null; amount = _amount;
    }


}
