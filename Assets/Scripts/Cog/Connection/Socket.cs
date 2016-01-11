using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public abstract class Socket : MonoBehaviour {

    public Peg autoconnectPeg;
    //public bool matePermanently; //TODO: implement permanency // consider whether you need a var for it here?

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
                Assert.IsTrue(_parentContainer != null);
            }
            return _parentContainer;
        }
    }
    public Drivable getParentDrivable() {
        return parentContainer.getTransform().GetComponent<Drivable>();
    }

    private Peg _drivingPeg;
    public Peg drivingPeg {
        get {
            return _drivingPeg;
        }
        set {
            if (value != null) {
                _drivingPeg = value;
                _drivingPeg.receiveChild(this);
            } else {
                if (_drivingPeg != null) {
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
        if (autoconnectPeg != null) {
            drivingPeg = autoconnectPeg;
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

    public void removeConstraint() {
        if (hasChildPeg()) {
            childPeg.removeIsChildConstraintAndItsParentConstraint(this);
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
        if (peg.owner != null) {
            return peg.owner;
        }
        return otherSocket.getParentDrivable();
    }

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
