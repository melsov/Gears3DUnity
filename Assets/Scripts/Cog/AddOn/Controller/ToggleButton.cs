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

    protected Rigidbody rb;
    protected float restDistance;
    protected bool shouldTrackPress = true;
    protected bool trackingPress;

    protected LineSegment lineSegment;

    protected Vector3 buttonTravel {
        get { return anchor.position - button.position; }
    }

    protected override void awake() {
        base.awake();
        Assert.IsTrue(button.GetComponent<CollisionProxy>() != null);
        rb = button.GetComponent<Rigidbody>();
        Assert.IsTrue(rb.constraints == (RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezePositionX ^ RigidbodyConstraints.FreezePositionZ)); // only x, z pos is not constrained
        restDistance = buttonTravel.magnitude;
        lineSegment = GetComponentInChildren<LineSegment>();
        button.GetComponent<LinearConstraint>().lineSegment = lineSegment;
	}
    
    public void proxyCollisionEnter(Collision collision) {
        if (shouldTrackPress) {
            float travelScale =  Vector3.Dot(collision.impulse.normalized, buttonTravel.normalized);
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
	}

    void FixedUpdate() {
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(lineSegment.normalized.vector3(0f) * rb.mass * 120f); // TODO: indicator light // TODO: scale the pushback to distance (or is this not needed)?
    }

}
