using UnityEngine;
using System.Collections;
using System;

public class FreeRotationPeg : Peg {
    protected override void awake() {
        throw new Exception("don't use free rotation pegs anymore please");
        _pegIsParentRotationMode = RotationMode.FREE_ONLY;
        base.awake();
    }
}
