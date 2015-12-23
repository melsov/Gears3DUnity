using UnityEngine;
using System.Collections;

public class SocketSet 
{
    public Socket[] sockets;

    public SocketSet(Socket[] _sockets) {
        sockets = _sockets;
    }

    public bool contains(Socket s) {
        foreach (Socket soc in sockets) {
            if (soc == s) {
                return true;
            }
        }
        return false;
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
            if (!RotationModeHelper.CompatibleModes(requiredSocketRotationMode, relevantSocketRotationMode)) { continue; } //TODO: socket Is Child not nec. true. make sep class: SocketDJ
            Vector3 nextdDist = socket.transform.position - global;
            if (nextdDist.magnitude < dist.magnitude) {
                dist = nextdDist;
                closest = socket;
            }
        }
        return closest;
    }

    public virtual Peg closestOpenPegOnFrontendOf(Collider other, out Socket closestSocket) {
        Peg aPeg = null;
        closestSocket = null;
        Bug.printComponents(other.gameObject);
        ISocketSetContainer ssc = other.GetComponent<ISocketSetContainer>();
        if (ssc == null) return null;
        Vector3 distance = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach(Socket s in ssc.getFrontendSocketSet().sockets) {
            if (s.childPeg != null)
            if (s.childPeg != null && !s.childPeg.occupiedByChild ) {
                Socket soc = getOpenChildSocketClosestTo(s.childPeg.transform.position, s.childPeg.pegIsChildRotationMode);
                if (distance.magnitude > (s.childPeg.transform.position - soc.transform.position).magnitude) {
                    distance = s.childPeg.transform.position - soc.transform.position;
                    aPeg = s.childPeg;
                    closestSocket = soc;
                }
            }
        }
        return aPeg;
    }
}
