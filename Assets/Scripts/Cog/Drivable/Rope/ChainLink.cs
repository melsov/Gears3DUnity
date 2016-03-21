using UnityEngine;
using System;
using System.Collections;

public class ChainLink : MonoBehaviour {

    protected Rigidbody rb;
    protected ChainLink _downNeighbor;
    public ChainLink downNeighbor {
        set { _downNeighbor = value; }
    }

    protected WeakReference _upNeighbor = new WeakReference(null);
    public void setUpNeighbor(ChainLink upNeighbor_) {
        _upNeighbor = new WeakReference(upNeighbor_);
    }
    protected ChainLink upNeighbor {
        get {
            if (_upNeighbor.Target == null) return null;
            return (ChainLink)_upNeighbor.Target;
        }
    }
    protected BoxCollider boxCollider;
    protected float _length;
    protected Vector3 halfUp;
    protected Vector3 prevNorthEnd;
    protected Vector3 prevSouthEnd;
    protected Vector3 prevCenter;

    protected Vector3 northEnd {
        get { return transform.position + transform.rotation * halfUp; }
    }
    protected Vector3 southEnd {
        get { return transform.position - transform.rotation * halfUp; }
    }
    public float length {
        get { return _length; }
    }

    void Awake () {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponentInChildren<BoxCollider>();
        _length = boxCollider.size.z * boxCollider.transform.localScale.z;
        halfUp = EnvironmentSettings.up * (_length / 2f);
	}

    private bool physicsFrame;
    void FixedUpdate() {
        moveWithNeighbors(); 

        physicsFrame = true;
    }

    protected void moveWithNeighbors() {
        bool downNeiMoved = _downNeighbor != null && _downNeighbor.northMoved;
        bool upNeiMoved = upNeighbor != null && upNeighbor.southMoved;
        if (didMove && downNeiMoved && upNeiMoved) {
            if (Vector3.Dot(move, upNeighbor.move) > Vector3.Dot(move, _downNeighbor.move)) {
                angleTowardsUp();
            } else {
                angleTowardsDown();
            }
        } else {
            if (downNeiMoved && upNeiMoved) {
                angleTowardsUp(); //arbitrary choice
            } else if (downNeiMoved) {
                angleTowardsDown();
            } else if (upNeiMoved) {
                angleTowardsUp();
            }
        }
    }
    protected void angleTowardsUp() {
        angleTowards(upNeighbor.southEnd, northEnd);
        //Vector3 dif = upNeighbor.southEnd - transform.position;
        //Quaternion ang = Quaternion.Euler(dif);
        //rb.AddTorque(transform.up * ang.eulerAngles.y);
        //rb.AddForce(dif);
        //rb.MoveRotation(ang);
        //rb.MovePosition(transform.position + (upNeighbor.southEnd - northEnd));
    }
    protected void angleTowardsDown() {
        angleTowards(_downNeighbor.northEnd, southEnd);

        //Vector3 dif = _downNeighbor.northEnd - transform.position;
        //rb.MoveRotation(Quaternion.Euler(dif));
        //rb.MovePosition(transform.position + (_downNeighbor.northEnd - southEnd));
    }
    protected void angleTowards(Vector3 otherTarget, Vector3 end) {
        Vector3 dif = otherTarget - transform.position;
        Quaternion ang = Quaternion.FromToRotation(transform.up, dif); 
        rb.MoveRotation(ang); // TODO: re-calculate angle/rotation

        //rb.AddTorque(transform.up * ang.eulerAngles.y);
        //rb.AddForce((new VectorXZ(otherTarget - end)).vector3());
        rb.MovePosition(Vector3.Lerp(transform.position, transform.position + (otherTarget - end), .2f));
    }

    protected bool northMoved;
    protected bool southMoved;
	
    void LateUpdate() {
        if (!physicsFrame) return;
        physicsFrame = false;

        southMoved = !(prevSouthEnd.Equals(southEnd));
        northMoved = !(prevNorthEnd.Equals(northEnd));

        prevNorthEnd = northEnd;
        prevSouthEnd = southEnd;
        prevCenter = transform.position;
    }

    protected Vector3 move {
        get { return transform.position - prevCenter; }
    }

    protected bool didMove {
        get { return move.sqrMagnitude > Mathf.Epsilon; }
    }

	void Update () {

	}
}
