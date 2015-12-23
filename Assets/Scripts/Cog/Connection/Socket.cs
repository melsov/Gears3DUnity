using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Socket : MonoBehaviour {

    public virtual RotationMode socketIsChildRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    public virtual RotationMode socketIsParentRotationMode {
        get { return RotationMode.FREE_OR_FIXED; }
    }

    protected RelationshipConstraint _relationshipConstraint = RelationshipConstraint.CAN_BE_CHILD_OR_PARENT;
    public virtual RelationshipConstraint relationshipConstraint {
        get { return _relationshipConstraint; }
    }

    public Axel axel {
        get { return (Axel) drivingPeg; }
    }

    public ISocketSetContainer parentContainer;

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
        parentContainer = GetComponentInParent<ISocketSetContainer>();
        Assert.IsTrue(parentContainer != null);
    }

    public void disconnectDrivingPeg() {
        drivingPeg = null;
    }

    private Peg _childPeg;
    public Peg childPeg {
        get { return _childPeg; } 
        set { _childPeg = value;  }
    }

    public bool hasDrivingPeg() { return drivingPeg != null; }
    public bool hasChildPeg() { return childPeg != null; }
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
}
