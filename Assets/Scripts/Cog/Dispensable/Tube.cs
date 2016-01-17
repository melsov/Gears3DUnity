using UnityEngine;
using System.Collections;

public class Tube : MonoBehaviour {

    public float strength = 300f;

    void OnTriggerEnter(Collider other) {
        //other.GetComponent<Rigidbody>().useGravity = false;
        pullThrough(other);
    }

    void OnTriggerStay(Collider other) {
        //Vector3 destination = transform.position - other.transform.position;
        pullThrough(other);
        //destination.z = -strength;
        //other.GetComponent<Rigidbody>().AddForce(down.normalized * strength, ForceMode.VelocityChange);
    }
    
    void OnTriggerExit(Collider other) {
        //other.GetComponent<Rigidbody>().useGravity = true;
        pullThrough(other);
    }

    private void pullThrough(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        Vector3 towards = transform.position - other.transform.position;
        Vector3 down = transform.rotation * (Vector3.up * -1f);
        VectorXZ normal = new VectorXZ(down).normal;
        Vector3 toCenter = normal.vector3();
        //toCenter.x *= (normal.dot(new VectorXZ(towards)) > 1f ? -1f : 1f);
        rb.velocity = Vector3.Lerp(toCenter, rb.velocity.normalized, .2f) * strength;

        Vector3 dir = Vector3.Lerp(down, rb.velocity.normalized, .2f) * strength;
    }


}
