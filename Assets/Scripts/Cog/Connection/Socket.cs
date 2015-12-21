using UnityEngine;
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
        set { drivingPeg = (Axel) value;  }
    }

    private Peg _drivingPeg;
    public Peg drivingPeg {
        get { return _drivingPeg; }
        set { _drivingPeg = value; }
    }

    void Awake() {
        awake();
    }

    protected virtual void awake() {

    }

    public void disconnectDrivingPeg() {
        if (drivingPeg != null) {
            drivingPeg.disconnectChildSocket();
        }
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
