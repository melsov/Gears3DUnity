using UnityEngine;
using System.Collections;
using System;

public class Bopper : Drivable {
    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
