using UnityEngine;
using System.Collections;
using System;

public class RotateOffMotor : Motor {

    private MilestoneCounter milestoneCounter = new MilestoneCounter(360f, true);

    protected override void awake() {
        base.awake();
        milestoneCounter.register(onHitMilestone);
    }

    private void onHitMilestone() {
        power = 0f;
    }

    protected override void update() {
        base.update();
        milestoneCounter.updateTotal(angle);
    }
}
