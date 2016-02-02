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
        if (getHingeJoint().connectedBody == null) {
            print("hinge: conn body null for: " + gameObject.name);
            return;
        }
        Drivable d = getHingeJoint().connectedBody.GetComponent<Drivable>();
        if (d != null) {
            d.disconnectFromParentHinge();
        }
        getHingeJoint().connectedBody = null;
    }

    public Transform getConnectedBody() {
        return getHingeJoint().connectedBody.transform;
    }
}
