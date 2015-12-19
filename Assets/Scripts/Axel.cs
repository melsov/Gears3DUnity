using UnityEngine;
using System.Collections;

public class Axel : Peg {

    private AngleStep _angleStep;

    public float axisRotation {
        get {
            return transform.rotation.eulerAngles.y;
        }
    }
    private bool _occupied;
    public bool occupied {
        get { return _occupied; } 
        set { _occupied = value; }
    }

    public AngleStep angleStep {
        get { return _angleStep; }
    }

    public float angularVelocity {
        get { return _angleStep.angularVelocity(); }
    }

	void Awake () {
       // _angleStep = new AngleStep(axisRotation);
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
            float delta = angle - lastAngle;
            if (Mathf.Abs(delta) < 180f) { // CONSIDER: limits angVelocity to < 180. Is this OK?
                return delta;
            }
            // Correct deltas where angle has jumped over 360 limit
            return delta + -1f * Mathf.Sign(delta) * 360f;
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
    private float angle;

    public AngleStep(float _angle) {
        lastAngle = 0f;
        timestamp = Time.fixedTime; angle = _angle;
    }

    public void update(float nextAngle) {
        lastAngle = angle;
        timestamp = Time.deltaTime;
        angle = nextAngle;
    }

    public void debug() {
        MonoBehaviour.print("Angle Step: deltaAngle: " + deltaAngle + " deltaTime: " + deltaTime);
    }

}


