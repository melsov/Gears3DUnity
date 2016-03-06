using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class ToggleButton : Switch , ICollisionProxyClient {

    [Tooltip("button with spring joint component")]
    public Transform button;
    public Transform anchor;

    public float resetSeconds = .2f;
    public float sensitivity = .2f;

    public float bounce = 3f;

    public bool isPulseButton = false;
    private float pulseTimer;

    protected Rigidbody buttonRB;
    protected float restDistance;
    protected bool shouldTrackPress = true;
    protected bool trackingPress;

    protected LineSegment lineSegment;

    protected override void toggleOn() {
        if (isPulseButton) {
            on.setState(SwitchState.ON);
            if (onOffIndicator != null) {
                pulseTimer = Time.fixedTime;
                onOffIndicator.state = on.getState();
            }
            updateClient();
        } else {
            base.toggleOn();
        }
    }

    protected Vector3 buttonTravel {
        get { return anchor.position - button.position; }
    }

    protected override void awake() {
        base.awake();
        Assert.IsTrue(button.GetComponent<CollisionProxy>() != null);
        buttonRB = button.GetComponent<Rigidbody>();
        Assert.IsTrue(buttonRB.constraints == (RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezePositionX ^ RigidbodyConstraints.FreezePositionZ)); // only x, z pos is not constrained
        restDistance = buttonTravel.magnitude;
        lineSegment = GetComponentInChildren<LineSegment>();
        button.GetComponent<LinearConstraint>().lineSegment = lineSegment;
	}

    public void proxyCollisionEnter(Collision collision) {
        if (shouldTrackPress) {
            float travelScale =  Vector3.Dot(collision.impulse.normalized, buttonTravel.normalized);
            travelScale = Mathf.Abs(travelScale); // travelScale insists on being negative in duplicates of toggle button (the game object) but not the original
            // this is pretty mysterious. luckily this duct tape does the trick with no downside.
            if (travelScale > sensitivity) {
                shouldTrackPress = false;
                toggleOn();
            }
        }
    }

    public void proxyCollisionStay(Collision collision) {
    }

    public void proxyCollisionExit(Collision collision) {
    }

	// Update is called once per frame
	void LateUpdate () {
        if (!shouldTrackPress) {
            // relatively close to rest position?
            if(buttonTravel.magnitude > restDistance * .9f) {
                shouldTrackPress = true;
            }
        }

        if (isPulseButton) {
            if (Time.fixedTime - pulseTimer > .4f) {
                onOffIndicator.state = SwitchState.OFF;
            }
        }
    }

    void FixedUpdate() {
        buttonRB.angularVelocity = Vector3.zero;
        buttonRB.AddForce(lineSegment.normalized.vector3(0f) * buttonRB.mass * 120f); // TODO: indicator light // TODO: scale the pushback to distance (or is this not needed)?
    }

}
