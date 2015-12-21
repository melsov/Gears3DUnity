using UnityEngine;
using System.Collections;

public class SocketSet 
{
    public Socket[] sockets;

    public SocketSet(Socket[] _sockets) {
        sockets = _sockets;
    }

    //public Socket getSocketClosestTo(Vector3 global, RotationMode rm) { return getSocketClosestTo(global, false, false, rm); }

    public Socket getOpenChildSocketClosestTo(Vector3 global, RotationMode rm) {
        return getChildSocketClosestTo(global, true, false, rm);
    }

    public Socket getParentSocketWithChildPegClosestTo(Vector3 global, RotationMode rm) {
        return getParentSocketClosestTo(global, false, true, rm);
    }
    public Socket getOpenParentSocketClosestTo(Vector3 global, RotationMode rm) {
        return getParentSocketClosestTo(global, true, false, rm);
    }

    protected Socket getParentSocketClosestTo(Vector3 global,
        bool needOpenPegSlot,
        bool needPegInPegSlot,
        RotationMode requiredSocketRotationMode) {
        return getSocketClosestTo(global, needOpenPegSlot, needPegInPegSlot, requiredSocketRotationMode, RigidRelationshipConstraint.CAN_ONLY_BE_PARENT);
    }

    protected Socket getChildSocketClosestTo(Vector3 global,
        bool needOpenPegSlot,
        bool needPegInPegSlot,
        RotationMode requiredSocketRotationMode) {
        return getSocketClosestTo(global, needOpenPegSlot, needPegInPegSlot, requiredSocketRotationMode, RigidRelationshipConstraint.CAN_ONLY_BE_CHILD);
    }

    public virtual Socket getSocketClosestTo(Vector3 global, 
        bool needOpenPegSlot, 
        bool needPegInPegSlot, 
        RotationMode requiredSocketRotationMode, 
        RigidRelationshipConstraint socketRelationshipConstraint) {
        
        Socket closest = null;
        Vector3 dist = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach(Socket socket in sockets) {
            if (needOpenPegSlot && (socket.hasChildPeg())) { continue; }
            if (needPegInPegSlot && !socket.hasChildPeg()) { continue; }
            if (!RelationshipConstraintUtil.Compatible(socketRelationshipConstraint, socket.relationshipConstraint)) { continue; }
            RotationMode relevantSocketRotationMode = socketRelationshipConstraint == RigidRelationshipConstraint.CAN_ONLY_BE_PARENT ? 
                socket.socketIsParentRotationMode : socket.socketIsChildRotationMode;
            if (!RotationModeHelper.CompatibleModes(requiredSocketRotationMode, socket.socketIsChildRotationMode)) { continue; } //TODO: socket Is Child not nec. true. make sep class: SocketDJ
            Vector3 nextdDist = socket.transform.position - global;
            if (nextdDist.magnitude < dist.magnitude) {
                dist = nextdDist;
                closest = socket;
            }
        }
        return closest;
    }
}
