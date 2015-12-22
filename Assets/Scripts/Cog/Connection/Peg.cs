using UnityEngine;
using System.Collections;
using System;

public class Peg : MonoBehaviour , ICursorAgentClient
{
    public Material freeRotationMaterial;
    public Material fixedRotationMaterial;

    private RotationMode __pegIsParentRotationMode = RotationMode.FREE_OR_FIXED;

    public Transform hingePrefab;

    protected Hinge getHinge() {
        Hinge hinge = GetComponentInChildren<Hinge>();
        return hinge;
        //HingeJoint hj = GetComponent<HingeJoint>();
        //if (hj == null) {
        //    hj = gameObject.AddComponent<HingeJoint>();
        //    hj.axis = EnvironmentSettings.towardsCameraDirection;
        //    hj.anchor = Vector3.zero;
        //}
        //return hj;
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
            print("set up hinge joint");
            //socket.parentContainer.getTransform().SetParent(getHinge().getHingeJoint().connectedBody.transform);
            TransformUtil.AlignXZ(socket.parentContainer.getTransform(), getHinge().getHingeJoint().connectedBody.transform, socket.transform);
            getHinge().connect(socket.parentContainer.getRigidbodyWithGravity());
            getHinge().getHingeJoint().connectedAnchor = socket.transform.localPosition;
            //TransformUtil.ParentToAndAlignXZ(socket.parentContainer.getTransform(), getHinge().transform, socket.transform);
            //getHinge().getHingeJoint().connectedBody = socket.parentContainer.getRigidbodyWithGravity();
            //getHingeJoint().connectedBody  // = socket.parentContainer.getRigidbodyWithGravity();
        } else {
            TransformUtil.ParentToAndAlignXZ(socket.parentContainer.getTransform(), transform, socket.transform);
        }
    } 

    public void releaseChild(Socket socket) {
        getHinge().disconnectObject();
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
}

public enum RotationMode
{
    FREE_OR_FIXED, FREE_ONLY, FIXED_ONLY
};

public static class RotationModeHelper
{
    public static bool CompatibleModes(RotationMode a, RotationMode b) {
        MonoBehaviour.print("Ro modes compatible " + (a == b || (a == RotationMode.FREE_OR_FIXED || b == RotationMode.FREE_OR_FIXED)));
        return a == b || (a == RotationMode.FREE_OR_FIXED || b == RotationMode.FREE_OR_FIXED);
    }
}
