using UnityEngine;
using System.Collections;

public class Funnel : MonoBehaviour {

    public float strength = 20f;
    public Vector3 down = new Vector3(0f, -1f, 0f);
    
    void OnTriggerEnter(Collider other) {
        pullToCenter(other);
    }

    void OnTriggerStay(Collider other) {
        pullToCenter(other);
    }

    private void pullToCenter(Collider other) {
        Vector3 towards = transform.position - other.transform.position;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        float dot = Vector3.Dot(towards, transform.rotation * down);
        if (dot > 0f) { 
            rb.velocity = Vector3.Lerp(towards.normalized, rb.velocity.normalized, .5f) * strength * towards.magnitude;
        } else {
            rb.velocity = Vector3.Lerp(transform.rotation * down, rb.velocity.normalized, .5f) * rb.velocity.magnitude * .95f;
        }
    }
}
