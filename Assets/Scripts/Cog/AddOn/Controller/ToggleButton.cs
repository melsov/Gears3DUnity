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

    private float discreteCollisionTimer;
    public float discreteCollisionInterval = .4f;

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
        lineSegment = GetComponentInChildren<LineSegment>();
        button.GetComponent<LinearConstraint>().lineSegment = lineSegment;
        anchor = lineSegment.start; // TEST: replace anchor with start of LS
        restDistance = lineSegment.distance.magnitude; // buttonTravel.magnitude;
	}

    protected bool pressed(Collision collision) {
        float travelScale = Vector3.Dot(collision.impulse.normalized, buttonTravel.normalized);
        travelScale = Mathf.Abs(travelScale); // travelScale insists on being negative in duplicates of toggle button (the game object) but not the original
                                              // this is pretty mysterious. luckily this duct tape does the trick with no downside.
        return travelScale > sensitivity;
    }

    public void proxyCollisionEnter(Collision collision) {
        handleCollision(collision);
    }

    protected void handleCollision(Collision collision) {
        if (Time.fixedTime - discreteCollisionTimer < discreteCollisionInterval) { return; }
        if (shouldTrackPress) {
            if (pressed(collision)) {
                shouldTrackPress = false;
                discreteCollisionTimer = Time.fixedTime;
                toggleOn();
                StartCoroutine(watch());
            }
        }
    }
    protected IEnumerator watch() {
        while(buttonTravel.magnitude < restDistance * .9f) {
            yield return new WaitForFixedUpdate();
        }
        shouldTrackPress = true;
    }

    public void proxyCollisionStay(Collision collision) {
        handleCollision(collision);
    }

    public void proxyCollisionExit(Collision collision) {
    }

    protected bool completedPulse() {
        return Time.fixedTime - pulseTimer > .4f;
    }

	void LateUpdate () {
        //if (!shouldTrackPress) {
        //    // relatively close to rest position?
        //    if(buttonTravel.magnitude > restDistance * .9f) {
        //        shouldTrackPress = true;
        //    }
        //}

        if (isPulseButton) {
            if (completedPulse()) {
                onOffIndicator.state = SwitchState.OFF;
            }
        }
    }

    protected VectorXZ distanceFromHome {
        get { return new VectorXZ(lineSegment.end.position - buttonRB.position); }
    }

    void FixedUpdate() {
        buttonRB.angularVelocity = Vector3.zero;
        //buttonRB.MovePosition(Vector3.Lerp(buttonRB.position, anchor.position, .1f));
        buttonRB.AddForce(distanceFromHome.vector3(0f) * buttonRB.mass * 120f);
        //buttonRB.AddForce(lineSegment.normalized.vector3(0f) * buttonRB.mass * 1200f); // TODO: indicator light // TODO: scale the pushback to distance (or is this not needed)?
    }

}
