using UnityEngine;
using System.Collections;

public abstract class Constraint : MonoBehaviour {

    public ConstraintTarget constraintTarget;
    protected Rigidbody rb;
    public bool isParentConstraint;
    public Transform systemParent;

    void Awake() {
        awake();
    }
    protected virtual void awake() {
        rb = GetComponent<Rigidbody>();
        rb.drag = 10f;
    }

    void FixedUpdate() {
        fixedUpdate();
    }
    protected void fixedUpdate() {
        if (isParentConstraint) {
            return;
        }
        if (constraintTarget.parentConstraint != null) {
            constraintTarget.parentConstraint.constrain();
        }
        constrain();
    }

    public virtual void removeTarget() {
        constraintTarget.target = null;
    }

    protected abstract void constrain();

    public virtual void configure() {
    }

}

[System.Serializable]
public struct ConstraintTarget
{
    public Transform target;
    public Transform reference;
    public Transform altReference;
    public LineSegment lineSegmentReference;
    public Constraint parentConstraint;

    private Drivable _driverReference;
    public Drivable driverReference {
        get {
            if (_driverReference == null) {
                Socket s = reference.GetComponent<Socket>();
                if (s == null) { MonoBehaviour.print("ref socket null"); return null; }
                Peg dPeg = s.drivingPeg;
                if (dPeg == null) { return null; }
                _driverReference = dPeg.GetComponentInParent<Drivable>();
            }
            return _driverReference;
        }
        set {
            _driverReference = value;
        }
    }
    private Drivable _drivenReference;
    public Drivable drivenReference {
        get {
            if (_drivenReference == null) {
                _drivenReference = target.GetComponent<Peg>().GetComponentInParent<Drivable>();
            }
            return _drivenReference;
        }
        set {
            _drivenReference = value;
        }
    }

    public ConstraintTarget(Transform _target, Transform _reference) {
        target = _target; reference = _reference; parentConstraint = null; altReference = null; lineSegmentReference = null;
        _driverReference = null; _drivenReference = null;
    }
    public ConstraintTarget (Transform _target) {
        target = _target; reference = null; parentConstraint = null; altReference = null; lineSegmentReference = null;
        _driverReference = null; _drivenReference = null;
    }

    public bool isPsuedoNull() {
        return target == null;
    }
}
