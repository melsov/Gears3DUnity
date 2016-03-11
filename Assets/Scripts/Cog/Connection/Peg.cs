using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;
using System.Collections.Generic;

public class Peg : Cog , ICursorAgentClient, IGameSerializable, IRestoreConnection, IConstrainable
{
    public Material freeRotationMaterial;
    public Material fixedRotationMaterial;

    private RotationMode __pegIsParentRotationMode = RotationMode.FREE_OR_FIXED;

    public Hinge hingePrefab;

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

    protected Hinge getHinge() {
        Hinge hinge = GetComponentInChildren<Hinge>();
        if (hinge == null) {
            if (hingePrefab != null) {
                hinge = Instantiate<Hinge>(hingePrefab);
                hinge.gameObject.SetActive(true);
                TransformUtil.ParentToAndAlignXZ(hinge.transform, transform, null);
            }
        }
        return hinge;
    }

    private Constraint _isChildConstraint;
    public virtual Constraint isChildConstraint {
        get { return _isChildConstraint; }
    }

    protected RotationMode _pegIsParentRotationMode {
        get { return __pegIsParentRotationMode; }
        set {
            __pegIsParentRotationMode = value;
            setMaterial();
        }
    }

    public void receiveChild(Socket socket) {
        if (pegIsParentRotationMode == RotationMode.FREE_ONLY || socket.socketIsChildRotationMode == RotationMode.FREE_ONLY) {
            TransformUtil.AlignXZ(socket.parentContainer.getTransform(), transform, socket.transform);
            getHinge().connect(socket.parentContainer.getRigidbodyWithGravity());
            getHinge().getHingeJoint().connectedAnchor = socket.transform.localPosition;
        } else {
            TransformUtil.ParentToAndAlignXZ(socket.parentContainer.getTransform(), transform, socket.transform);
        }
        _childSocket = new WeakReference(socket);
    } 

    public void releaseChild(Socket socket) {
        if (getHinge() != null) {
            getHinge().disconnectObject();
        }
        _childSocket.Target = null;
    }

    private Renderer findRenderer() {
        Renderer result = GetComponent<Renderer>();
        if (result == null) {
             foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
                if(r.gameObject.tag == "ChildMesh") {
                    result = r;
                }
            }
        }
        return result;
    }

    private void setMaterial() {
        Renderer renderer = findRenderer();
        if (renderer == null) return;
        if (pegIsParentRotationMode == RotationMode.FREE_ONLY) {
            renderer.material = freeRotationMaterial;
        } else if (pegIsParentRotationMode == RotationMode.FIXED_ONLY) {
            renderer.material = fixedRotationMaterial;
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
        setMaterial();
        _isChildConstraint = GetComponent<Constraint>();
        gameObject.AddComponent<Highlighter>();
        GetComponent<Highlighter>().highlightColor = Color.green;
        _owner = GetComponentInParent<Drivable>();
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

    public bool connectTo(Collider other) {
        if (other == null) return false;
        Pegboard pegboard = other.GetComponent<Pegboard>();
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

    public bool beChildOf(Socket socket) {
        return beChildOf(socket, false);
    }

    public bool beChildOf(Socket socket, bool skipConstraint) {
        socket.childPeg = this;
        _parentSocket = new WeakReference(socket);
        if (!skipConstraint && isChildConstraint != null) {
            setupConstraint();
            GetComponent<Highlighter>().highlight(Color.cyan);
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

    [System.Serializable]
    class ConnectionData
    {
        public bool hasChild;
        public string connectedGuid; //sure this exists?
        public int connectedSocketID;
    }
    public void storeConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd = new ConnectionData();
        if (hasChild) {
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
                if (cd.connectedGuid == null || string.IsNullOrEmpty(cd.connectedGuid)) { return; }
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
