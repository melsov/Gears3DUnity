using UnityEngine;
using System.Collections.Generic;
using System;

public class Magnet : MonoBehaviour {

    private delegate void Register(Magnet other);
    private static Register register;

    private HashSet<Magnet> others = new HashSet<Magnet>();
    protected Rigidbody rb;
    [SerializeField]
    protected float _power = 1000f;
    protected float power { get { return _power * multiplier; } }
    public float multiplier = 1f;
    public float getPower() { return power * _reversed * (_active ? 1f : 0f); }
    [SerializeField]
    protected Transform tNorth;

    protected delegate void BeAMagnet();
    protected BeAMagnet beAMagnet;
    protected BeAMagnet beActive;

    public float angularVelocityDamper = 0f;

    protected bool _active = true;
    public bool active {
        get { return _active; }
        set {
            _active = value;
            if (_active) {
                beAMagnet = beActive;
            } else {
                beAMagnet = delegate () { };
            }
        }
    }
    public float _reversed = 1f;
    public float getReversed() { return _reversed; }
    public bool reversed {
        get { return _reversed > 0f; }
        set {
            _reversed = value ? -1f : 1f;
        }
    }

	public virtual void Awake () {
        beActive =  delegate () {
            foreach (Magnet m in others) {
                m.beInfluencedBy(this);
            }
        };
        beAMagnet = beActive;
        if (!tNorth) {
            tNorth = TransformUtil.FindChildWithName(transform, "NorthPole");
            if (!tNorth)
                tNorth = TransformUtil.FindChildWithName(transform, "North");
        }
        UnityEngine.Assertions.Assert.IsTrue(tNorth, "Need a north pole transform: name a child 'North' or assign tNorth in the inspector.");
        rb = GetComponent<Rigidbody>();
        register += addMagnet;
	}

    public void Start() {
        register(this);
    }

    public void OnDestroy() {
        foreach(Magnet m in others) {
            m.removeMagnet(this);
        }
    }

    //private bool nonNegotiating { get { return rb.isKinematic; } }

    private void removeMagnet(Magnet other) { others.Remove(other); }

    private void addMagnet(Magnet other) {
        if (other == this) { return; }
        others.Add(other);
        other.others.Add(this);
    }

    private VectorXZ posNorth { get { return tNorth.position; } }
    private VectorXZ posSouth { get { return rb.position - (tNorth.position - rb.position); } }
//CONSIDER: better to define localNorth with transform.position and not rb.position? (THEORY: values differ because of some update cycle)
    protected VectorXZ localNorth { get { return posNorth - rb.position; } }
    protected VectorXZ localSouth { get { return posSouth - rb.position; } }
    protected Vector3 north { get { return (posNorth - transform.position).normalized.vector3(); } } // transform.forward; } }
    protected Vector3 south { get { return north * -1f; } }

    private float fieldAlignment(Magnet m) {
        return Vector3.Dot(m.north, north);
    }

    private float forceOn(Magnet m) {
        return fieldAlignment(m) * forceAt(distance(m.rb));
    }

    protected float forceFrom(Magnet other) {
        return other.forceOn(this);
    }

    private float forceAt(VectorXZ distance) {
        return power / Mathf.Max(.1f, (distance.magnitudeSquared));
    }

    private static Vector3 invertMagnitude(Vector3 v) { return v / v.sqrMagnitude; }

    protected Vector3 weightedTorqueLookDirection(Magnet m) {
        Vector3 avg = (north + m.north).normalized; 
        float angleInfluence = Vector3.Dot(m.north, avg);
        float powerInfluence = influenceRatio(m);
        float totalInfluence = angleInfluence * powerInfluence;
        Vector3 look = Vector3.Slerp(m.north, avg, totalInfluence);
        return look;
    }

    protected Vector3 weightedTorqueLookFrom(Magnet other) {
        return other.weightedTorqueLookDirection(this);
    }

    private VectorXZ forceDirection(VectorXZ v, Magnet m) {
        return /*fieldAlignment(m) **/ forceAt(v) * v.normalized;
    }

    protected VectorXZ aggregateNorthPoleInfluenceFor(Magnet m) {
        VectorXZ nts = m.northToSouthOf(this);
        VectorXZ stn = -1f * m.southToNorthOf(this); //flip direction to apply correctly to northTangent
        VectorXZ ntn = -1f * m.northToNorthOf(this); //flip to represent a pushing force
        VectorXZ sts = -1f * -1f * m.southToSouthOf(this); //flip twice for both reasons


        nts = forceDirection(nts, m);
        stn = forceDirection(stn, m);
        ntn = forceDirection(ntn, m);
        sts = forceDirection(sts, m);

        VectorXZ aggregate = nts + stn + ntn + sts;
        return aggregate;
    }

    /* This magnet's torque on m */
    protected VectorXZ weightedTorqueLookDirectionAlt(Magnet m) {
        return (localNorth + aggregateNorthPoleInfluenceFor(m)).normalized;
    }

    protected Vector3 altWeightedTorqueLookFrom(Magnet other) {
        return other.weightedTorqueLookDirectionAlt(this).vector3();
    }

//maybe distance "as it applies to my north??? and its north...somehow...TODO: divine the mysteries
    protected Vector3 intoScreenTorqueDirection(Magnet m) {
        return Vector3.Cross(north, distance(m.rb));
    }

    protected Vector3 altTorqueFor(Magnet m) {
        VectorXZ aggregate = aggregateNorthPoleInfluenceFor(m);
        VectorXZ northTangent = Angles.positiveRotationTangent(m.localNorth);
        VectorXZ tangent = northTangent.dot(aggregate) * northTangent;
        VectorXZ vertical = aggregate - tangent;
        return intoScreenTorqueDirection(m) * fieldAlignment(m) * (tangent.magnitude);
    }

    protected Vector3 altTorqueFrom(Magnet other) {
        return other.altTorqueFor(this);
    }

    private float influenceRatio(Magnet m) {
        return power / (power + m.power);
    }
	
    private Vector3 distance(Rigidbody otherRB) {
        return otherRB.position - rb.position;
    }

    protected VectorXZ northToSouthOf(Magnet other) { return other.posSouth - posNorth; }
    protected VectorXZ southToNorthOf(Magnet other) { return other.posNorth - posSouth; }
    protected VectorXZ northToNorthOf(Magnet other) { return other.posNorth - posNorth; }
    protected VectorXZ southToSouthOf(Magnet other) { return other.posSouth - posSouth; }

    private VectorXZ distanceXZ(Rigidbody other) {
        return distance(other);
    }

    /* add torque proportional to a look direction,
     * linear force equal to total force minus
     * tangent implied by torque */
    protected virtual void beInfluencedBy(Magnet other) {
        VectorXZ northToSouth = northToSouthOf(other).normalized * other.forceOn(this);
        VectorXZ weightedLookXZ = other.weightedTorqueLookDirection(this); //WANT ??

        Vector3 torque = Angles.radialVectorsToTorqueXZ(localNorth, weightedLookXZ, rb, Time.fixedDeltaTime);
        VectorXZ tangent = weightedLookXZ - localNorth;
        Vector3 lin = (northToSouth - tangent).vector3() * Time.fixedDeltaTime;

        rb.AddTorque(torque * other._reversed, ForceMode.Impulse);
        rb.AddForce(lin * other._reversed, ForceMode.Force); // ForceMode.Impulse);

    }


	public void FixedUpdate () {
        beAMagnet();
        //foreach(Magnet m in others) {
        //    m.beInfluencedBy(this);
        //    //influence(m);
        //}
	}
}
