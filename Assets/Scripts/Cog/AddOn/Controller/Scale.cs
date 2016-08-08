using UnityEngine;
using System.Collections;

public class Scale : Switch {

    private float lastMass;
    [SerializeField]
    private float threshhold = 4f;

    //private SpringJoint sj;
    private Spring sj;
    private Rigidbody platformRB;
    private float startPosition;
    private delegate float GetAFloat();
    private GetAFloat getPosition;
    private GetAFloat getGravity;

    protected override void awake() {
        base.awake();
        sj = GetComponentInChildren<Spring>();
        platformRB = sj.GetComponent<Rigidbody>();
        getPosition = delegate () { return platformRB.transform.localPosition.z - sj.connectedBody.transform.localPosition.z; };
        startPosition = getPosition();
        getGravity = delegate () { return Physics.gravity.z; };
        print(getGravity());
        print(sj.spring);
    }

    private float displacement {
        get { return getPosition() - startPosition; }
    }

    private float springForce {
        get { return sj.spring * displacement + sj.damper * platformRB.velocity.z; }
    }

    private float mass {
        get { return springForce / getGravity() - platformRB.mass; }
    }

    private static int test;
    public void FixedUpdate() {
        checkToggle();
        lastMass = mass;
    }

    private void checkToggle() {
        if (lastMass < threshhold != mass > threshhold) { return; }
        toggle();
    }

    protected override void toggle() {
        SwitchState state = mass - lastMass > 0f ? SwitchState.ON : SwitchState.OFF;
        on.setState(state);
    }

}
