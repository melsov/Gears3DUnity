using UnityEngine;
using System.Collections;

public class Hinge : MonoBehaviour {

    protected HingeJoint _hingeJoint;
    public HingeJoint getHingeJoint () {
        if (_hingeJoint == null) {
            _hingeJoint = GetComponentInChildren<HingeJoint>();
        }
        return _hingeJoint;
    }

    public void connect(Rigidbody rb) {
        rb.useGravity = true;
        rb.isKinematic = false;
        getHingeJoint().connectedBody = rb;
    }

    public void disconnectObject() {
        getHingeJoint().connectedBody = null;
    }
}
