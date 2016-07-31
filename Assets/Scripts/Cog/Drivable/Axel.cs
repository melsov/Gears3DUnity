﻿using UnityEngine;
using System.Collections;

public class Axel : Peg {

    private AngleStep _angleStep;

    public override RotationMode pegIsParentRotationMode {
        get {
            return RotationMode.FIXED_ONLY;
        }
    }

    public float axisRotation {
        get {
            return transform.rotation.eulerAngles.y;
        }
    }

    public AngleStep angleStep {
        get { return _angleStep; }
    }

    public float angularVelocity {
        get { return _angleStep.angularVelocity(); }
    }

    public float turnTo(float d) {
        transform.eulerAngles = new Vector3(0f, d, 0f);
        _angleStep.update(axisRotation);
        return d;
    }
}

public struct AngleStep
{
    public float deltaAngle {
        get {
            return Angles.adjustedDeltaAngleDegrees(angle_ - lastAngle);
        }
    }

    public float deltaTime {
        get { return timestamp; }
    }

    public float angularVelocity() {
        if (deltaTime > 0f)
            return deltaAngle / deltaTime;
        return 0f;
    }

    private float timestamp;
    private float lastAngle;
    private float angle_;
    public float getAngle() { return angle_; }

    //public AngleStep(float _angle) {
    //    lastAngle = 0f;
    //    timestamp = Time.fixedTime; angle = _angle;
    //}

    public void update(float nextAngle) {
        lastAngle = angle_;
        timestamp = Time.deltaTime;
        angle_ = nextAngle;
    }

    public void debug() {
        MonoBehaviour.print("Angle Step: deltaAngle: " + deltaAngle + " deltaTime: " + deltaTime);
    }

}


