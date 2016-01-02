using UnityEngine;
using System.Collections;
using System;

public class Pegboard : Drivable
{
    public override float driveScalar() {
        return 0;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }
}
