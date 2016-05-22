using UnityEngine;
using System.Collections;

public class RotationObserver : MonoBehaviour {

    public Transform target;
    public delegate void NotifyRotation(RotationEvent rotationEvent);
    public NotifyRotation notifyRotation;
    
    private int _intervals = 4;
    private bool[] triggers;
    public int intervals {
        get { return _intervals; }
        set {
            _intervals = value;
            triggers = new bool[_intervals];
        }
    }
    private RotationEvent rotationEvent;

    public int axisIndex = 1; // y axis

    private float rotationAngle {
        get { return transform.eulerAngles[axisIndex]; }
    }
    public void Awake() {
        intervals = _intervals;
        if (target == null) {
            target = transform;
        }
        rotationEvent.previousAngleDegrees = rotationEvent.angleDegrees = rotationAngle;
    }

    public void FixedUpdate() {
        rotationEvent.update(rotationAngle);
        checkEvent();
    }

    protected void checkEvent() {
        float wedgeDegrees = (1 /(float) intervals) * 360f;
        for(int i = 0; i < triggers.Length; ++i) {
            float ang = i * wedgeDegrees;
            bool isJumping = rotationEvent.isJumpingAngle(ang);
            bool wasJumping = triggers[i];
            if (!wasJumping && isJumping) {
                if (notifyRotation != null) {
                    notifyRotation(rotationEvent);
                }
            } else if (wasJumping && !isJumping) {
                triggers[i] = false;
            }
        }
    }
}

public struct RotationEvent
{
    public float angleDegrees;
    public float previousAngleDegrees;

    public void update(float angle) {
        previousAngleDegrees = angleDegrees;
        angleDegrees = angle;
    }

    public float sign() {
        return Mathf.Sign(delta());
    }

    public float delta() {
        return Angles.adjustedDeltaAngleDegrees(rawDelta());
    }

    public float rawDelta() {
        return angleDegrees - previousAngleDegrees;
    }

    public bool isJumpingZero() {
        return Mathf.Abs(rawDelta()) > 180f;
    }

    public bool isJumpingAngle(float degrees) {
        if (degrees == 0f ||  Angles.VerySmall(degrees)) { return isJumpingZero(); }
        if (sign() > 0f) {
            return angleDegrees >= degrees && previousAngleDegrees < degrees;
        } else {
            return angleDegrees <= degrees && previousAngleDegrees > degrees;
        }
    }

    public override string ToString() {
        return string.Format("Rotation Event ang: {0} , prev: {1}, delta: {2}", angleDegrees, previousAngleDegrees, delta());
    }
}
