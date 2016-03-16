using UnityEngine;
using System;
using System.Collections;

public class ChainLink : MonoBehaviour {

    HingeJoint hj;
    protected WeakReference _rope = new WeakReference(null);
    public Rope rope {
        get {
            if (_rope.Target == null) return null;
            return (Rope)_rope.Target;
        }
        set {
            _rope = new WeakReference(value);
        }
    }
    //protected Gear guide;
    //public bool isGuided {
    //    get { return guide != null; }
    //}
    //private VectorXZ push = VectorXZ.fakeNull;

    //private WeakReference _downNeighbor = new WeakReference(null);
    //public ChainLink downNeighbor {
    //    get {
    //        if (_downNeighbor.Target == null) return null;
    //        return (ChainLink)_downNeighbor.Target; }
    //    set { _downNeighbor = new WeakReference(value); }
    //}
    //private WeakReference _upNeighbor = new WeakReference(null);
    public ChainLink upNeighbor {
        get {
            if (connectedRigidbody == null) return null;
            return connectedRigidbody.GetComponent<ChainLink>();
            //if (_upNeighbor.Target == null) return null;
            //return (ChainLink)_upNeighbor.Target;
        }
        //set { _upNeighbor = new WeakReference(value); }
    }

    protected Rigidbody rb;

    protected CapsuleCollider _capsuleCollider;

	void Awake() {
        hj = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponentInChildren<CapsuleCollider>();
	}

    public float length {
        get {
            return _capsuleCollider.height * _capsuleCollider.transform.localScale.y; } // return 1.05f; }
    }

    public Rigidbody connectedRigidbody {
        get { return hj.connectedBody; }
        set {
            hj.connectedBody = value;
            hj.autoConfigureConnectedAnchor = true;
            Vector3 dif = hj.connectedBody.transform.position - transform.position;
            hj.anchor = new Vector3(dif.x, 0f, dif.z);
        }
    }

    public Rigidbody rigidbod {
        get { return rb; }
    }

    public VectorXZ direction { get { return new VectorXZ(transform.rotation * Vector3.forward); } }

    //void OnCollisionStay(Collision collision) {
    //       Gear gear = findGear(collision);
    //       if (gear == null) { return; }
    //       VectorXZ fromCenter = new VectorXZ(transform.position - gear.transform.position);
    //       if (upNeighbor != null && downNeighbor != null) {
    //           float upDot = fromCenter.dot(upNeighbor.direction);
    //           float downDot = fromCenter.dot(downNeighbor.direction);
    //           if (Mathf.Sign(upDot) != Mathf.Sign(downDot)) {
    //               //get pushed by gear
    //               push = direction * gear.driveScalar(); // fromCenter.normal * gear.driveScalar() * -1f;
    //               pushFalloff = 1f;
    //           }
    //       }
    //   }
    //protected float pushFalloff = 0f;
    void FixedUpdate() {
        if (upNeighbor != null) {
            Vector3 dif = upNeighbor.transform.position - transform.position;
            if (dif.sqrMagnitude > length * length * 1.5f) {
                rb.MovePosition(transform.position + upNeighbor.direction.vector3() * length);
            }
        }
    }

    private Gear findGear(Collision collision) {
        return collision.gameObject.GetComponent<Gear>();
    }

	void Update () {

	}
}
