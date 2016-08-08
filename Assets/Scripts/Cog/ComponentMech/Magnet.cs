using UnityEngine;
using System.Collections.Generic;
using System;

public class Magnet : MonoBehaviour {

    private delegate void Register(Magnet other);
    private static Register register;

    private HashSet<Magnet> others = new HashSet<Magnet>();
    protected Rigidbody rb;
    [SerializeField]
    protected float power = 1000f;
    [SerializeField]
    protected Transform tNorth;

    protected delegate void BeAMagnet();
    protected BeAMagnet beAMagnet;
    protected BeAMagnet beActive;

    protected bool _active;
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

	public void Awake () {
        beActive =  delegate () {
            foreach (Magnet m in others) {
                m.beInfluencedBy(this);
            }
        };
        beAMagnet = beActive;
        if (!tNorth) {
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
    protected VectorXZ localNorth { get { return posNorth - rb.position; } }
    protected VectorXZ localSouth { get { return posSouth - rb.position; } }
    protected Vector3 north { get { return transform.forward; } }
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
        return Mathf.Min(power / .1f, power / (distance.magnitudeSquared));
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

    private VectorXZ forceDirection(VectorXZ v) {
        return forceAt(v) * v.normalized;
    }

    /* Our torque on m */
    protected Vector3 weightedTorqueLookDirectionAlt(Magnet m) {
        VectorXZ nts = m.northToSouthOf(this);
        VectorXZ stn = -1f * m.southToNorthOf(this);
        VectorXZ ntn = -1f * m.northToNorthOf(this);
        VectorXZ sts = -1f * -1f * m.southToSouthOf(this);

        VectorXZ northTangent = Angles.positiveRotationTangent(m.localNorth);
        //VectorXZ southTangent = Angles.positiveRotationTangent(m.localSouth);

        nts = forceDirection(nts);
        stn = forceDirection(stn);
        ntn = forceDirection(ntn);
        sts = forceDirection(sts);

        nts = northTangent * nts.dot(northTangent);
        //stn = southTangent * stn.dot(southTangent);
        stn = northTangent * stn.dot(northTangent);
        ntn = northTangent * ntn.dot(northTangent);
        //sts = southTangent * sts.dot(southTangent);
        sts = northTangent * sts.dot(northTangent);

        VectorXZ aggregateTangent = nts + stn + ntn + sts;
        VectorXZ result = localNorth + aggregateTangent;
        return result.vector3();
    }

    protected Vector3 altWeightedTorqueLookFrom(Magnet other) {
        return other.weightedTorqueLookDirectionAlt(this);
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

        rb.AddTorque(torque, ForceMode.Impulse);
        rb.AddForce(lin, ForceMode.Force); // ForceMode.Impulse);

    }


	public void FixedUpdate () {
        beAMagnet();
        //foreach(Magnet m in others) {
        //    m.beInfluencedBy(this);
        //    //influence(m);
        //}
	}
}
