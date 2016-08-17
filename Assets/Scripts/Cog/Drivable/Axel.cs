using UnityEngine;
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
        rotate(Quaternion.Euler(Vector3.up * d));
        //transform.eulerAngles = new Vector3(0f, d, 0f);
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

    public float getAngle() { return angle_; }

    private float timestamp;
    private float lastAngle;
    private float angle_;

    public static AngleStep AngleStepWithStartRotation(float startRotation) {
        AngleStep result = new AngleStep();
        result.lastAngle = result.angle_ = startRotation;
        return result;
    }

    public void update(float nextAngle) {
        lastAngle = angle_;
        timestamp = Time.deltaTime; //CONSIDER: unreliable?? (what if we're not updating every frame?)
        angle_ = nextAngle;
    }

    public void debug() {
        MonoBehaviour.print("Angle Step: deltaAngle: " + deltaAngle + " deltaTime: " + deltaTime);
    }

}


