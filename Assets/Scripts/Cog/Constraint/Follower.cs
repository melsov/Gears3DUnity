using UnityEngine;
using System.Collections;

public class Follower : MonoBehaviour {

    public Transform target;
    protected Rigidbody rb;

    public void Awake() {
        rb = GetComponentInChildren<Rigidbody>();
    }

	public void FixedUpdate () {
        if (target == null) { return; }
        if (rb != null) {
            rb.MovePosition(xzPosition());
            return;
        }
        transform.position = xzPosition();
	}

    protected Vector3 xzPosition() {
        return Vector3.Lerp(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z), .7f);
    }
}
