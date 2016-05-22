using UnityEngine;
using System.Collections;

public class Follower : MonoBehaviour {

    public Transform target;
    public Vector3 offset = Vector3.zero;
    protected Rigidbody rb;
    public bool ignoreRigidBody;

    public void Awake() {
        if (!ignoreRigidBody) {
            rb = GetComponentInChildren<Rigidbody>();
        }
    }

	public void FixedUpdate () {
        if (target == null) { return; }
        if (rb != null) {
            rb.MovePosition(xzPosition());
            return;
        }
        transform.position = target.position + offset; // xzPosition();
	}

    protected Vector3 xzPosition() {
        return Vector3.Lerp(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z), .7f) + offset;
    }
}
