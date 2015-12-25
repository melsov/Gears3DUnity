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
    } 

    public void releaseChild(Socket socket) {
        if (getHinge() != null) {
            getHinge().disconnectObject();
        }
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
    }

    public void disconnect() {
        Socket socket = GetComponentInParent<Socket>();
        if (socket != null) {
            socket.childPeg = null;
        }
        transform.SetParent(null);
    }

    public bool connectTo(Collider other) {
        if (other == null) return false;
        Drivable drivable = other.GetComponent<Drivable>();
        if (drivable != null) {
            Socket socket = drivable.getFrontendSocketSet().getOpenParentSocketClosestTo(transform.position, pegIsChildRotationMode); 
            if (socket == null) return false;
            return beChildOf(socket);
        }
        return false;
    }

    public bool makeConnectionWithAfterCursorOverride(Collider other) {
        return false;
    }

    public bool beChildOf(Socket socket) {
        socket.childPeg = this;
        TransformUtil.ParentToAndAlignXZ(transform, socket.transform, null);
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
