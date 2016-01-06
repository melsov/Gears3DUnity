﻿using UnityEngine;
using System.Collections;
using System;

public class LinearActuator : Drivable {
    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    // Linear Actuator never does the connecting itself?
    protected override bool vConnectTo(Collider other) {
        return false;
    }

}