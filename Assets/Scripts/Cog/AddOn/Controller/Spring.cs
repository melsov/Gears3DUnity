using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour {

    [SerializeField]
    public Rigidbody connectedBody;
    protected Rigidbody rb;

    [SerializeField]
    protected LineSegment lineSegment;
    [SerializeField]
    protected bool constrainToSegment;

    public float spring;
    public float damper;

    private delegate float GetDistance();
    private GetDistance getDistance;

    private delegate Vector3 GetNormalizedForce();
    private GetNormalizedForce getNormalizedForce;

    public void Awake() {
        rb = GetComponent<Rigidbody>();
        if (lineSegment) {
            getDistance = delegate () { return lineSegment.axisPosition(rb.position); };
            getNormalizedForce = delegate () { return lineSegment.normalized.vector3(); };
        } else {
            getDistance = delegate () { return rb.position.z - connectedBody.position.z; };
            getNormalizedForce = delegate () { return EnvironmentSettings.up; };
        }
    }

    private float force {
        get { return -spring * getDistance(); } 
    }

    private Vector3 velocityInForceDirection {
        get { return Vector3.Dot(rb.velocity, getNormalizedForce()) * getNormalizedForce(); }
    }
    
    private Vector3 forceV {
        get { return getNormalizedForce() * force - damper * velocityInForceDirection; }
    }

    public void FixedUpdate() {
        rb.AddForce(forceV); 
    }
}
