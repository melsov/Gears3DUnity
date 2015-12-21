﻿using UnityEngine;
using System.Collections;

public class BackendSocket : Socket {

    protected override void awake() {
        base.awake();
        _relationshipConstraint = RelationshipConstraint.CAN_ONLY_BE_CHILD;
    }

}
