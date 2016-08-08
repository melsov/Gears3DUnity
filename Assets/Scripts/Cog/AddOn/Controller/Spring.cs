using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour {

    [SerializeField]
    public Rigidbody connectedBody;
    protected Rigidbody rb;

    public float spring;
    public float damper;

    private delegate float GetDistance();
    private GetDistance getDistance;

    public void Awake() {
        rb = GetComponent<Rigidbody>();
        getDistance = delegate () { return rb.position.z - connectedBody.position.z; };
    }

    private float force {
        get { return -spring * getDistance() - damper * rb.velocity.z; }
    }
    
    private Vector3 forceV {
        get { return new Vector3(0f, 0f, force); }
    }

    public void FixedUpdate() {
        rb.AddForce(forceV);
    }
}
