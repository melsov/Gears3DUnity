using UnityEngine;
using System.Collections;
using System;

public class Electromagnet : Activatable
{
    private Magnet magnet;

    protected override float leverMultiplier {
        get {
            return magnet.multiplier;
        }
        set {
            magnet.multiplier = value;
        }
    }

    protected override void awake() {
        magnet = GetComponentInChildren<Magnet>();
        leverMultiplier = 0f;
        base.awake();
    }

    public override float driveScalar() {
        return 0f;
    }

    public override Drive receiveDrive(Drive drive) {
        return drive;
    }

    protected override void activate(float scalar) {
        magnet.active = scalar != 0f;
        magnet.reversed = scalar < 0f;
        updateIndicator();
    }

    protected override SwitchState getSwitchState() {
        if (!magnet.active) { return SwitchState.OFF; }
        return magnet.reversed ? SwitchState.REVERSE : SwitchState.ON;
    }

    protected override float getPower() {
        return magnet.getPower();
    }
}
