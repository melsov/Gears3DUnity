using UnityEngine;
using System.Collections;

public class FrontendSocket : Socket {
    protected override void awake() {
        base.awake();
        _relationshipConstraint = RigidRelationshipConstraint.CAN_ONLY_BE_PARENT;
    }
}
