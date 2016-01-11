using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class Peg : MonoBehaviour , ICursorAgentClient
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
            return _parentSocket.Target as Socket;
        }
    }

    protected WeakReference _childSocket;
    public Socket child {
        get {
            return _childSocket.Target as Socket;
        }
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

    public void removeIsChildConstraintAndItsParentConstraint(Socket parent) { 
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
        socket.childPeg = this;
        _parentSocket = new WeakReference(socket);
        if (isChildConstraint != null) {
            isChildConstraint.constraintTarget = socket.getConstraintTargetForChildPegConstraint();
            isChildConstraint.constraintTarget.parentConstraint = socket.parentContainer.getTransform().GetComponent<Drivable>().parentConstraintFor(isChildConstraint, transform);
            // TODO: let parent constraint align the child?
            GetComponent<Highlighter>().highlight(Color.cyan);
        } else {
            TransformUtil.ParentToAndAlignXZ(transform, socket.transform, null);
        }
        return true;
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
