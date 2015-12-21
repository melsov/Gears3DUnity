using UnityEngine;
using System.Collections;
using System;

public class Peg : MonoBehaviour , ICursorAgentClient
{
    public virtual RotationMode pegIsParentRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    public virtual RotationMode pegIsChildRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    private bool _occupied;
    public bool occupiedByChild {
        get { return _occupied; } 
        set { _occupied = value; }
    }

    public void disconnectChildSocket() {
        occupiedByChild = false;
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
            socket.childPeg = this;
            TransformUtil.ParentToAndAlignXZ(transform, socket.transform, null);
            return true;
        }
        return false;
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
        return a == b || (a == RotationMode.FREE_OR_FIXED || b == RotationMode.FREE_OR_FIXED);
    }
}
