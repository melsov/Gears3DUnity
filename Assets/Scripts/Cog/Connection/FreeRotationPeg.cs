using UnityEngine;
using System.Collections;

public class FreeRotationPeg : Peg {
    protected override void awake() {
        _pegIsParentRotationMode = RotationMode.FREE_ONLY;
        base.awake();
    }
}
