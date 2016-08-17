using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;

public class ToggleButton : Switch , ICollisionProxyClient, ITriggerProxyClient {

    [Tooltip("button with spring joint component")]
    public Transform button;
    protected Transform anchor {
        get { return lineSegment.start; }
    }

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
    protected LinearSpringConstraint linearSpringConstraint;

    protected override void toggle() {
        if (isPulseButton) {
            on.setState(SwitchState.ON);
            pulseTimer = Time.fixedTime;
            //if (onOffIndicator != null) {
            //    onOffIndicator.state = on.getState();
            //}
            updateClient();
            StartCoroutine(pulseOff());
        } else {
            base.toggle();
        }
    }

    private IEnumerator pulseOff() {
        yield return new WaitForSeconds(.4f);
        on.setState(SwitchState.OFF);
    }

    protected Vector3 buttonTravel {
        get { return anchor.position - button.position; }
    }

    protected override void awake() {
        base.awake();
        Assert.IsTrue(button.GetComponent<CollisionProxy>() != null);
        buttonRB = button.GetComponent<Rigidbody>();
        buttonRB.isKinematic = true; // TESTWANT
        Assert.IsTrue(buttonRB.constraints == (RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezePositionX ^ RigidbodyConstraints.FreezePositionZ)); // only x, z pos is not constrained
        lineSegment = GetComponentInChildren<LineSegment>();
        button.GetComponent<LinearConstraint>().lineSegment = lineSegment;
        linearSpringConstraint = button.GetComponent<LinearSpringConstraint>();
        restDistance = lineSegment.distance.magnitude; // buttonTravel.magnitude;
	}

    protected bool pressed(Collision collision) {
        float travelScale = Vector3.Dot(collision.impulse.normalized, buttonTravel.normalized);
        travelScale = Mathf.Abs(travelScale); // travelScale insists on being negative in duplicates of toggle button (the game object) but not the original
                                              // this is pretty mysterious. luckily this duct tape does the trick with no downside.
        return travelScale > sensitivity;
    }


    public void proxyTriggerEnter(Collider other) {
        if (!allowedToPress()) { return; }
        press();
    }

    public void proxyTriggerStay(Collider other) {
    }

    public void proxyTriggerExit(Collider other) {
    }

    public void proxyCollisionEnter(Collision collision) {
        handleCollision(collision);
    }

    protected bool allowedToPress() {
        return shouldTrackPress && Time.fixedTime - discreteCollisionTimer > discreteCollisionInterval;
    }

    protected void handleCollision(Collision collision) {
        return; // ********
        print("got collision");
        if (!allowedToPress()) { return; }
        //if (Time.fixedTime - discreteCollisionTimer < discreteCollisionInterval) { return; }
        //if (shouldTrackPress) {
        //}
        if (pressed(collision)) {
            press();
        }
    }

    protected void press() {
        AudioManager.Instance.play(this, AudioLibrary.ButtonSoundName);
        linearSpringConstraint.pulse();
        shouldTrackPress = false;
        discreteCollisionTimer = Time.fixedTime;
        toggle();
        StartCoroutine(watch());

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

        //if (isPulseButton) {
        //    if (completedPulse()) {
        //        onOffIndicator.state = SwitchState.OFF;
        //    }
        //}
    }


    protected VectorXZ distanceFromHome {
        get { return new VectorXZ(lineSegment.end.position - buttonRB.position); }
    }

    //void FixedUpdate() {
    //    buttonRB.angularVelocity = Vector3.zero;
    //    //buttonRB.AddForce(distanceFromHome.vector3(0f) * buttonRB.mass * 120f);
    //}

}
