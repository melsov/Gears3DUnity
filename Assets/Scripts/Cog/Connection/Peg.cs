using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;
using System.Collections.Generic;

public class Peg : Cog , ICursorAgentClient, IGameSerializable, IRestoreConnection, IConstrainable
{
    //public Material freeRotationMaterial;
    //public Material fixedRotationMaterial;
    [SerializeField]
    protected Transform freeRotationMesh;
    [SerializeField]
    protected Transform fixedRotationMesh;

    private RotationMode __pegIsParentRotationMode = RotationMode.FREE_OR_FIXED;

    protected Hinge hingePrefab;
    protected Hinge hinge;

    protected Drivable _owner; //owners permanently own pegs (pegboards with pegs on them are not owners)
    public Drivable owner {
        get { return _owner; }
    }

    protected WeakReference _parentSocket;
    public Socket parent {
        get {
            if (_parentSocket != null) {
                return _parentSocket.Target as Socket;
            }
            return null;
        }
    }

    protected WeakReference _childSocket;
    public Socket child {
        get {
            if (_childSocket != null) {
                return _childSocket.Target as Socket;
            }
            return null;
        }
    }
    public bool hasChild {
        get { return child != null; }
    }

    public Hinge getHinge() {
        hinge = GetComponentInChildren<Hinge>();
        if (hinge == null) {
            hinge = Instantiate<Hinge>(getHingePrefab());
            hinge.gameObject.SetActive(true);
            TransformUtil.ParentToAndAlignXZ(hinge.transform, transform, null);
        }
        return hinge;
    }

    private Constraint _isChildConstraint;
    public virtual Constraint isChildConstraint {
        get { return _isChildConstraint; }
    }

    public RotationMode _pegIsParentRotationMode {
        get { return __pegIsParentRotationMode; }
        set {
            if (__pegIsParentRotationMode != value) {
                __pegIsParentRotationMode = value;
                setMesh();
                if (hasChild) {
                    receiveChild(child);
                }
            }
        }
    }

    public void receiveChild(Socket socket) {
        if (pegIsParentRotationMode == RotationMode.FREE_ONLY || socket.socketIsChildRotationMode == RotationMode.FREE_ONLY) {
            TransformUtil.AlignXZ(socket.parentContainer.getTransform(), transform, socket.transform);
            forceUnparentConnectedBody(socket.parentContainer.getRigidbodyWithGravity());
            getHinge().connect(socket.parentContainer.getRigidbodyWithGravity());
            getHinge().getHingeJoint().connectedAnchor = socket.transform.localPosition;
        } else {
            TransformUtil.ParentToAndAlignXZ(socket.parentContainer.getTransform(), transform, socket.transform);
        }
        _childSocket = new WeakReference(socket);
    } 
    private void forceUnparentConnectedBody(Rigidbody rb) {
        Cog c = rb.GetComponentInParent<Cog>();
        if (c.transform.parent == transform) {
            c.transform.parent = null;
        }
    }

    public void releaseChild(Socket socket) {
        if (hinge != null) {
            hinge.disconnectObject();
        }
        _childSocket.Target = null;
    }

    private void setMesh() {
        if (pegIsParentRotationMode == RotationMode.FREE_ONLY) {
            freeRotationMesh.gameObject.SetActive(true);
            fixedRotationMesh.gameObject.SetActive(false);
        } else if (pegIsParentRotationMode == RotationMode.FIXED_ONLY) {
            freeRotationMesh.gameObject.SetActive(false);
            fixedRotationMesh.gameObject.SetActive(true);
        }
    }

    public virtual RotationMode pegIsParentRotationMode {
        get {
            return _pegIsParentRotationMode;
        }
    }

    public virtual RotationMode pegIsChildRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    public bool occupiedByChild {
        get {
            if (transform.childCount == 0) return false;
            return transform.GetComponentInChildren<Drivable>() != null;
        }
    }

    void Awake() {
        awake();
    }

    #region serialize
    [System.Serializable]
    class SerializeStorage
    {
        public int __pegIsParentRotationMode;
    }

    public void Serialize(ref List<byte[]> data) {
        SerializeStorage stor = new SerializeStorage();
        stor.__pegIsParentRotationMode = (int)__pegIsParentRotationMode;
        SaveManager.Instance.SerializeIntoArray(stor, ref data);
    }

    public void Deserialize(ref List<byte[]> data) {
        SerializeStorage stor;
        if((stor = SaveManager.Instance.DeserializeFromArray<SerializeStorage>(ref data)) != null) {
            __pegIsParentRotationMode = (RotationMode)stor.__pegIsParentRotationMode;
        }
    }
    #endregion

    protected virtual void awake() {
        setMesh();
        _isChildConstraint = GetComponent<Constraint>();
        gameObject.AddComponent<Highlighter>();
        //GetComponent<Highlighter>().highlightColor = Color.green;
        _owner = GetComponentInParent<Drivable>();
        if (GetComponent<Rigidbody>() != null) {
            GetComponent<Rigidbody>().isKinematic = true; // OK for all pegs? This helps pegs not act weird when childed to LA pegboards
        }
    }
    private Hinge getHingePrefab() {
        if (hingePrefab == null) {
            foreach (Hinge h in Resources.LoadAll<Hinge>("Prefabs")) {
                if (h.name.Equals("HingePrefab")) {
                    hingePrefab = h;
                }
            }
        }
        Assert.IsTrue(hingePrefab != null, "wha? couldn't find hinge prefab");
        return hingePrefab;
    }

    public void disconnect() {
        Socket socket = GetComponentInParent<Socket>();
        if (socket != null) {
            socket.childPeg = null;
        }
        if (_owner == null) {
            transform.SetParent(null);
        }
        GetComponent<Highlighter>().unhighlight();
    }

    public void detachChildren() {
        removeIsChildConstraintAndItsParentConstraint();
    }

    public void disconnectFromParent() {
        print("peg disconnect froom parent");
        //removeIsChildConstraintAndItsParentConstraint();
    }

    protected void removeIsChildConstraintAndItsParentConstraint() { 
        if (isChildConstraint != null) {
            isChildConstraint.removeTarget();
            GameObject.Destroy(isChildConstraint.constraintTarget.parentConstraint);
            parent.childPeg = null;
        }
    }

    public void handleTriggerEnter(Collider other) {
        //TODO: peg highlights if it could connect with other
    }

    public bool connectTo(Collider other) {
        if (other == null) return false;
        Pegboard pegboard = other.GetComponent<Pegboard>();
        if (pegboard == null) {
            IPegProxy ipp = other.GetComponent<IPegProxy>();
            if (ipp != null) { pegboard = ipp.getPegboard(); }
        }
        if (pegboard != null) {
            Socket socket = pegboard.getFrontendSocketSet().getOpenParentSocketClosestTo(transform.position, pegIsChildRotationMode);
            if (socket == null) return false;
            return beChildOf(socket);
        }
        return false;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public virtual bool hasParentSocket {
        get {
            return transform.parent.GetComponent<Socket>() != null;
        }
    }
    //TODO: pegs childed to pegboard of LAs are moving around
    // seem to have (be reacting to????) a constraint? (though none have been added to them!)

    public bool beChildOf(Socket socket) {
        return beChildOf(socket, false);
    }

    public bool beChildOf(Socket socket, bool skipConstraint) {
        socket.childPeg = this;
        _parentSocket = new WeakReference(socket);
        if (!skipConstraint && isChildConstraint != null) {
            setupConstraint();
            GetComponent<Highlighter>().highlight();
            Bug.bugAndPause("be child of");
        } else if (isChildConstraint == null) {
            TransformUtil.ParentToAndAlignXZ(transform, socket.transform, null);
        }
        return true;
    }

    public void setupConstraint() {
        if (_parentSocket != null && _parentSocket.Target != null && isChildConstraint != null) {
            Socket socket = (Socket)_parentSocket.Target;
            isChildConstraint.constraintTarget = socket.getConstraintTargetForChildPegConstraint();
            isChildConstraint.constraintTarget.parentConstraint = socket.parentContainer.getTransform().GetComponent<Drivable>().parentConstraintFor(isChildConstraint, transform);
        }
    }

    public void suspendConnection() {
      
    }

    public Collider shouldPreserveConnection() {
        return null;
    }

    public void startDragOverride(VectorXZ cursorGlobal, Collider dragOverrideCollider) {
    }

    public void dragOverride(VectorXZ cursorGlobal) {
    }

    public void endDragOverride(VectorXZ cursorGlobal) {
    }

    public Collider mainCollider() { return GetComponent<Collider>(); }

    public void triggerExitDuringDrag(Collider other) {
    }
    public bool isAutoConnectPeg() {
        return hasChild && child.autoconnectPeg == this;
    }

    [System.Serializable]
    class ConnectionData
    {
        public bool hasChild;
        public string connectedGuid; //sure this exists?
        public int connectedSocketID;
    }
    public void storeConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd = new ConnectionData();
        if (hasChild && !isAutoConnectPeg()) {
            cd.hasChild = hasChild;
            Transform connectedT = child.parentContainer.getTransform();
            Guid connectedGuid = connectedT.GetComponent<Guid>();
            if (connectedGuid != null) {
                print("*peg connected guid name: " + connectedGuid.name + " guid: \n" + connectedGuid.guid.ToString());
                cd.connectedGuid = connectedGuid.guid.ToString();
                cd.connectedSocketID = child.id;
            } else Debug.LogError("No connected guid for: " + connectedT.name + "(child of peg: " + name + ")");
        }
        SaveManager.Instance.SerializeIntoArray(cd, ref connectionData);
    }

    public void restoreConnectionData(ref List<byte[]> connectionData) {
        print("restore connection data in peg");
        ConnectionData cd;
        if ((cd = SaveManager.Instance.DeserializeFromArray<ConnectionData>(ref connectionData)) != null) {
            if (cd.hasChild) {
                if (cd.connectedGuid == null || string.IsNullOrEmpty(cd.connectedGuid)) {
                    Debug.LogError("conn GUID null for: " + name + " cog parent: " + Bug.GetCogParentName(transform));
                    return;
                }
                GameObject connectedGO = SaveManager.Instance.FindGameObjectByGuid(cd.connectedGuid);
                if (connectedGO != null)
                    print("found game object for guid: " + connectedGO.name);
                else
                    print("connectedGo null: '" + cd.connectedGuid + "' (end)");
                Pegboard pb = connectedGO.GetComponentInChildren<Pegboard>();
                if (pb != null) {
                    print("found pegboard");
                    Socket s = pb.getBackendSocketSet().socketWithId(cd.connectedSocketID);
                    s.drivingPeg = this;
                    //receiveChild(s);
                }
            }
        }
    }

    public void onDragEnd() {
    }

}

public enum RotationMode
{
    FREE_OR_FIXED, FREE_ONLY, FIXED_ONLY
};

public static class RotationModeHelper
{
    public static bool CompatibleModes(RotationMode a, RotationMode b) {
        return a == b || (a == RotationMode.FREE_OR_FIXED || b == RotationMode.FREE_OR_FIXED);
    }
}
