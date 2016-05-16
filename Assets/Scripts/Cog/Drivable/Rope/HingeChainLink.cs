using UnityEngine;
using System;
using System.Collections;

//TODO: arrange to that chain link pegboard meshes are fixed to last links but still are children of pulley (fixed joint)
// <--causes hinge joint wiggles. find another way

public class HingeChainLink : MonoBehaviour , ICogProxy {

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

    public Collider getCollider() { return GetComponentInChildren<Collider>(); }

    private WeakReference _downNeighbor = new WeakReference(null);
    public HingeChainLink downNeighbor {
        get {
            if (_downNeighbor.Target == null) return null;
            return (HingeChainLink)_downNeighbor.Target;
        }
        set { _downNeighbor = new WeakReference(value); }
    }
    public HingeChainLink upNeighbor {
        get {
            if (connectedRigidbody == null) return null;
            return connectedRigidbody.GetComponent<HingeChainLink>();
        }
    }

    protected Rigidbody rb;

    protected CapsuleCollider _capsuleCollider;

	public virtual void Awake() {
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

    public Cog getCog() {
        return rope.GetComponentInParent<Cog>();
    }
}
