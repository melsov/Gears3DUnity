using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SocketSet 
{
    public Socket[] sockets;

    public SocketSet(Socket[] _sockets) {
        if (_sockets != null) {
            sockets = _sockets;
        } else {
            sockets = new Socket[] { };
        }

        for(int i = 0; i < sockets.Length; ++i) {
            sockets[i].id = i;
        }
    }

    public Socket socketWithId(int id) {
        foreach(Socket s in sockets) {
            if (id == s.id) { return s; }
        }
        return null;
    }

    public bool contains(Socket s) {
        foreach (Socket soc in sockets) {
            if (soc == s) {
                return true;
            }
        }
        return false;
    }

    public Socket getChildSocketWithParentPegClosestTo(Vector3 global, RotationMode rm) {
        return getChildSocketClosestTo(global, false, true, rm);
    }

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
        Vector3 dist = new Vector3(999999f,999999f,999999f);
        foreach(Socket socket in sockets) {
            if (needOpenPegSlot && (socket.hasChildPeg())) { continue; }
            if (needPegInPegSlot && !socket.hasDrivingPeg()) { debugGetSocket("no child"); continue; }
            if (!RelationshipConstraintUtil.Compatible(socketRelationshipConstraint, socket.relationshipConstraint)) { debugGetSocket("relationship constraint not compatible"); continue; }
            RotationMode relevantSocketRotationMode = socketRelationshipConstraint == RigidRelationshipConstraint.CAN_ONLY_BE_PARENT ? 
                socket.socketIsParentRotationMode : socket.socketIsChildRotationMode;
            if (!RotationModeHelper.CompatibleModes(requiredSocketRotationMode, relevantSocketRotationMode)) { continue; } 
            Vector3 nextdDist = socket.transform.position - global;
            if (nextdDist.sqrMagnitude < dist.sqrMagnitude) {
                dist = nextdDist;
                closest = socket;
            }
        }
        return closest;
    }

    public Socket getSocketClosestTo(Vector3 global) {
        Vector3 dist = new Vector3(9999999f, 9999999f, 9999999f);
        Socket closest = null;
        foreach(Socket s in sockets) {
            Vector3 nextdDist = s.transform.position - global;
            if (nextdDist.sqrMagnitude < dist.sqrMagnitude) {
                dist = nextdDist;
                closest = s;
            }
        }
        return closest;
    }

    public Socket getSocketFurthestFrom(Vector3 global) {
        Vector3 dist = Vector3.zero;
        Socket closest = null;
        foreach(Socket s in sockets) {
            Vector3 nextdDist = s.transform.position - global;
            if (nextdDist.sqrMagnitude > dist.sqrMagnitude) {
                dist = nextdDist;
                closest = s;
            }
        }
        return closest;
    }

    private void debugGetSocket(string s) {
        if (true) MonoBehaviour.print("get soc closest to: " +s);
    }

    public List<Socket> openParentSockets() {
        List<Socket> result = new List<Socket>(sockets.Length);
        foreach (Socket socket in sockets) {
            if (RelationshipConstraintUtil.CanBeAParent(socket.relationshipConstraint) && !socket.hasChildPeg()) { 
                result.Add(socket);
            } 
        }
        return result;
    }

    public List<Socket> socketsWithOpenChildPegs() {
        List<Socket> result = new List<Socket>(sockets.Length);
        foreach (Socket socket in sockets) {
            if (socket.hasChildPeg() && !socket.childPeg.occupiedByChild) {
                result.Add(socket);
            } 
        }
        return result;
    }

    public List<Socket> openChildableSockets() {
        return childableSockets(false);
    }

    public List<Socket> childSocketsWithParents() {
        return childableSockets(true);
    }
    public List<Socket> childableSockets(bool wantHasParentPeg) {
        List<Socket> result = new List<Socket>(sockets.Length);
        foreach (Socket socket in sockets) {
            if (RelationshipConstraintUtil.CanBeAChild(socket.relationshipConstraint) && socket.hasDrivingPeg() == wantHasParentPeg) {
                result.Add(socket);
            }
        }
        return result;
    }
    public bool hasUniqueParentPeg() {
        List<Socket> socs = childSocketsWithParents();
        if (socs.Count == 1) return true;
        return false;
    }

    public virtual Socket openBackendSocketOnOtherClosestToOpenPegOnThis(Transform other, out Peg closestPeg) {
        Socket aSocket = null;
        closestPeg = null;
        ISocketSetContainer ssc = findSocketSetContainer(other); 
        if (ssc == null) return null;
        Vector3 distance = VectorXZ.maxVector3;
        List<Socket> openBackendSockets = ssc.getBackendSocketSet().openChildableSockets();
        List<Socket> openChildPegSockets = socketsWithOpenChildPegs();
        foreach(Socket openBackSocket in openBackendSockets) {
            foreach(Socket childPegSocket in openChildPegSockets) {
                Vector3 nextDist = openBackSocket.transform.position - childPegSocket.transform.position;
                if (distance.magnitude > nextDist.magnitude) {
                    distance = nextDist;
                    aSocket = openBackSocket;
                    closestPeg = childPegSocket.childPeg;
                }
            }
        }
        return aSocket;
    }

//Connect to frontend of other regardless of peg (peg will be instantiated)
//Only condition: socket not already occupied

    public virtual Peg drivingPegOnBackendOfOtherClosestToOpenSocketOnThis(Transform other, out Socket closestOpenSocket) {
        Peg aPeg = null;
        closestOpenSocket = null;
        if (findSocketSetContainer(other) == null) { 
            MonoBehaviour.print("no i socket set container with collider: " + other.name);
            return null;
        }
        ISocketSetContainer ssc = findSocketSetContainer(other); // other.GetComponent<ISocketSetContainer>();
        if (ssc == null) return null; 
        Vector3 distance = VectorXZ.maxVector3;
        List<Socket> occupiedBackendSockets = ssc.getBackendSocketSet().childSocketsWithParents();
        List<Socket> openParentSockets_ = this.openParentSockets();
        foreach(Socket occupiedBackSocket in occupiedBackendSockets) {
            foreach(Socket openParentSocket in openParentSockets_) {
                Vector3 nextDist = occupiedBackSocket.transform.position - openParentSocket.transform.position;
                if (distance.magnitude > nextDist.magnitude) {
                    distance = nextDist;
                    aPeg = occupiedBackSocket.drivingPeg;
                    closestOpenSocket = openParentSocket;
                }
            }
        }
        return aPeg;
    }


    public virtual Socket closestSocketOnFrontendOfRegardlessOfPeg(Collider other, out Socket closestSocket) {
        Socket otherSocket = null;
        closestSocket = null;
        ISocketSetContainer ssc = findSocketSetContainer(other);
        if (ssc == null) { Debug.LogError("no socket set"); return null; }
        Vector3 distance = new Vector3(9999999F, 9999999F, 9999999F);
        SocketSet otherSocketSet = ssc.getFrontendSocketSet();

        foreach (Socket s in otherSocketSet.sockets) { 
            if (s.childPeg != null && s.childPeg.occupiedByChild) { continue; }
            Socket soc = getOpenChildSocketClosestTo(s.transform.position, RotationMode.FREE_OR_FIXED);

            if (distance.sqrMagnitude > (s.transform.position - soc.transform.position).sqrMagnitude) {
                distance = s.transform.position - soc.transform.position;
                otherSocket = s;
                closestSocket = soc;
            }
        }
        return otherSocket;
    }

    public virtual Peg closestOpenPegOnFrontendOf(Collider other, out Socket closestSocket) {
        Peg aPeg = null;
        closestSocket = null;
        ISocketSetContainer ssc = findSocketSetContainer(other); 
        if (ssc == null) return null;
        Vector3 distance = new Vector3(9999999F, 9999999F, 9999999F);
        SocketSet otherSocketSet = ssc.getFrontendSocketSet(); //  wantFrontEndSocketSetOfOther ? ssc.getFrontendSocketSet() : ssc.getBackendSocketSet();
        foreach(Socket s in otherSocketSet.sockets) {
            if (s.childPeg != null && !s.childPeg.occupiedByChild ) {
                Socket soc = getOpenChildSocketClosestTo(s.childPeg.transform.position, s.childPeg.pegIsChildRotationMode);

                if (distance.sqrMagnitude > (s.childPeg.transform.position - soc.transform.position).sqrMagnitude) {
                    distance = s.childPeg.transform.position - soc.transform.position;
                    aPeg = s.childPeg;
                    closestSocket = soc;
                }
            }
        }
        return aPeg;
    }

    public virtual Peg closestOpenPegOnBackendOf(Collider other, out Socket closestSocket) {
        Peg aPeg = null;
        closestSocket = null;
        ISocketSetContainer ssc = findSocketSetContainer(other);
        if (ssc == null) return null;
        Vector3 distance = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        SocketSet otherSocketSet = ssc.getBackendSocketSet();
        foreach(Socket s in otherSocketSet.sockets) {
            Peg drivingPeg = s.drivingPeg;
            if (drivingPeg != null && !drivingPeg.hasParentSocket) {
                Socket soc = getOpenParentSocketClosestTo(drivingPeg.transform.position, drivingPeg.pegIsChildRotationMode); 
                if (distance.magnitude > (drivingPeg.transform.position - soc.transform.position).magnitude) {
                    distance = drivingPeg.transform.position - soc.transform.position;
                    aPeg = drivingPeg;
                    closestSocket = soc;
                }
            }
        }
        return aPeg;
    }

    public void removeConstraintTargetSets() {
        foreach(Socket soc in sockets) {
            GameObject.Destroy(soc.GetComponent<ConstratintTargetSet>());
        }
    }

    public bool isConnected() {
        foreach(Socket soc in sockets) {
            if (soc.isConnected()) {
                return true;
            }
        }
        return false;
    }

    public Socket childSocketOf(Peg peg) {
        foreach(Socket s in sockets) {
            if (s.drivingPeg == peg) {
                return s;
            }
        }
        return null;
    }

    public Peg pegDrivingThisSetOnOther(SocketSet other) {
        foreach(Socket s in other.sockets) {
            if (s.hasChildPeg()) {
                if (childSocketOf(s.childPeg) != null) {
                    return s.childPeg;
                }
            }
        }
        return null;
    }

    public Socket getAnother(Socket socket) {
        if (!contains(socket)) { return null; }
        foreach(Socket s in sockets) {
            if (socket != s) return s;
        }
        return null;
    }

    public T connectedCog<T>() where T : Cog {
        foreach(Socket s in sockets) {
            if (s.hasChildPeg()) {
                if (s.childPeg.occupiedByChild && s.childPeg.child.getParentDrivable() is T) {
                    return s as T;
                }
            } else if (s.hasDrivingPeg()) {
                if (s.drivingPeg.hasParentSocket && s.drivingPeg.parent.getParentDrivable() is T) {
                    return s as T;
                }
            }
        }
        return null;
    }
    public bool isConnectedToCog<T>() where T : Cog {
        return connectedCog<T>() != null;
    }

    public static ISocketSetContainer findISocketSetContainer(Transform other) { return findSocketSetContainer(other); }

    protected static ISocketSetContainer findSocketSetContainer(Transform other) {
        Cog cog = other.GetComponentInParent<Cog>();
        if (cog is IPegProxy) {
            if (((IPegProxy)cog).getPegboard() != null) { return ((IPegProxy)cog).getPegboard(); }
        }
        if (cog == null) { Debug.LogError("no cog ??"); return null; }
        return cog.GetComponentInChildren<ISocketSetContainer>();
    }

    protected ISocketSetContainer findSocketSetContainer(Collider other) {
        return findSocketSetContainer(other.transform);
    }
}
