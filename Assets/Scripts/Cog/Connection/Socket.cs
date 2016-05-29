using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public abstract class Socket : MonoBehaviour, IRestoreConnection {

    public Peg autoconnectPeg;
    public int id;

    public virtual RotationMode socketIsChildRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    public virtual RotationMode socketIsParentRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    protected RigidRelationshipConstraint _relationshipConstraint = RigidRelationshipConstraint.CAN_ONLY_BE_CHILD;
    public virtual RigidRelationshipConstraint relationshipConstraint {
        get { return _relationshipConstraint; }
    }

    public Axel axel {
        get { return (Axel) drivingPeg; }
    }

    protected ISocketSetContainer _parentContainer;
    public ISocketSetContainer parentContainer {
        get {
            if (_parentContainer == null) {
                _parentContainer = GetComponentInParent<ISocketSetContainer>();
                Assert.IsTrue(_parentContainer != null, "No parent container??");
            }
            return _parentContainer;
        }
    }
    public Drivable getParentDrivable() {
        return parentContainer.getTransform().GetComponent<Drivable>();
    }

    public delegate void SocketToParentPeg(Socket socket);
    public SocketToParentPeg socketToParentPeg;
    private Peg _drivingPeg;
    public Peg drivingPeg {
        get {
            return _drivingPeg;
        }
        set {
            if (value != null) {
                _drivingPeg = value;
                _drivingPeg.receiveChild(this);
                if (socketToParentPeg != null) { socketToParentPeg(this); }
            } else {
                if (_drivingPeg) {
                    _drivingPeg.releaseChild(this);
                    _drivingPeg = value;
                    parentContainer.getTransform().SetParent(null);
                    parentContainer.unsetRigidbodyWithGravity();
                }
            }
        }
    }

    void Awake() {
        awake();
    }

    protected virtual void awake() {
        gameObject.layer = LayerMask.NameToLayer("CogComponent");
    }

    void Start() {
        if (autoconnectPeg) {
            drivingPeg = autoconnectPeg;
        }
    }

    public void forceFreeRotationPeg(bool wantParentPeg) {
        print("force");
        if (wantParentPeg && hasDrivingPeg()) {
            print("force driving peg");
            drivingPeg._pegIsParentRotationMode = RotationMode.FREE_ONLY;
        } else if (!wantParentPeg && hasChildPeg()) {
            childPeg._pegIsParentRotationMode = RotationMode.FREE_ONLY;
        }
    }

    public void disconnectDrivingPeg() {
        drivingPeg = null;
    }

    private Peg _childPeg;
    public Peg childPeg {
        get { return _childPeg; } 
        set {
            _childPeg = value;
        }
    }

    public bool hasDrivingPeg() { return drivingPeg != null; }
    public bool hasChildPeg() { return childPeg != null; }
    public bool isConnected() {
        return hasChildPeg() || hasDrivingPeg();
    }

    public bool isFreeRotatingOnPeg() {
        return hasDrivingPeg() && drivingPeg.pegIsParentRotationMode == RotationMode.FREE_ONLY;
    }

    public virtual ConstraintTarget getConstraintTargetForChildPegConstraint() {
        return getConstraintTarget(true);
    }
    public virtual ConstraintTarget getConstraintTargetForParentPegConstratin() {
        return getConstraintTarget(false);
    }

    protected virtual ConstraintTarget getConstraintTarget(bool forChild) {
        ConstratintTargetSet cts = GetComponent<ConstratintTargetSet>();
        if (cts != null) {
            return forChild ? cts.forChildConstraintTarget : cts.forParentConstratintTarget;
        }
        return new ConstraintTarget(transform, null);
    }

    public void breakChildConnections() {
        removeConstraint();
    }

    public void removeConstraint() {
        if (hasChildPeg()) {
            //childPeg.removeIsChildConstraintAndItsParentConstraint(this);
            childPeg.detachChildren();
        }
    }

    public Drivable connectedDrivable() {
        Peg peg = null;
        Socket otherSocket = null;
        if (hasChildPeg()) {
            peg = childPeg;
            otherSocket = peg.child;
        } else if (hasDrivingPeg()) {
            peg = drivingPeg;
            otherSocket = peg.parent;
        } else {
            return null;
        }
        if (peg.owner) {
            return peg.owner;
        }
        if (!otherSocket) { return null; }
        return otherSocket.getParentDrivable();
    }

    [System.Serializable]
    class ConnectionData
    {
        public bool hasChildPeg;
        public string connectedGuid; //sure this exists?
    }
    public void storeConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd = new ConnectionData();
        if (hasChildPeg() && !(childPeg is Axel)) {
            cd.hasChildPeg = hasChildPeg();
            Guid connectedGuid = childPeg.GetComponent<Guid>();
            if (connectedGuid == null) {
                connectedGuid = getSpecialCaseGuid(childPeg.transform);
            }
            if (connectedGuid != null) {
                cd.connectedGuid = connectedGuid.guid.ToString();
            } else Debug.LogError("No connected guid for child peg: " + childPeg.name + " of socket: " + name + " parent: " + Bug.GetCogParentName(childPeg.transform));
        }
        SaveManager.Instance.SerializeIntoArray(cd, ref connectionData);
    }

    private Guid getSpecialCaseGuid(Transform tr) {
        IPegProxy ipp = tr.GetComponent<IPegProxy>();
        if (ipp == null) {
            ipp = tr.parent.GetComponent<IPegProxy>();
        }
        if (ipp != null) {
            return ipp.getGuid();
        }
        return null;
    }

    public void restoreConnectionData(ref List<byte[]> connectionData) {
        ConnectionData cd;
        try {
            if ((cd = SaveManager.Instance.DeserializeFromArray<ConnectionData>(ref connectionData)) != null) {
                if (cd.hasChildPeg) {
                    GameObject connectedGO = SaveManager.Instance.FindGameObjectByGuid(cd.connectedGuid);
                    if (connectedGO == null) {
                        return;
                    }
                    Peg peg = connectedGO.GetComponent<Peg>();
                    if (!peg) {
                        if (connectedGO.GetComponent<IPegProxy>() != null) {
                            peg = connectedGO.GetComponent<IPegProxy>().getPeg();
                        }
                    }
                    if (peg) {
                        print("restore soc " + Bug.GetCogParentName(transform) + " got peg: " + Bug.GetCogParentName(peg.transform));
                        peg.beChildOf(this, true);
                    }
                }
            }
        } catch (System.InvalidCastException ice) {
            Debug.LogError("caught invalid cast exception for sock w parent " + transform.parent.parent.name);
        }
    }

    public static implicit operator bool (Socket exists) { return exists != null; }

}

public enum RelationshipConstraint {
  CAN_BE_CHILD_OR_PARENT, CAN_ONLY_BE_CHILD, CAN_ONLY_BE_PARENT  
};

public enum RigidRelationshipConstraint{
    CAN_ONLY_BE_CHILD = RelationshipConstraint.CAN_ONLY_BE_CHILD, CAN_ONLY_BE_PARENT = RelationshipConstraint.CAN_ONLY_BE_PARENT
};

public static class RelationshipConstraintUtil
{
    public static bool Compatible(RelationshipConstraint a, RelationshipConstraint b) {
        return a == b || (a == RelationshipConstraint.CAN_BE_CHILD_OR_PARENT || b == RelationshipConstraint.CAN_BE_CHILD_OR_PARENT);
    }

    public static bool Compatible(RigidRelationshipConstraint a, RelationshipConstraint b) {
        return (int) a == (int) b || (a == (int) RelationshipConstraint.CAN_BE_CHILD_OR_PARENT || b == RelationshipConstraint.CAN_BE_CHILD_OR_PARENT);
    }

    public static bool Compatible(RelationshipConstraint a, RigidRelationshipConstraint b) {
        return Compatible(b, a);
    }
    
    public static bool Compatible(RigidRelationshipConstraint a, RigidRelationshipConstraint b) {
        return (int)a == (int)b;
    }

    public static bool CanBeAChild(RelationshipConstraint rc) {
        return rc != RelationshipConstraint.CAN_ONLY_BE_PARENT;
    }

    public static bool CanBeAChild(RigidRelationshipConstraint rrc) {
        return CanBeAChild((RelationshipConstraint)rrc);
    }

    public static bool CanBeAParent(RelationshipConstraint rc) {
        return rc != RelationshipConstraint.CAN_ONLY_BE_CHILD;
    }

    public static bool CanBeAParent(RigidRelationshipConstraint rrc) {
        return CanBeAParent((RelationshipConstraint)rrc);
    }
}
