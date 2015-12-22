using UnityEngine;
using System.Collections;

public class Hinge : MonoBehaviour {

    public HingeJoint getHingeJoint () {
        return GetComponentInChildren<HingeJoint>();
    }

    public void connect(Rigidbody rb) {
        rb.useGravity = true;
        rb.isKinematic = false;
        getHingeJoint().connectedBody = rb;
    }

    public void disconnectObject() {
        getHingeJoint().connectedBody = null;
    }

    //public Transform getConnectedObjectColliderTransform() {
    //    HingeJoint hj = getHingeJoint();
    //    Collider collider = hj.connectedBody.gameObject.GetComponentInChildren<Collider>();
    //    if (collider.name == "HingeCollider") {
    //        print("got the hinge collider");
    //        return collider.transform;
    //    }
    //    return null;
    //}
}
