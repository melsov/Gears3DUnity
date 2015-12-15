using UnityEngine;
using System.Collections;

public class Axel : MonoBehaviour {

    public Vector3 orientation = Vector3.up;

    private float lastAxisRotation;
    private AngleStep _angleStep;
    private float timestamp;

    public float axisRotation {
        get {
            return transform.rotation.eulerAngles.y;
        }
    }

    public float angularVelocity {
        get { return _angleStep.angularVelocity(); }
    }

	void Awake () {
        _angleStep = new AngleStep(0f, 1f);
        timestamp = Time.fixedTime;
	}
	
	void LateUpdate () {
        lastAxisRotation = axisRotation;
	}

    public float turnTo(float d) {
        lastAxisRotation = axisRotation;
        transform.eulerAngles = new Vector3(0f, d, 0f);

        _angleStep.deltaAngle = d - lastAxisRotation;
        _angleStep.deltaTime = Time.fixedTime - timestamp;
        timestamp = Time.fixedTime;

        return d;
    }
}

public struct AngleStep
{
    public float deltaAngle;
    public float deltaTime;

    public AngleStep(float _deltaAngle, float _deltaTime) {
        deltaAngle = _deltaAngle; deltaTime = _deltaTime;
    }

    public float angularVelocity() { return deltaAngle / deltaTime; }

    public void debug() {
        MonoBehaviour.print("Angle Step: deltaAngle: " + deltaAngle + " deltaTime: " + deltaTime);
    }
}


